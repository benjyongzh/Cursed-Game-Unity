using System.Collections;
//using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(AbilityModifierHolder))]

public class UnitStats : NetworkBehaviour
{
    public string unitName;

    [Header("Resource Parameters")]
    [SyncVar]
    private float currentHealth = 100f;
    [SyncVar]
    private float maxHealth = 100f;
    [SyncVar]
    private float healthRegen = 3f;
    [SyncVar]
    private float healthRegenMultiplier = 1f;
    [SyncVar]
    private bool isAlive = true;

    public Healthbar healthbar;

    [Header("Mobility Parameters")]
    [SyncVar]
    private float moveSpeed = 6f;
    [SyncVar]
    private float minimumMoveSpeed = 2f;
    [SyncVar]
    private float realMoveSpeedMultiplier = 1f;
    [SyncVar]
    private float usedMoveSpeedMultiplier = 1f;
    [SyncVar]
    private float walkSpeed = 1.6f;
    [SyncVar]
    private float minimumWalkSpeed = 0.9f;
    [SyncVar]
    private float realWalkSpeedMultiplier = 1f;
    [SyncVar]
    private float usedWalkSpeedMultiplier = 1f;
    [SyncVar]
    private float crouchSpeed = 1.2f;
    [SyncVar]
    private float minimumCrouchSpeed = 0.6f;
    [SyncVar]
    private float realCrouchSpeedMultiplier = 1f;
    [SyncVar]
    private float usedCrouchSpeedMultiplier = 1f;
    [SyncVar]
    private float jumpForce = 4f;
    [SyncVar]
    private float jumpForceMultiplier = 1f;
    [SyncVar]
    private bool isDashing = false;

    [Header("FOV Parameters")]
    public Transform aimSource;
    public Transform aimTarget;

    GameObject playerManager;
    PostProcessing postProcessing;
    AbilityHolder abilHolder;
    AbilityModifierHolder modHolder;

    public override void OnStartAuthority()//has to apply for both NPC and player gameobjects
    {
        abilHolder = GetComponent<AbilityHolder>();
        modHolder = GetComponent<AbilityModifierHolder>();

        if (gameObject.tag == "Player")
            postProcessing = GetComponent<PostProcessing>();

        if (healthbar)
        {
            healthbar.SetMaxHealth(maxHealth);
            healthbar.SetCurrentHealth(currentHealth);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isAlive)
        {
            PassiveHealthRegeneration();
            HandleMoveSpeedCaps();
        }
    }

    private void PassiveHealthRegeneration()
    {
        if (currentHealth < maxHealth)
        {
            if (currentHealth + (GetHealthRegen() * Time.deltaTime) >= maxHealth)
                SetCurrentHealth(maxHealth);
            else
                SetCurrentHealth(currentHealth + (GetHealthRegen() * Time.deltaTime));
        }
    }

    public GameObject PlayerManager
    {
        get { return playerManager; }
        set { playerManager = value; }
    }

    #region health

    public float GetCurrentHealth() => currentHealth;

    public void SetCurrentHealth(float newHealth)
    {
        currentHealth = newHealth;
        if (healthbar)
            healthbar.SetCurrentHealth(newHealth);
    }

    public float GetMaxHealth() => maxHealth;

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        if (healthbar)
            healthbar.SetMaxHealth(maxHealth);
    }

    public float GetHealthRegen() => (healthRegen * healthRegenMultiplier);

    public void SetHealthRegenMultiplier(float newHealthRegenMultiplier) => healthRegenMultiplier = newHealthRegenMultiplier;

    #endregion

    #region movement

    private void HandleMoveSpeedCaps()
    {
        if (MoveSpeed < MinimumMoveSpeed)
            UsedMoveSpeedMultiplier = (MinimumMoveSpeed / MoveSpeed);
        else
            UsedMoveSpeedMultiplier = RealMoveSpeedMultiplier;
    }

    public float MoveSpeed
    {
        get { return moveSpeed * usedMoveSpeedMultiplier; }
        set { moveSpeed = value; }
    }

    public float UsedMoveSpeedMultiplier
    {
        get { return usedMoveSpeedMultiplier; }
        set { usedMoveSpeedMultiplier = value; }
    }

    public float RealMoveSpeedMultiplier
    {
        get { return realMoveSpeedMultiplier; }
        set { realMoveSpeedMultiplier = value; }
    }

    public float MinimumMoveSpeed { get { return minimumMoveSpeed; } }

    public float WalkSpeed
    {
        get { return walkSpeed * usedWalkSpeedMultiplier; }
        set { walkSpeed = value; }
    }

    public float UsedWalkSpeedMultiplier
    {
        get { return usedWalkSpeedMultiplier; }
        set { usedWalkSpeedMultiplier = value; }
    }

    public float RealWalkSpeedMultiplier
    {
        get { return realWalkSpeedMultiplier; }
        set { realWalkSpeedMultiplier = value; }
    }

    public float MinimumWalkSpeed { get { return minimumWalkSpeed; } }

    public float CrouchSpeed
    {
        get { return crouchSpeed * usedCrouchSpeedMultiplier; }
        set { crouchSpeed = value; }
    }

    public float UsedCrouchSpeedMultiplier
    {
        get { return usedCrouchSpeedMultiplier; }
        set { usedCrouchSpeedMultiplier = value; }
    }

    public float RealCrouchSpeedMultiplier
    {
        get { return realCrouchSpeedMultiplier; }
        set { realCrouchSpeedMultiplier = value; }
    }

    public float MinimumCrouchSpeed { get { return minimumCrouchSpeed; } }


    public float JumpForce
    {
        get { return jumpForce * jumpForceMultiplier; }
        set { jumpForce = value; }
    }

    public float JumpForceMultiplier
    {
        get { return jumpForceMultiplier; }
        set { jumpForceMultiplier = value; }
    }

    public bool IsDashing
    {
        get { return isDashing; }
        set { isDashing = value; }
    }

    #endregion

    #region combat

    public Transform AimSource { get { return aimSource; } }

    public Transform AimTarget { get { return aimTarget; } }

    /*
    public void DealDamageTo(GameObject target, float dmg)
    {
        UnitStats stats = target.GetComponent<UnitStats>();
        if (stats)
            stats.SvrOnTakeDamage(dmg);
    }
    */

    public void OnTakeDamageRange(float minDmg, float maxDmg)
    {
        if (hasAuthority)
        {
            float finalDmg = Random.Range(minDmg, maxDmg);
            CmdOnTakeDamage(finalDmg);

            Debug.Log(gameObject.name + " has taken " + finalDmg + " damage.");
        }
    }

    /*
    public void OnTakeDamagePublic(float Dmg)
    {
        CmdOnTakeDamage(Dmg);
    }
    */

    [Command]
    public void CmdOnTakeDamage(float damage) => SvrOnTakeDamage(damage);

    public void SvrOnTakeDamage(float damage)
    {
        RpcOnTakeDamage();

        if ((currentHealth - damage) > 0)
            SetCurrentHealth(currentHealth - damage);
        else
        {
            SetCurrentHealth(0);
            OnDeath();
        }
        //Debug.Log(gameObject.name + " has taken " + damage + " damage.");
    }

    [ClientRpc]
    private void RpcOnTakeDamage()
    {
        if (hasAuthority && gameObject.tag == "Player")
            postProcessing.VignetteDamage();
    }

    public bool IsAlive { get { return isAlive; } }

    [Server]
    public void OnDeath()
    {
        if (!gameObject)
            return;

        isAlive = false;

        //Ability[] tempAbilArray = abilHolder.TempAbilArray;
        //Array.Clear(tempAbilArray, 0, tempAbilArray.Length);
        abilHolder.TempAbilArray = null;

        for (int i = 0; i < abilHolder.abilities.Count; i++)
        {
            //destroy all ability instances
            Ability abil = abilHolder.abilities[i];
            abilHolder.abilities.Remove(abil);
            //UnityEngine.Object.Destroy(abil);
            Object.Destroy(abil);
        }

        for (int i = 0; i < modHolder.modifiers.Count; i++)
        {
            //destroy all mod instances
            AbilityModifier mod = modHolder.modifiers[i];
            modHolder.modifiers.Remove(mod);
            //UnityEngine.Object.Destroy(mod);
            Object.Destroy(mod);
        }

        //ragdoll
        gameObject.GetComponent<Ragdoll>().ActivateRagdoll();

        //contact playermanager, if its a player
        if (gameObject.tag == "Player")
            playerManager.GetComponent<PlayerManager>().Die();

    }

    #endregion

}
