using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryColliderDamager : MonoBehaviour
{
    float minDamage;
    float maxDamage;
    float duration;
    float height;
    float width;
    LayerMask attackableLayers;
    int maxTargets;
    AbilityModifier[] modifiersOnTargetsAfterAttack;

    float elapsedTime = 0f;

    List<GameObject> victims = new List<GameObject>();

    public void Initialize(float minDmg, float maxDmg, float _duration, float _height, float _width, LayerMask attackLayers, int _maxTargets)
    {
        minDamage = minDmg;
        maxDamage = maxDmg;
        duration = _duration;
        height = _height;
        width = _width;
        attackableLayers = attackLayers;
        maxTargets = _maxTargets;
    }

    public void Initialize(float minDmg, float maxDmg, float _duration, float _height, float _width, LayerMask attackLayers, int _maxTargets, AbilityModifier[] _modifiersOnTargetsAfterAttack)
    {
        minDamage = minDmg;
        maxDamage = maxDmg;
        duration = _duration;
        height = _height;
        width = _width;
        attackableLayers = attackLayers;
        maxTargets = _maxTargets;
        modifiersOnTargetsAfterAttack = _modifiersOnTargetsAfterAttack;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, width / 2);
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, height, 0), width / 2);
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= duration)
        {
            if (this)
                Expire();
                return;
        }


        Collider[] overlaps = new Collider[70];
        int count = Physics.OverlapCapsuleNonAlloc(transform.position, transform.position + new Vector3(0, height, 0), width/2, overlaps, attackableLayers);
        Debug.Log("number of colliders is " + count + ". temporarycolliderdamager:update()");
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
                    if ((_target.GetComponent<UnitStats>() != null) && !victims.Contains(_target.gameObject) && _target.gameObject != gameObject)
                    {
                        DamageTarget(_target.gameObject);
                        Debug.Log("temporarycolliderdamager:DamageTarget()");
                    }
                }
            }
        }
    }

    private void DamageTarget(GameObject target)
    {
        //damage
        UnitStats stats = target.GetComponent<UnitStats>();
        float dmg = Random.Range(minDamage, maxDamage);
        stats.CmdOnTakeDamage(dmg);
        //stats.OnTakeDamageRange(minDamage, maxDamage);
        //Debug.Log(minDamage + " to " + maxDamage + " damage to be taken. temporarycolliderdamager:DamageTarget()");

        //modifiers
        AbilityModifierHolder modholder = target.GetComponent<AbilityModifierHolder>();
        if (modifiersOnTargetsAfterAttack.Length > 0)
            foreach (AbilityModifier mod in modifiersOnTargetsAfterAttack)
                modholder.AddModifier(mod.name);

        victims.Add(target);

        if (victims.Count >= maxTargets)
            Expire();

        Debug.Log(target.name + " has been in contact.");
    }

    void Expire()
    {
        if (GetComponent<UnitStats>().IsDashing)
            GetComponent<UnitStats>().IsDashing = false;


        if (this)
            Destroy(this);
        Debug.Log("temporary collider damage destroyed. Expire()");
    }


}
