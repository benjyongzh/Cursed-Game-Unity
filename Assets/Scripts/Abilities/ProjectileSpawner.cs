using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ProjectileSpawner : NetworkBehaviour
{
    public void SpawnProjectile(GameObject projectile, Transform shooter, Vector3 position, Quaternion rotation, float damage, float velocity, float maxTravelDistance, float maxLifetime, float mass, AbilityModifier[] modifiersOnTargetsAfterAttack, LayerMask triggerLayers, bool targetPenatration)
    {
        if (!isServer)
            return;

        if (!gameObject)
            return;

        GameObject projectileInstance = Instantiate(projectile, position, rotation);
        NetworkServer.Spawn(projectileInstance);

        projectileInstance.GetComponent<ProjectileController>().Initialize(
            shooter,
            damage,
            velocity,
            maxTravelDistance,
            maxLifetime,
            mass,
            modifiersOnTargetsAfterAttack,
            triggerLayers,
            targetPenatration
            );

        Debug.Log("projectile spawned");
    }
}
