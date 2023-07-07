using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Melee")]
public class Melee : Ability
{
    [Header("Attack Parameters")]
    [SerializeField] float minDamage;
    [SerializeField] float maxDamage;
    [SerializeField] float maxRange;
    [SerializeField, Range(5, 90)] float attackAngle;
    [SerializeField] bool multipleTargets = false;
    [Header("Animation Parameters")]
    [SerializeField] float windUpTime;//time taken to swing attack back before hitting target
    [SerializeField] float strikeTime;//time taken to swing attack from back to target
    [SerializeField] float backswingTime;//time taken to swing attack after hitting target
    //[SerializeField] float punchingForce;//force applied to rigidbody if its a killing blow
    [SerializeField] string animatorLayer;//Villager, Defender, Creep1, Ranger, Creep2, etc
    [SerializeField] string animationStateName = "Attacking";//name of animation state that plays the melee animation
    [SerializeField] string animationTransitionTriggerName = "AttackPressed";//animation trigger to get to state
    //[SerializeField] string animationTransitionBoolName = "IsAttacking";//animation trigger to get to state
    [SerializeField] int numberOfAnimationsPossible = 1;//number of different animation poses for this melee attack
    [SerializeField] string attackAnimSpeedParameterName = "AnimSpeed";//Villager, Defender, Creep1, Ranger, Creep2, etc
    [Header("Charging Parameters")]
    [SerializeField] bool TriggerOnRelease;//trigger swing attack only after mouse is let go
    [SerializeField] float minChargeTime;//only starts counting after windUpTime is over
    [SerializeField] float maxChargeTime;//only serves to cap any multiplier effect from charging up attack. does not force attack if mouse is held longer
    [Header("Modifier Parameters")]
    [SerializeField] AbilityModifier[] modifiersOnSelfDuringAttack;
    [SerializeField] AbilityModifier[] modifiersOnSelfAfterAttack;
    [SerializeField] AbilityModifier[] modifiersOnTargetsAfterAttack;

    float elapsedTime = 0f;
    float currentChargeTime = 0;
    float cooldownTimeRemaining = 0;

    bool InCombo = false;
    bool mouseDown = false;

    public override bool IsInComboAnimation()
    { return InCombo;}

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
        
        if (InCombo)
            elapsedTime += Time.deltaTime;

        if (this.currentState == State.Active)
        {
            InCombo = true;

            //check elapsedtime to determine time to trigger ability
            if(elapsedTime >= windUpTime)
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
                            //charging
                            currentChargeTime += Time.deltaTime;
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
        FOVCollider.Distance = maxRange;
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

        //start self modifier
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

            //start anim
            animHandler.TriggerAttackAnim(animatorLayer, animationStateName, animationTransitionTriggerName, numberOfAnimationsPossible);

            if (modifiersOnSelfDuringAttack.Length > 0)
                //start self modifier
                foreach (AbilityModifier mod in modifiersOnSelfDuringAttack)
                    modifierHolder.AddModifier(mod.name);
            //Debug.Log("ability " + AbilityName + " has started.");
        }
        else
        {
            //Debug.Log("ability " + AbilityName + " is already in use.");
        }
            
    }

    public override void TriggerAbility()//called in this script in Update()
    {
        this.currentState = State.OnCooldown;
        cooldownTimeRemaining = this.AbilityCooldown;
        swingState = SwingState.Backswing;

        List<Transform> targets = CheckFOVForTargets();

        if (targets.Count > 0)
            if (TriggerOnRelease)
                //at strike time. apply damage, but with any multipliers
                CycleTargetsInFOV(targets);
            else
                //at strike time. apply damage
                CycleTargetsInFOV(targets);

        FOVCollider.ResetCone();

        //start self modifier
        if (modifiersOnSelfDuringAttack.Length > 0)
            foreach (AbilityModifier mod in modifiersOnSelfDuringAttack)
                for (int i = 0; i < modifierHolder.modifiers.Count; i++)
                    if (mod.name == modifierHolder.modifiers[i].name)
                        modifierHolder.RemoveModifier(modifierHolder.modifiers[i].name);

        //apply modifier on self
        if (modifiersOnSelfAfterAttack.Length > 0)
            foreach (AbilityModifier mod in modifiersOnSelfAfterAttack)
                modifierHolder.AddModifier(mod.name);

        Debug.Log("melee attack triggered. TriggerAbility()");
    }

    private List<Transform> CheckFOVForTargets()
    {
        //make FOVcone and detect enemies in cone. use objectlayers and hitboxes
        if (FOVCollider.TargetsInCone != null)
            return FOVCollider.TargetsInCone;
        return null;
    }

    private void CycleTargetsInFOV(List<Transform> targets)
    {
        //check if multiple targets. check apply modifiers
        if (targets.Count == 1)
            ApplyMeleeEffectsOnTarget(targets[0]);

        else if (multipleTargets)
            foreach (Transform target in targets)
                ApplyMeleeEffectsOnTarget(target);
        else
        {
            //choose the one closest to centre of FOV
            float distance = maxRange;
            Transform finalTarget = targets[0];
            foreach (Transform target in targets)
            {
                float temp_dist;
                temp_dist = Vector3.Distance(target.position, player.position);
                if(temp_dist < distance)
                    finalTarget = target;
            }
            ApplyMeleeEffectsOnTarget(finalTarget);
        }
        //Debug.Log("cycled thru targets. CycleTargetsInFOV()");
    }

    private void ApplyMeleeEffectsOnTarget(Transform target)
    {
        UnitStats targetStats = target.gameObject.GetComponent<UnitStats>();
        targetStats.OnTakeDamageRange(minDamage, maxDamage);

        //apply modifier on targets
        if (modifiersOnTargetsAfterAttack.Length > 0)
        {
            AbilityModifierHolder targetModifierHolder = target.GetComponent<AbilityModifierHolder>();
            foreach (AbilityModifier mod in modifiersOnTargetsAfterAttack)
                targetModifierHolder.AddModifier(mod.name);
        }
        //Debug.Log("applied melee effects on targets. ApplyMeleeEffectsOnTarget()");
    }

    //end of scriptableobject
}
