using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AbilityModifiers/MovespeedSlow")]
public class MovespeedSlow : AbilityModifier
{
    [Header("Movespeed Effect Parameters")]
    [SerializeField, Range(-50, 50)] float initialSlowPercentage;
    [SerializeField, Range(-50, 50)] float intervalSlowPercentage;
    [SerializeField] bool stackingIntervalEffect = false;

    Transform player;
    UnitStats unitStats;
    AbilityModifierHolder modHolder;
    float totalSlowApplied = 0;
    bool modifierHasStopped = false;

    public override void OnCreated(GameObject obj)
    {
        player = obj.transform;
        initialSlowPercentage = initialSlowPercentage / 100;
        intervalSlowPercentage = intervalSlowPercentage / 100;
        unitStats = obj.GetComponent<UnitStats>();
        modHolder = obj.GetComponent<AbilityModifierHolder>();
        this.elapsedTime = 0f;
        this.elapsedTimeInterval = 0f;
        this.stacks = 1;

        ApplyInitialEffect();

        Debug.Log(this.name + " effect proc-ed.");
        //Debug.Log(this.name + " modifier OnCreated()");
    }

    public override void Update()
    {
        if (player == null)
            return;

        this.elapsedTime += Time.deltaTime;
        if (this.elapsedTime >= this.duration)
        {
            StopModifier();
            return;
        }

        if (this.hasIntervalEffect)
        {
            this.elapsedTimeInterval += Time.deltaTime;
            if (this.elapsedTimeInterval >= this.effectInterval)
            {
                ApplyIntervalEffect();
                this.elapsedTimeInterval = 0f;
            }
        }

        //Debug.Log(this.elapsedTime + " seconds have passed. Update()");

    }

    public override void ApplyInitialEffect()
    {
        //depend on stacks?
        unitStats.RealMoveSpeedMultiplier -= (this.stacks * initialSlowPercentage);
        totalSlowApplied += (this.stacks * initialSlowPercentage);
        //Debug.Log(this.name + " modifier applied. ApplyInitialEffect()");
    }

    public override void ApplyIntervalEffect()
    {
        if (stackingIntervalEffect)
        {
            unitStats.RealMoveSpeedMultiplier -= (this.stacks * intervalSlowPercentage);
            totalSlowApplied += (this.stacks * intervalSlowPercentage);
        }
        else
        {
            unitStats.RealMoveSpeedMultiplier -= intervalSlowPercentage;
            totalSlowApplied += intervalSlowPercentage;
        }
        
    }

    public override void OnRefresh()
    {
        this.elapsedTime = 0f;
        if (this.stackable)
        {
            this.stacks += 1;
            unitStats.RealMoveSpeedMultiplier -= initialSlowPercentage;
            totalSlowApplied += initialSlowPercentage;
        }
        //Debug.Log(this.name + " modifier refreshed. OnRefresh().");


    }

    public override void StopModifier() => modHolder.RemoveModifier(this.name);

    public override void Destroy()
    {
        //stop any looping
        if (modifierHasStopped)
            return;
        else
            modifierHasStopped = true;

        unitStats.RealMoveSpeedMultiplier += totalSlowApplied;

        //delete this modifier
        modHolder.modifiers.Remove(this);
        Object.Destroy(this);
        //Debug.Log(this.name + " modifier destroyed. Destroy()");
    }

    
}
