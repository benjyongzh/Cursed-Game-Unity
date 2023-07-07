using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ActivationSelf")]
public class ActivationSelf : Ability
{
    [Header("Activation Parameters")]
    [SerializeField] bool canCastInAir = false;
    [SerializeField] float windUpTime = 0.35f;//time taken to swing attack back before hitting target
    [SerializeField] float strikeTime = 0.1f;//time taken to swing attack from back to target
    [SerializeField] float backswingTime = 0.9f;//time taken to swing attack after hitting target

    [Header("Charging Parameters")]
    [SerializeField] bool charged = false;
    [SerializeField] float minChargeTime = 0.1f;//only used if charged = true
    [SerializeField] float maxChargeTime = 2.0f;//only used if charged = true
    [SerializeField] bool releaseOnFullCharge = false;//only used if charged = true

    [Header("Animation Parameters")]
    [SerializeField] string animatorLayer;//Villager, Defender, Creep1, Ranger, Creep2, etc
    [SerializeField] string animationStateName = "Attacking";//name of animation state that plays the melee animation
    [SerializeField] string animationTransitionTriggerName = "AttackPressed";//animation trigger to get to state
    //[SerializeField] string animationTransitionBoolName = "IsAttacking";//animation trigger to get to state
    [SerializeField] int numberOfAnimationsPossible = 0;//number of different animation poses for this melee attack
    [SerializeField] string attackAnimSpeedParameterName = "AnimSpeed";//Villager, Defender, Creep1, Ranger, Creep2, etc

    [Header("Modifier Parameters")]
    [SerializeField] AbilityModifier[] modifiersOnSelfDuringCharging;//only used if charged = true
    [SerializeField] AbilityModifier[] modifiersOnSelfOnTrigger;

    float elapsedTime = 0f;
    float chargedTime = 0f;
    float cooldownTimeRemaining = 0;

    bool InCombo = false;
    bool mouseDown = false;

    public override bool IsInComboAnimation()
    { return InCombo; }

    SwingState swingState = SwingState.None;
    enum SwingState
    {
        None,
        WindUp,
        Forwardswing,
        Charging,
        Backswing
    }

    Transform player;
    //PlayerPrimaryController playerController;
    PlayerInput playerInput;
    UnitStats unitStats;
    //Transform aimSource;
    //Transform aimTarget;
    AbilityAnimationHandler animHandler;
    AbilityModifierHolder modifierHolder;

    public override void Initialize(GameObject obj)
    {
        player = obj.transform;
        //playerController = obj.GetComponent<PlayerPrimaryController>();
        playerInput = obj.GetComponent<PlayerInput>();
        //aimSource = obj.GetComponent<UnitStats>().AimSource;
        //aimTarget = obj.GetComponent<UnitStats>().AimTarget;
        animHandler = obj.GetComponent<AbilityAnimationHandler>();
        modifierHolder = obj.GetComponent<AbilityModifierHolder>();
    }

    public override void Update()//called by AbilityHolder()
    {
        if (player == null)
            return;

        mouseDown = playerInput.attacking;

        if (InCombo)
            elapsedTime += Time.deltaTime;


        if (this.currentState == State.Active)
        {
            InCombo = true;

            //not charging ability
            if (!charged)
            {
                //check elapsedtime to determine time to trigger ability
                if (elapsedTime >= windUpTime)
                {
                    if (swingState == SwingState.WindUp)
                    {
                        //start forward swinging anim
                        swingState = SwingState.Forwardswing;
                    }
                    else if (swingState == SwingState.Forwardswing)
                        //wait for strikeTime then TriggerAbility()
                        if (elapsedTime >= (windUpTime + strikeTime))
                            TriggerAbility();
                }
            }

            //charging ability, mouse is down
            else
            {
                //in forward swing. doesnt matter if mousedown or not
                if (swingState == SwingState.Forwardswing)
                    if (elapsedTime >= windUpTime + chargedTime + strikeTime)
                        TriggerAbility();

                if (mouseDown)
                {
                    //check elapsedtime to determine time to trigger ability
                    if (elapsedTime >= windUpTime)
                    {
                        //just started charging
                        if (swingState == SwingState.WindUp)
                            swingState = SwingState.Charging;

                        //check if reached minimum charging time
                        else if (swingState == SwingState.Charging)
                        {
                            //in charging state
                            chargedTime += Time.deltaTime;

                            if (chargedTime > minChargeTime)
                            {
                                //add modifierduringcharge if player doesnt have it yet
                                if (modifiersOnSelfDuringCharging.Length > 0)
                                    foreach (AbilityModifier mod in modifiersOnSelfDuringCharging)
                                        if (!modifierHolder.GetModifierOnUnit(mod.name))
                                            modifierHolder.AddModifier(mod.name);

                                //check if reached max charging time && releaseOnMaxCharge
                                if (chargedTime >= maxChargeTime && releaseOnFullCharge)
                                {
                                    //start swinging and remove modifierduringcharge
                                    if (modifiersOnSelfDuringCharging.Length > 0)
                                        foreach (AbilityModifier mod in modifiersOnSelfDuringCharging)
                                            if (modifierHolder.GetModifierOnUnit(mod.name))
                                                modifierHolder.RemoveModifier(mod.name);

                                    //start forwardswing
                                    swingState = SwingState.Forwardswing;
                                }
                                    
                            }
                        }
                    }
                }

                //charging ability, mouse let go
                else
                {
                    //check if reached minimum charging time
                    if (chargedTime > minChargeTime)
                    {
                        //start swinging and remove modifierduringcharge
                        if (modifiersOnSelfDuringCharging.Length > 0)
                            foreach (AbilityModifier mod in modifiersOnSelfDuringCharging)
                                if (modifierHolder.GetModifierOnUnit(mod.name))
                                    modifierHolder.RemoveModifier(mod.name);

                        //start forwardswing
                        swingState = SwingState.Forwardswing;
                    }
                    else
                        //cancel ability
                        CancelAbility();
                }
            }
            


        }

        //cooldown countdown
        else if (this.currentState == State.OnCooldown)
        {
            CheckForEndOfCombo();

            cooldownTimeRemaining -= Time.deltaTime;
            if (cooldownTimeRemaining < 0)
            {
                this.currentState = State.Ready;
                cooldownTimeRemaining = 0;
            }
        }
        else
            //abil is ready. cooldown done but anim still playing?
            CheckForEndOfCombo();

    }

    public override void CancelAbility()
    {
        //anim go back to normal
        swingState = SwingState.None;
        this.currentState = State.Ready;
        elapsedTime = 0;
        chargedTime = 0f;
        InCombo = false;
        animHandler.ResumeAnimation(attackAnimSpeedParameterName);
    }

    private void CheckForEndOfCombo()
    {
        if (
            (!charged && elapsedTime >= (windUpTime + strikeTime + backswingTime))
            ||
            (charged && elapsedTime >= (windUpTime + chargedTime + strikeTime + backswingTime))
            )
        {
            //attack is over, but cooldown not necessarily done. or cooldown could have been done earlier than elapsedTime
            swingState = SwingState.None;
            elapsedTime = 0;
            chargedTime = 0f;
            InCombo = false;
        }
    }

    public override void StartAbility()//called by playerCombat on mousedown
    {
        //mousedown was detected
        if (this.currentState == State.Ready)
        {
            if (!canCastInAir && !player.gameObject.GetComponent<CharacterController>().isGrounded)
                return;

            this.currentState = State.Active;
            InCombo = true;
            swingState = SwingState.WindUp;
            elapsedTime = 0f;
            chargedTime = 0f;

            //start anim
            animHandler.TriggerAttackAnim(animatorLayer, animationStateName, animationTransitionTriggerName, numberOfAnimationsPossible);
        }

    }

    public override void TriggerAbility()//called in this script in Update()
    {
        this.currentState = State.OnCooldown;
        cooldownTimeRemaining = this.AbilityCooldown;
        swingState = SwingState.Backswing;

        //apply self modifiers
        if (modifiersOnSelfOnTrigger.Length > 0)
            foreach (AbilityModifier mod in modifiersOnSelfOnTrigger)
                modifierHolder.AddModifier(mod.name);
        

        Debug.Log("activation ability triggered. TriggerAbility()");
    }
}
