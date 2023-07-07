using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AbilityModifiers/DashModifier")]
public class DashModifier : AbilityModifier
{
    [Header("Dash Movement Parameters")]
    [SerializeField] float velocity = 20f;
    [SerializeField, Range(0, 1f)] float residualVelocityFactor = 0.7f;
    [SerializeField] bool aimBased = false;
    [SerializeField] bool resetHorizontalVelocity = true;
    [SerializeField] bool resetVerticalVelocity = false;

    [Header("Dash Ability Parameters")]
    [SerializeField] Ability[] correspondingAbility;

    [Header("Dash Modifier Parameters")]
    [SerializeField] AbilityModifier[] modifiersOnSelfDuringDash;
    [SerializeField] AbilityModifier[] modifiersOnSelfAfterDash;
    [SerializeField] AbilityModifier[] modifiersOnSelfToRemoveOnExpire;

    Transform player;
    UnitStats unitStats;
    PlayerPrimaryController playerController;
    CharacterController CController;
    PlayerInput playerInput;
    Transform aimSource;
    Transform aimTarget;
    AbilityHolder abilityHolder;
    AbilityModifierHolder modHolder;

    Vector2 inputDirection = Vector2.zero;
    Vector3 moveDirection = Vector3.zero;
    bool dashSuccessEnd = false;
    bool modifierHasStopped = false;

    public override void OnCreated(GameObject obj)
    {
        player = obj.transform;
        unitStats = obj.GetComponent<UnitStats>();
        playerController = obj.GetComponent<PlayerPrimaryController>();
        CController = obj.GetComponent<CharacterController>();
        playerInput = obj.GetComponent<PlayerInput>();
        aimSource = obj.GetComponent<UnitStats>().AimSource;
        aimTarget = obj.GetComponent<UnitStats>().AimTarget;
        abilityHolder = obj.GetComponent<AbilityHolder>();
        modHolder = obj.GetComponent<AbilityModifierHolder>();

        this.elapsedTime = 0f;
        this.elapsedTimeInterval = 0f;
        this.stacks = 1;

        this.AbilityModifierType = "Dash Ability";

        ApplyInitialEffect();

        Debug.Log(this.name + " effect proc-ed.");
    }

    public override void Update()
    {
        if (player == null)
            return;

        this.elapsedTime += Time.deltaTime;
        if (this.elapsedTime >= this.duration)
        {
            dashSuccessEnd = true;
            StopModifier();
            return;
        }

        //movement
        if (resetVerticalVelocity)
        {
            playerController.MoveDirection = new Vector3(
                playerController.MoveDirection.x,
                0,
                playerController.MoveDirection.z);
            playerController.SetImpact(
                new Vector3(
                    playerController.GetImpact().x,
                    0,
                    playerController.GetImpact().z)
                );
        }

        if (resetHorizontalVelocity)
        {
            playerController.MoveDirection = new Vector3(
                0,
                playerController.MoveDirection.y,
                0);
            playerController.SetImpact(
                new Vector3(
                    0,
                    playerController.GetImpact().y,
                    0)
                );
        }

        CController.Move(moveDirection * velocity * Time.deltaTime);
        unitStats.IsDashing = true;
    }

    public override void ApplyInitialEffect()
    {
        //remove all other dash abilities. only 1 dashingmodifier allowed at any point
        //foreach (AbilityModifier mod in modHolder.modifiers)
        //    if (mod.AbilityModifierType == "Dash Ability" && mod != this)
        //        modHolder.RemoveModifier(mod.name);

        if (aimBased)
        {
            //target point based on aiming direction. can be up down too
            moveDirection = (aimTarget.position - aimSource.position).normalized;
        }
        else
        {
            //target point based on current movement input. sticks to ground
            //might encounter problems since its not hasAuthority.
            inputDirection = new Vector2(playerInput.input.x, playerInput.input.y);
            if (inputDirection.magnitude <= 0.1)
            {
                //reset ability
                if (correspondingAbility.Length > 0)
                {
                    foreach (Ability abil in correspondingAbility)
                    {
                        Ability dashAbility = abilityHolder.GetAbility(abil.name);
                        if (dashAbility)
                            dashAbility.CancelAbility();
                    }

                }
                dashSuccessEnd = false;
                StopModifier();
                return;
            }
                
            inputDirection = inputDirection.normalized;
            moveDirection = (player.TransformDirection(Vector3.right) * inputDirection.x) + (player.TransformDirection(Vector3.forward) * inputDirection.y);
        }

        //add other modifiers on self
        if (modifiersOnSelfDuringDash.Length > 0)
        {
            foreach (AbilityModifier mod in modifiersOnSelfDuringDash)
            {
                if (!modHolder.GetModifierOnUnit(mod.name))
                    modHolder.AddModifier(mod.name);
            }
        }
    }

    public override void ApplyIntervalEffect()
    {

    }

    public override void OnRefresh()
    {
        this.elapsedTime = 0f;
        if (this.stackable)
            this.stacks += 1;
        //Debug.Log(this.name + " modifier refreshed. OnRefresh().");


    }

    public override void StopModifier() => modHolder.RemoveModifier(this.name);

    public override void Destroy()
    {
        //check if there are other dash modifiers on unit
        int dashModifiers = 0;
        foreach (AbilityModifier mod in modHolder.modifiers)
            if (mod.AbilityModifierType == this.AbilityModifierType)
                dashModifiers++;
        if (unitStats.IsDashing && dashModifiers <= 1)
            unitStats.IsDashing = false;

        //stop any looping from other modifier's Destroy()
        if (modifierHasStopped)
            return;
        else
            modifierHasStopped = true;

        //remove other modifiers on self
        if (modifiersOnSelfToRemoveOnExpire.Length > 0)
        {
            foreach (AbilityModifier mod in modifiersOnSelfToRemoveOnExpire)
            {
                if (modHolder.GetModifierOnUnit(mod.name))
                    modHolder.RemoveModifier(mod.name);
            }
        }

        //add aftereffect modifiers on successful dashing
        if (dashSuccessEnd)
        {
            if (modifiersOnSelfAfterDash.Length > 0)
            {
                foreach (AbilityModifier mod in modifiersOnSelfAfterDash)
                {
                     modHolder.AddModifier(mod.name);
                }
            }
        }

        playerController.MoveDirection = new Vector3(moveDirection.x * residualVelocityFactor * velocity, playerController.MoveDirection.y, moveDirection.z * residualVelocityFactor * velocity);
        playerController.AddImpact(moveDirection, velocity * residualVelocityFactor * Time.deltaTime);

        //remove this modifier
        modHolder.modifiers.Remove(this);
        Object.Destroy(this);
    }


}
