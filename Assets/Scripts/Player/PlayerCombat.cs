using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

enum CharacterIndexList//for anim
{
    Villager = 0,
    Defender = 1,
    Ranger = 2
}

[RequireComponent(typeof(PlayerInput))]
public class PlayerCombat : NetworkBehaviour
{
    [Header("Combat Parameters")]
    [SyncVar] public bool canAttack = true;
    [SyncVar] public bool canBlock = true;

    [Header("Animation Parameters")]
    [SyncVar] public int CharacterIndex = 0;

    [Header("Aiming Parameters")]
    public Transform aimSource;
    public Transform aimTarget;

    [Header("Blocking Parameters")]
    [SyncVar, Range(0,1f)] public float damageReductionFactor = 0.75f;

    PlayerInput playerInput;
    Animator animator;
    AbilityHolder abilityHolder;

    public Transform AimSource
    { get { return aimSource; } }

    public Transform AimTarget
    { get { return aimTarget; } }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        abilityHolder = GetComponent<AbilityHolder>();
    }

    void Update()
    {
        if (gameObject == null)
            return;

        SetCharacterIndexLayer(true);

        if (!GetComponent<UnitStats>().IsAlive)
            return;

        if (canAttack)
            HandleAttacking();
        else if(abilityHolder.IsAttacking)
            SetIsAttacking(false);

        if (canBlock)
            HandleBlocking();
        else if(abilityHolder.IsBlocking && hasAuthority)
            CmdSetBlocking(false);

    }

    private void SetCharacterIndexLayer(bool activate) //responsible for switching animation layers on and off depending on character type
    {
        if (activate)
        {
            if (CharacterIndex == (int)CharacterIndexList.Villager)
                animator.SetLayerWeight(animator.GetLayerIndex("Villager"), 1f);
        }
        else
        {
            if (CharacterIndex == (int)CharacterIndexList.Villager)
                animator.SetLayerWeight(animator.GetLayerIndex("Villager"), 0f);
        }
    }

    private int GetCharacterLayer() //responsible for switching animation layers on and off depending on character type
    {
        CharacterIndexList charactername = (CharacterIndexList)CharacterIndex;
        for (int i = 0; i < animator.layerCount; i++)
        {
            if (charactername.ToString() == animator.GetLayerName(i))
                return i;
        }
        return 0;
    }

    #region attacking

    private void HandleAttacking()
    {
        if (playerInput.attackpressed && hasAuthority)
            CmdStartCurrentAbility();
    }

    [Command]
    private void CmdStartCurrentAbility() => RpcStartCurrentAbility();

    [ClientRpc]
    private void RpcStartCurrentAbility()
    {
        if (abilityHolder.GetCurrentAbility.AbilityState == Ability.State.Ready)
            abilityHolder.GetCurrentAbility.StartAbility();
    }

    #endregion

    #region blocking

    private void HandleBlocking()
    {
        if (!hasAuthority)
            return;

        if (playerInput.blocking)
            CmdStartBlocking();
        else
            CmdSetBlocking(false);

        animator.SetBool("IsBlocking", abilityHolder.IsBlocking);
    }

    [Command]
    private void CmdStartBlocking()
    {
        if (!abilityHolder.IsBlocking)
            SetIsBlocking(true);
        if (abilityHolder.IsAttacking)
            SetIsAttacking(false);

        RpcStartBlocking();
    }

    [ClientRpc]
    private void RpcStartBlocking()
    {
        //only can block if not in attacking state
        CancelAbility();

        /*
        if (!abilityHolder.IsBlocking)
            SetIsBlocking(true);
        if (abilityHolder.IsAttacking)
            SetIsAttacking(false);
        */
    }

    [Command]
    private void CmdSetBlocking(bool what) => SetIsBlocking(what);

    #endregion

    private void CancelAbility()
    {
        if (abilityHolder.GetCurrentAbility.AbilityState == Ability.State.Active)
        {
            //cancel ability
            abilityHolder.GetCurrentAbility.CancelAbility();
            Debug.Log("ability cancelled");
            //cancel anims
        }
    }

    private void SetIsAttacking(bool what) => abilityHolder.IsAttacking = what;

    private void SetIsBlocking(bool what) => abilityHolder.IsBlocking = what;

    //end of class
}
