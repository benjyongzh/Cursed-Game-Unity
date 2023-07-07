using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ranged")]
public class Ranged : Ability
{
    [Header("Attack Parameters")]
    [SerializeField] float minDamage;
    [SerializeField] bool damageScalesWithChargeTime = false;
    [SerializeField] float maxDamage;
    [SerializeField, Range(0, 20)] float attackAngle = 0f;
    [Header("Projectile Parameters")]
    [SerializeField] float startingVelocity;
    [SerializeField] bool velocityScalesWithChargeTime = false;
    [SerializeField] float maxVelocity;
    [SerializeField] float maxTravelDistance;
    [SerializeField] float maxLifetime;
    [SerializeField] float mass;
    [SerializeField] bool targetPenatration = false;
    [SerializeField] LayerMask triggerLayers;
    [Header("Animation Parameters")]
    [SerializeField] float windUpTime = 0.6f;//time taken to swing attack back before hitting target
    [SerializeField] float strikeTime = 0.1f;//time taken to swing attack from back to target
    [SerializeField] float backswingTime = 0.9f;//time taken to swing attack after hitting target
    //[SerializeField] float punchingForce;//force applied to rigidbody if its a killing blow
    [SerializeField] string animatorLayer;//Villager, Defender, Creep1, Ranger, Creep2, etc
    [SerializeField] string animationStateName = "Attacking";//name of animation state that plays the melee animation
    [SerializeField] string animationTransitionTriggerName = "AttackPressed";//animation trigger to get to state
    //[SerializeField] string animationTransitionBoolName = "IsAttacking";//animation trigger to get to state
    [SerializeField] int numberOfAnimationsPossible = 0;//number of different animation poses for this melee attack
    [SerializeField] string attackAnimSpeedParameterName = "AnimSpeed";//Villager, Defender, Creep1, Ranger, Creep2, etc
    [Header("Charging Parameters")]
    [SerializeField] bool TriggerOnRelease;//trigger swing attack only after mouse is let go
    [SerializeField] float minChargeTime;//only starts counting after windUpTime is over
    [SerializeField] float maxChargeTime;//only serves to cap any multiplier effect from charging up attack. does not force attack if mouse is held longer
    [Header("Modifier Parameters")]
    [SerializeField] AbilityModifier[] modifiersOnSelfDuringAttack;
    [SerializeField] AbilityModifier[] modifiersOnSelfAfterAttack;
    [SerializeField] AbilityModifier[] modifiersOnTargetsAfterAttack;

    public GameObject projectile;

    float elapsedTime = 0f;
    float currentChargeTime = 0;
    float cooldownTimeRemaining = 0;
    float actualDamage;
    float actualVelocity;

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
    PlayerInput playerInput;
    UnitStats unitStats;
    Transform aimSource;
    Transform aimTarget;
    ConeCollider FOVCollider;
    AbilityAnimationHandler animHandler;
    AbilityModifierHolder modifierHolder;

    public override void Initialize(GameObject obj)
    {
        player = obj.transform;
        playerInput = obj.GetComponent<PlayerInput>();
        aimSource = obj.GetComponent<UnitStats>().AimSource;
        aimTarget = obj.GetComponent<UnitStats>().AimTarget;
        FOVCollider = aimSource.GetComponent<ConeCollider>();
        animHandler = obj.GetComponent<AbilityAnimationHandler>();
        modifierHolder = obj.GetComponent<AbilityModifierHolder>();
    }

    public override void Update()//called by AbilityHolder()
    {
        if (player == null)
            return;

        mouseDown = playerInput.attacking;
        //Debug.Log("ability state is " + this.currentState);

        if (InCombo)
            elapsedTime += Time.deltaTime;

        //Debug.Log("elapsed time is " + elapsedTime);
        //Debug.Log("melee state is " + this.currentState);
        //Debug.Log("IsAttacking is " + IsAttacking);
        //Debug.Log("SwingState is " + swingState);

        if (this.currentState == State.Active)
        {
            InCombo = true;

            //check elapsedtime to determine time to trigger ability
            if (elapsedTime >= windUpTime)
            {
                if (TriggerOnRelease)
                {
                    if (mouseDown)
                    {
                        if (swingState == SwingState.WindUp)
                        {
                            //just started charging. can use this space for sfx etc
                            //pause anim for upperbody, for charging effect
                            animHandler.PauseAnimation(attackAnimSpeedParameterName);
                            swingState = SwingState.Charging;
                        }
                        else if (swingState == SwingState.Charging)
                        {
                            //charging
                            currentChargeTime += Time.deltaTime;
                            if (damageScalesWithChargeTime)
                            {
                                if (currentChargeTime < minChargeTime)
                                    actualDamage = minDamage;
                                else if (currentChargeTime > maxChargeTime)
                                    actualDamage = maxDamage;
                                else
                                    actualDamage = minDamage + ((maxDamage - minDamage) * ((currentChargeTime - minChargeTime) / (maxChargeTime - minChargeTime)));
                            }
                            if (velocityScalesWithChargeTime)
                            {
                                if (currentChargeTime < minChargeTime)
                                    actualVelocity = startingVelocity;
                                else if (currentChargeTime > maxChargeTime)
                                    actualVelocity = maxVelocity;
                                else
                                    actualVelocity = startingVelocity + ((maxVelocity - startingVelocity) * ((currentChargeTime - minChargeTime) / (maxChargeTime - minChargeTime)));
                            }

                        }
                            
                    }
                    else if (swingState != SwingState.Forwardswing)
                    {
                        //mouse was just let go
                        if (currentChargeTime >= minChargeTime)
                        {
                            //start forwardswing. can no longer charge up ability
                            animHandler.ResumeAnimation(attackAnimSpeedParameterName);
                            swingState = SwingState.Forwardswing;
                            SetFOVRangeAngle();
                            FOVCollider.CreateCone();
                        }
                        else
                            CancelAbility();
                    }
                    //already in forwardswing. wait for strikeTime then TriggerAbility()
                    else if (elapsedTime >= (windUpTime + currentChargeTime + strikeTime))
                        TriggerAbility();

                }
                else
                {
                    if (swingState == SwingState.WindUp)
                    {
                        //start forward swinging anim
                        swingState = SwingState.Forwardswing;
                        SetFOVRangeAngle();
                        FOVCollider.CreateCone();
                    }
                    else if (swingState == SwingState.Forwardswing)
                        //wait for strikeTime then TriggerAbility()
                        if (elapsedTime >= (windUpTime + strikeTime))
                            TriggerAbility();
                }
            }
            else
            {
                //still winding up attack
                if (TriggerOnRelease && !mouseDown && elapsedTime > 0)
                    CancelAbility();
                //Debug.Log("bla");
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

    private void SetFOVRangeAngle()
    {
        FOVCollider.Angle = attackAngle;
        FOVCollider.Distance = 2f;
        //attackAngle -= 5f;
        //maxRange += 1f;
    }

    public override void CancelAbility()
    {
        //anim go back to normal
        swingState = SwingState.None;
        this.currentState = State.Ready;
        currentChargeTime = 0;
        elapsedTime = 0;
        InCombo = false;
        animHandler.ResumeAnimation(attackAnimSpeedParameterName);

        //stop self modifier
        if (modifiersOnSelfDuringAttack.Length > 0)
            foreach (AbilityModifier mod in modifiersOnSelfDuringAttack)
                for (int i = 0; i < modifierHolder.modifiers.Count; i++)
                    if (mod.name == modifierHolder.modifiers[i].name)
                        modifierHolder.RemoveModifier(modifierHolder.modifiers[i].name);
    }

    private void CheckForEndOfCombo()
    {
        //in the midst of swinging attack
        if (!TriggerOnRelease)
        {
            if (elapsedTime >= (windUpTime + strikeTime + backswingTime))
            {
                //attack is over, but cooldown not necessarily done. or cooldown could have been done earlier than elapsedTime
                swingState = SwingState.None;
                elapsedTime = 0;
                InCombo = false;
            }
        }
        else if (elapsedTime >= (windUpTime + currentChargeTime + strikeTime + backswingTime))
        {
            //attack is over, but cooldown not necessarily done. or cooldown could have been done earlier than elapsedTime
            swingState = SwingState.None;
            elapsedTime = 0;
            InCombo = false;
        }
    }

    public override void StartAbility()//called by playerCombat on mousedown
    {
        //mousedown was detected
        if (this.currentState == State.Ready)
        {
            this.currentState = State.Active;
            InCombo = true;
            swingState = SwingState.WindUp;
            elapsedTime = 0f;
            actualDamage = minDamage;
            actualVelocity = startingVelocity;

            //start anim
            animHandler.TriggerAttackAnim(animatorLayer, animationStateName, animationTransitionTriggerName, numberOfAnimationsPossible);

            if (modifiersOnSelfDuringAttack.Length > 0)
                //start self modifier
                foreach (AbilityModifier mod in modifiersOnSelfDuringAttack)
                    modifierHolder.AddModifier(mod.name);
        }

    }

    public override void TriggerAbility()//called in this script in Update()
    {
        this.currentState = State.OnCooldown;
        cooldownTimeRemaining = this.AbilityCooldown;
        swingState = SwingState.Backswing;

        //stop self modifier
        if (modifiersOnSelfDuringAttack.Length > 0)
            foreach (AbilityModifier mod in modifiersOnSelfDuringAttack)
                for (int i = 0; i < modifierHolder.modifiers.Count; i++)
                    if (mod.name == modifierHolder.modifiers[i].name)
                        modifierHolder.RemoveModifier(modifierHolder.modifiers[i].name);

        //apply modifier on self
        if (modifiersOnSelfAfterAttack.Length > 0)
            foreach (AbilityModifier mod in modifiersOnSelfAfterAttack)
                modifierHolder.AddModifier(mod.name);

        //fire projectile - projspawner should be a singleton?
        ProjectileSpawner projspawner = player.GetComponent<ProjectileSpawner>();
        projspawner.SpawnProjectile(
            projectile,
            player,
            aimSource.position,
            aimSource.rotation,
            actualDamage,
            actualVelocity,
            maxTravelDistance,
            maxLifetime,
            mass,
            modifiersOnTargetsAfterAttack,
            triggerLayers,
            targetPenatration
            );

        FOVCollider.ResetCone();

        Debug.Log("ranged attack triggered. TriggerAbility()");
    }

}
