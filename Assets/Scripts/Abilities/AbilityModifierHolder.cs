using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AbilityModifierHolder : NetworkBehaviour
{
    public List<AbilityModifier> modifiers = new List<AbilityModifier>();

    private void Update()
    {
        if (gameObject == null)
            return;

        if (modifiers.Count > 0)
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                modifiers[i].Update();
            }
        }
    }

    [Command]
    public void CmdAddModifier(string modifierName) => AddModifier(modifierName);

    public void AddModifier(string modifierName)//using chance, so therefore use server
    {
        if (!isServer)
            return;

        //establish chance value
        float rndm = Random.Range(0, 100f);
        Debug.Log("Chance to add/refresh modifier " + modifierName + " effect is " + rndm + ". AddModifier().");

        //search modifier database
        AbilityModifierDatabase database = AbilityModifierDatabase.instance;
        foreach (AbilityModifier mod in database.AllModifiers)
        {
            //this is the modifier to add/refresh
            if (mod.name == modifierName)
            {
                Debug.Log(mod.ProcChancePercentage + " was required. AddModifier().");
                if (rndm < mod.ProcChancePercentage)
                {
                    //proc-ed
                    RpcAddModifier(modifierName);
                    Debug.Log(modifierName + " was procced. RpcAddModifier() should run. AddModifier().");
                }
                return;
            }
        }
    }

    [Command]
    public void CmdRemoveModifier(string modifierName) => RemoveModifier(modifierName);

    public void RemoveModifier(string modifierName)
    {
        if (modifiers.Count > 0)
        {
            //check if modifiers list already contains it
            for (int i = 0; i < modifiers.Count; i++)
            {   
                if (modifiers.Count > 0)
                {
                    if (modifiers[i] && modifiers[i].name == modifierName)
                    {
                        modifiers[i].Destroy();//already contains modifiers.Remove() and Object.Destroy()
                        //modifiers.Remove(modifiers[i]);
                        //Object.Destroy(modifiers[i]);
                        Debug.Log("modifier removed. RemoveModifier()");
                        return;
                    }
                }
                    
            }
        }
    }

    public AbilityModifier GetModifierOnUnit(string modifierName)
    {
        if (modifiers.Count > 0)
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].name == modifierName)
                    return modifiers[i];
            }
        }
        return null;
    }

    [ClientRpc]
    private void RpcAddModifier(string modifierName)
    {
        //check if there are modifiers already on unit
        if (modifiers.Count > 0)
        {
            //check if the specific modifier is already on unit
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].name == modifierName)
                {
                    //modifier already exists on unit. OnRefresh() it
                    modifiers[i].OnRefresh();
                    return;
                }
            }

            //cycled thru all modifiers on unit and none are modifierName
            //cycle thru modifiers from database to find which modifier to add to unit
            AbilityModifierDatabase database = AbilityModifierDatabase.instance;
            foreach (AbilityModifier mod in database.AllModifiers)
            {
                //this is the modifier to add
                if (mod.name == modifierName)
                {
                    AbilityModifier newmod = Object.Instantiate(mod);
                    modifiers.Add(newmod);
                    newmod.OnCreated(gameObject);
                    return;
                }
            }
        }

        else
        //no modifiers on unit yet. add new one
        {
            //cycle thru modifiers from database to find which modifier to add to unit
            AbilityModifierDatabase database = AbilityModifierDatabase.instance;
            foreach (AbilityModifier mod in database.AllModifiers)
            {
                //this is the modifier to add
                if (mod.name == modifierName)
                {
                    AbilityModifier newmod = Object.Instantiate(mod);
                    modifiers.Add(newmod);
                    newmod.OnCreated(gameObject);
                    return;
                }
            }
        }
    }

}
