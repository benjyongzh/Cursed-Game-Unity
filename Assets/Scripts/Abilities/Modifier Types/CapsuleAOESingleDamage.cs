using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AbilityModifiers/CapsuleAOESingleDamage")]
public class CapsuleAOESingleDamage : AbilityModifier
{
    [Header("Collision Parameters")]
    [SerializeField] float minDamage;
    [SerializeField] float maxDamage;
    [SerializeField] bool useCharacterHeight;
    [SerializeField] float height;
    [SerializeField] float width;

    [Header("Target Parameters")]
    [SerializeField] LayerMask targetLayers;
    [SerializeField, Range(1, 20)] int maxTargets;
    [SerializeField] AbilityModifier[] modifiersOnTargets;

    [Header("Other Modifier Parameters")]
    [SerializeField] AbilityModifier[] modifiersOnSelfToAddOnExpire;
    [SerializeField] AbilityModifier[] modifiersOnSelfToRemoveOnExpire;

    Transform player;
    PlayerPrimaryController playerController;
    UnitStats unitStats;
    AbilityModifierHolder modHolder;

    List<GameObject> victims = new List<GameObject>();
    bool modifierHasStopped = false;

    public override void OnCreated(GameObject obj)
    {
        player = obj.transform;
        playerController = obj.GetComponent<PlayerPrimaryController>();
        unitStats = obj.GetComponent<UnitStats>();
        modHolder = obj.GetComponent<AbilityModifierHolder>();
        this.elapsedTime = 0f;
        this.elapsedTimeInterval = 0f;
        this.stacks = 1;

        ApplyInitialEffect();

        //Debug.Log(this.name + " effect proc-ed.");
    }

    public override void Update()
    {
        if (player == null)
            return;

        this.elapsedTime += Time.deltaTime;
        if (this.elapsedTime >= this.duration || victims.Count >= maxTargets)
        {
            //make sure dash modifier is successful
            StopModifier();
            return;
        }

        Collider[] overlaps = new Collider[70];
        int count = Physics.OverlapCapsuleNonAlloc(player.position, player.position + new Vector3(0, height, 0), width / 2, overlaps, targetLayers);
        Debug.Log("number of colliders is " + count + ". capsuleAOEsingledamage:update()");
        if (count > 0)
        {
            foreach (Collider collider in overlaps)
            {
                //make sure it isnt null. it shouldnt be
                if (collider != null)
                {
                    Transform _target = collider.transform;
                    //cycle upwards to find ultimate grandparent of hitbox
                    while (_target.transform.parent)
                        _target = _target.transform.parent;

                    //if the grandparent has UnitStats, and isnt already in the targets list, add it
                    if ((_target.GetComponent<UnitStats>() != null) && !victims.Contains(_target.gameObject) && _target.gameObject != player.gameObject)
                    {
                        DamageTarget(_target.gameObject);
                        Debug.Log("capsuleAOEsingledamage:DamageTarget()");
                    }
                }
            }
        }


    }

    private void DamageTarget(GameObject target)
    {
        //damage
        UnitStats stats = target.GetComponent<UnitStats>();
        stats.OnTakeDamageRange(minDamage, maxDamage);
        //Debug.Log(minDamage + " to " + maxDamage + " damage to be taken. temporarycolliderdamager:DamageTarget()");

        //modifiers
        AbilityModifierHolder modholder = target.GetComponent<AbilityModifierHolder>();
        if (modifiersOnTargets.Length > 0)
            foreach (AbilityModifier mod in modifiersOnTargets)
                modholder.AddModifier(mod.name);

        victims.Add(target);

        //Debug.Log(target.name + " has been in contact.");
    }

    public override void ApplyInitialEffect()
    {
        if (useCharacterHeight)
            height = playerController.GetStandingHeight();

        victims = new List<GameObject>();
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
        //stop any looping from any other modifier's Destroy()
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

        //add other modifiers on self
        if (modifiersOnSelfToAddOnExpire.Length > 0)
        {
            foreach (AbilityModifier mod in modifiersOnSelfToAddOnExpire)
            {
                if (!modHolder.GetModifierOnUnit(mod.name))
                    modHolder.AddModifier(mod.name);
            }
        }

        //delete this modifier
        modHolder.modifiers.Remove(this);
        Object.Destroy(this);
        //Debug.Log(this.name + " modifier destroyed. Destroy()");
    }


}
