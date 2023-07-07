using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(UnitStats))]
public class PlayerPrimaryController : NetworkBehaviour
{
    //used for cutscenes/deathscenes etc where player cant control character

    [Header("Movement Parameters")]
    public bool canMove = true;
    private float correctSpeed;
    [SerializeField, Range(0.1f, 0.5f)] float slidingMinimumFactor = 0.2f;
    [SerializeField, Range(0.1f, 50f)] float groundFrictionFactor = 50f;
    [SerializeField, Range(0.1f, 5f)] float airFrictionFactor = 5f;

    [Header("Jumping Parameters")]
    [SerializeField] float gravity = -9.81f;

    [Header("Crouch Parameters")]
    [SerializeField] float crouchHeight = 1.2f;
    [SerializeField] float standingHeight = 1.8f;
    [SerializeField] float timeToCrouch = 0.3f;
    private Vector3 crouchingCenter = Vector3.zero;
    private Vector3 standingCenter = Vector3.zero;
    
    private bool isCrouching = false;
    private bool isInAir = false; //used by HandleAnimations

    [HideInInspector]
    public bool isStationary = false; //used by PlayerAnimations.cs and HandleAnimations
    private bool duringCrouchAnim = false;

    [Header("Look Parameters")]
    public bool canLook = true;
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 100)] private float upperLookLimit = 75.0f;
    [SerializeField, Range(1, 100)] private float lowerLookLimit = 75.0f;
    float rotationX;
    float rotationY;

    [SerializeField] Transform playerCameraSocket;
    CharacterController characterController;
    PlayerInput playerInput;
    UnitStats unitStats;
    Animator animator;
    [SerializeField] GameObject playerUI;

    //for ccmove only
    Vector2 inputDirection = Vector2.zero;
    Vector3 moveDirection = Vector3.zero;
    Vector3 slidingDirection = Vector3.zero;

    public float GetStandingHeight() => standingHeight;

    public Vector3 MoveDirection
    {
        get { return moveDirection; }
        set { moveDirection = value; }
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        unitStats = GetComponent<UnitStats>();
        animator = GetComponent<Animator>();
        if (hasAuthority)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerUI.SetActive(true);
            //Destroy(playerUI);
        }
    }

    void Update()
    {
        if (!hasAuthority)
            return;

        if (canLook && !unitStats.IsDashing)
            ControlMouseLook();

        if (canMove && !unitStats.IsDashing)
        {
            HandleAnimations();

            ControlMovement();
            ControlJump();
            ControlCrouch();
        }

        HandleImpactMovement();
    }


    private void FixedUpdate()
    {
        if (!hasAuthority)
            return;

        if (canMove)
            ApplyFinalMovements();
    }

    public bool IsStationary() => isStationary;

    private void ControlMovement()
    {
        //Check walking and crouching
        if (playerInput.walk)
            if (playerInput.crouch)
                correctSpeed = unitStats.CrouchSpeed;
            else
                correctSpeed = unitStats.WalkSpeed;
        else if (playerInput.crouch)
            correctSpeed = unitStats.CrouchSpeed;
        else
            correctSpeed = unitStats.MoveSpeed;
            
        float moveDirectionY = moveDirection.y;
        if (characterController.isGrounded)
        {
            //was already on ground. normal movement
            inputDirection = new Vector2(correctSpeed * playerInput.input.x, correctSpeed * playerInput.input.y);
            moveDirection = (transform.TransformDirection(Vector3.right) * inputDirection.x) + (transform.TransformDirection(Vector3.forward) * inputDirection.y);
        }
        else
        {
            //while in air, give slight control only
            inputDirection = new Vector2(correctSpeed * playerInput.raw.x, correctSpeed * playerInput.raw.y);
            var temp_vector = (transform.TransformDirection(Vector3.right) * inputDirection.x) + (transform.TransformDirection(Vector3.forward) * inputDirection.y);
            moveDirection.x += temp_vector.x * Time.deltaTime;
            moveDirection.z += temp_vector.z * Time.deltaTime;
        }
            
        moveDirection.y = moveDirectionY;
    }

    private void ControlMouseLook()
    {
        rotationX -= playerInput.mouse.y * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        rotationY += playerInput.mouse.x * lookSpeedX;

        /*
        if (isStationary)
        {
            //only camerasocket rotates with player mouse input
            playerCameraSocket.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        }
        else
        {
        //camerasocket rotates with vertical player mouse input, but horizontal mouse input affects entire player object
        }
        */

        if (unitStats.IsAlive)
        {
            playerCameraSocket.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, playerInput.mouse.x * lookSpeedX, 0);
        }
        else
            playerCameraSocket.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);

    }

    private void ControlJump()
    {
        if (characterController.isGrounded && playerInput.jump)
        {
            moveDirection.y = unitStats.JumpForce;
            float randomTestValue = Random.Range(5, 8);
            unitStats.CmdOnTakeDamage(randomTestValue);
            //Debug.Log("player took " + randomTestValue + " damage.");
        }
    }

    private void ControlCrouch()
    {
        if (!duringCrouchAnim)
        {
            if (!isCrouching && characterController.isGrounded && playerInput.crouch)
            {
                crouchingCenter = new Vector3(0, crouchHeight/2, 0);
                standingCenter = new Vector3(0, standingHeight/2, 0);
                StartCoroutine(CrouchStand());
                //animation start crouching
                animator.SetBool("IsCrouching", true);
            }
            if (isCrouching && !playerInput.crouch)
            {
                if (!Physics.Raycast(playerCameraSocket.transform.position, Vector3.up, 0.3f + standingHeight-crouchHeight))
                {
                    crouchingCenter = new Vector3(0, crouchHeight/2, 0);
                    standingCenter = new Vector3(0, standingHeight/2, 0);
                    StartCoroutine(CrouchStand());
                    //animator stop crouching
                    animator.SetBool("IsCrouching", false);
                }
                
            }
        }

        //make sure camera socket follows crouching and standing height
        playerCameraSocket.transform.localPosition = new Vector3(0, (characterController.height - 0.1f), 0);
    }

    private IEnumerator CrouchStand()
    {
        duringCrouchAnim = true;
        float timeElapsed = 0f;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;
        
        while(timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed/timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed/timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;
        isCrouching = !isCrouching;

        duringCrouchAnim = false;
    }

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
            //in air
            moveDirection.y += gravity * Time.deltaTime;
        //else
        //    moveDirection += slidingDirection;

        Vector3 finalMove = moveDirection + slidingDirection;
        //moveDirection += slidingDirection;

        characterController.Move(finalMove * Time.deltaTime);
    }

    private void HandleAnimations()
    {
        //jumping anim
        if (!characterController.isGrounded)
        {
            //just lifted off ground
            if (!isInAir)
            {
                animator.SetBool("IsCrouching", false);
                animator.SetBool("IsJumping", true);
                isInAir = true;
            }
            if (isStationary)
                isStationary = false;

        }
        else
        {
            //on ground
            animator.SetFloat("Velocity X", inputDirection.x/6);
            animator.SetFloat("Velocity Z", inputDirection.y/6);

            //just landed on ground
            if (isInAir)
            {
                animator.SetBool("IsJumping", false);
                isInAir = false;
                //apply landingslow modifier
                CmdAddLandingModifier();

                //impact of landing
                RemoveImpact();
                AddImpact(new Vector3(moveDirection.x, 0, moveDirection.z), 1);
            }

            //player is not moving on ground
            if (inputDirection.magnitude < 0.1f)
                //player just stopped moving
                if (!isStationary)
                    //make player camera socket facing = horizontal facing of player object
                    //playerCameraSocket.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
                    isStationary = true;
            else
                //player just started moving
                if (isStationary)
                    //make player object facing = horizontal facing of camera socket
                    //transform.rotation = Quaternion.Euler(0, playerCameraSocket.transform.rotation.y, 0);
                    //playerCameraSocket.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
                    isStationary = false;
        }

        //crouching anim done by ControlCrouch()

    }

    [Command]
    private void CmdAddLandingModifier()
    {
        AbilityModifierHolder modholder = GetComponent<AbilityModifierHolder>();
        modholder.AddModifier("LandingSlow");
    }

    public Vector2 GetInputDirection() => inputDirection;

    public GameObject PlayerUI { get { return playerUI; } }


    #region impact

    public void AddImpact(Vector3 direction, float acceleration) => slidingDirection += direction.normalized * acceleration;

    public void SetImpact(Vector3 newDirection)
    {
        slidingDirection = new Vector3(newDirection.x, newDirection.y, newDirection.z);
    }

    public Vector3 GetImpact() => slidingDirection;

    public void RemoveImpact() => slidingDirection = Vector3.zero;

    private void HandleImpactMovement()
    {
        if (slidingDirection.magnitude > slidingMinimumFactor)
        {
            if (characterController.isGrounded)
                slidingDirection = Vector3.Lerp(slidingDirection, Vector3.zero, groundFrictionFactor * Time.deltaTime);
            else
                slidingDirection = Vector3.Lerp(slidingDirection, Vector3.zero, airFrictionFactor * Time.deltaTime);
        }
        else if (slidingDirection != Vector3.zero)
            RemoveImpact();

        //Debug.Log(slidingDirection);
    }

    #endregion



    //end of class
}