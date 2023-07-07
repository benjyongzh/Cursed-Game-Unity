using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AbilityHolder : NetworkBehaviour
{
    public List<Ability> abilities = new List<Ability>();
    [SerializeField] Ability currentAbility;
    [SyncVar(hook = nameof(UpdateCurrentAbility))] private int currentAbilityIndex;

    private Ability[] tempAbilArray;
    [SyncVar] private bool IsInAttackingMode = false;
    [SyncVar] private bool IsInBlockingMode = false;

    UnitStats unitstats;
    Animator animator;

    #region references

    public bool IsAttacking
    {
        get { return IsInAttackingMode; }
        set { IsInAttackingMode = value; }
    }
    public bool IsBlocking
    {
        get { return IsInBlockingMode; }
        set { IsInBlockingMode = value; }
    }

    public List<Ability> GetAbilities { get { return abilities; } }

    public Ability GetCurrentAbility { get { return currentAbility; } }

    public int GetCurrentAbilityIndex { get { return currentAbilityIndex; } }

    public Ability[] TempAbilArray
    {
        get { return tempAbilArray; }
        set { tempAbilArray = value; }
    }

    public Ability GetAbility(string abilityName)
    {
        foreach (Ability abil in abilities)
        {
            if (abil.name == abilityName)
                return abil;
        }
        return null;
    }

    #endregion

    private void Awake()
    {
        unitstats = GetComponent<UnitStats>();
        animator = GetComponent<Animator>();

        //check if unitName is valid
        UnitSettings database = MasterManager.UnitSettings;
        if (!database.AllUnitNames.Contains(unitstats.unitName))
            return;

        //create a temporary array of abilities meant to be added to unit. only for referencing
        tempAbilArray = Resources.LoadAll<Ability>("Abilities/" + unitstats.unitName);
        if (tempAbilArray.Length < 1)
            return;

        //cycle thru this temp array
        for (int i = 0; i < tempAbilArray.Length; i++)
            //check for the climbing value of abilityindex in these abilities
            foreach (Ability abil in tempAbilArray)
                //add the first ability come across as having the correct abilityindex
                if (abil.AbilityIndex == i)
                {
                    //instantiate abilities in character folder into abilityholder
                    Ability newAbil = Object.Instantiate(abil);
                    abilities.Add(newAbil);
                    break;
                }

        //set current ability
        if (abilities.Count > 0)
        {
            currentAbilityIndex = 0;
            SetCurrentAbilityByIndex(currentAbilityIndex);
        }
    }

    public void SetCurrentAbilityByIndex(int _abilityIndex)
    {
        currentAbility = abilities[_abilityIndex];
        currentAbilityIndex = _abilityIndex;
    }

    private void Start()
    {      
        foreach (Ability abil in abilities)
            abil.Initialize(gameObject);
    }

    private void Update()
    {
        if (gameObject == null)
            return;

        IsInAttackingMode = false;
        foreach (Ability abil in abilities)
            if (abil.IsInComboAnimation())
            {
                //already in attacking mode
                IsInAttackingMode = true;
                break;
            }

        foreach (Ability abil in abilities)
            abil.Update();

        if (hasAuthority)
        {
            animator.SetBool("IsAttacking", IsAttacking);

            //choosing ability
            if (gameObject.tag == "Player")
            {
                PlayerInput playerInput = GetComponent<PlayerInput>();
                if (playerInput.Ability1Selected)
                    CmdHandleAbilityChanging(0);
                if (playerInput.Ability2Selected)
                    CmdHandleAbilityChanging(1);
                if (playerInput.Ability3Selected)
                    CmdHandleAbilityChanging(2);
                if (playerInput.Ability4Selected)
                    CmdHandleAbilityChanging(3);
                if (playerInput.Ability5Selected)
                    CmdHandleAbilityChanging(4);
            }
        }


    }

    [Command]
    private void CmdHandleAbilityChanging(int selectedAbility)
    {
        if (abilities.Count > selectedAbility)
            if (currentAbilityIndex != selectedAbility)
                currentAbilityIndex = selectedAbility;
    }

    private void UpdateCurrentAbility(int oldAbilIndex, int newAbilIndex)
    {
        currentAbility = abilities[newAbilIndex];
        Debug.Log("ability changed to " + currentAbility.name);
    }

}
