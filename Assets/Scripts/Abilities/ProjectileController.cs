using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ProjectileController : NetworkBehaviour
{
    float damage;
    float velocity;
    float maxTravelDistance;
    float maxLifetime;
    float mass;
    AbilityModifier[] modifiersOnTargetsAfterAttack;
    LayerMask triggerLayers;
    bool targetPenatration;

    [Header("Trigger Parameters")]
    [SerializeField] bool destroyOnImpact = true;
    [SerializeField] float AOERadius = 0f;
    [SerializeField] bool AOEEffect = false;

    [SyncVar] float elapsedTime = 0f;
    [SyncVar] float distanceTravelled = 0f;
    Vector3 prevPos;

    Rigidbody rb;
    GameObject target;
    Transform shooter;

    public void Initialize(Transform _shooter, float _damage, float _velocity, float _maxTravelDistance, float _maxLifetime, float _mass, AbilityModifier[] _modifiersOnTargetsAfterAttack, LayerMask _triggerLayers, bool _targetPenatration)
    {
        shooter = _shooter;
        damage = _damage;
        velocity = _velocity;
        maxTravelDistance = _maxTravelDistance;
        maxLifetime = _maxLifetime;
        mass = _mass;
        modifiersOnTargetsAfterAttack = _modifiersOnTargetsAfterAttack;
        triggerLayers = _triggerLayers;
        targetPenatration = _targetPenatration;
    }

    public override void OnStartServer()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;

        rb.AddForce(transform.forward * velocity);
    }

    [Server]
    void Update()
    {
        if (elapsedTime >= maxLifetime || distanceTravelled >= maxTravelDistance)
        {
            //destroy item
            Expire();
            return;
        }

        elapsedTime += Time.deltaTime;
        distanceTravelled += Vector3.Distance(transform.position, prevPos);
        prevPos = transform.position;
        Debug.Log("Projectile elapsedTime is " + elapsedTime + " seconds. ProjectileController: Update()");
        Debug.Log("Projectile distanceTravelled is " + distanceTravelled + " metres. ProjectileController: Update()");

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer)
            return;

        Transform culprit = collision.transform;
        while (culprit.transform.parent)
            culprit = culprit.transform.parent;

        if (culprit == shooter)
            return;

        if (triggerLayers == (triggerLayers | (1 << culprit.gameObject.layer)))
        {
            //triggeringlayer is detected

            target = culprit.gameObject;
            TriggerImpactEffect();
        }
    }

    private void Expire()
    {
        if (isServer)
            NetworkServer.Destroy(gameObject);
        //sfx?
        Debug.Log("projectile is destroyed. ProjectileController: Update()");
    }

    private void TriggerImpactEffect()
    {
        if (AOEEffect)
        {
            //multiple targets
            Collider[] overlaps = new Collider[30];
            int count = Physics.OverlapSphereNonAlloc(transform.position, AOERadius, overlaps, triggerLayers);
            //check if there are any targets hitboxes at all
            if (count > 0)
            {
                //create empty list to store all targets in AOE
                List<GameObject> targets = new List<GameObject>();

                //cycle thru all colliders in AOE. will contain ragdoll hitboxes
                for (int i = 0; i < count + 1; i++)
                    //make sure it isnt null. it shouldnt be
                    if (overlaps[i] != null)
                    {
                        Transform _target = overlaps[i].transform;
                        //cycle upwards to find ultimate grandparent of hitbox
                        while (_target.transform.parent)
                            _target = _target.transform.parent;

                        //if the grandparent has UnitStats, and isnt already in the targets list, add it
                        if ((_target.GetComponent<UnitStats>() != null) && !targets.Contains(_target.gameObject))
                            targets.Add(_target.gameObject);
                    }

                //cycle thru targets list and ApplyEffect()
                for (int j = 0; j < targets.Count; j++)
                    ApplyEffect(targets[j]);
            }
        }
        else if (target.GetComponent<UnitStats>() != null)
            ApplyEffect(target);

        if (destroyOnImpact)
            Expire();

        Debug.Log("projectile is impacted. ProjectileController: TriggerImpactEffect()");
    }

    private void ApplyEffect(GameObject _target)
    {
        if (!isServer)
            return;

        //damage
        UnitStats stats = _target.GetComponent<UnitStats>();
        stats.SvrOnTakeDamage(damage);

        //modifiers
        AbilityModifierHolder modholder = _target.GetComponent<AbilityModifierHolder>();
        if (modifiersOnTargetsAfterAttack.Length > 0)
            foreach (AbilityModifier mod in modifiersOnTargetsAfterAttack)
                modholder.AddModifier(mod.name);
    }
}
