using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityModifierDatabase : SingletonMonoBehaviour<AbilityModifierDatabase>
{
    public AbilityModifier[] AllModifiers;

    private void Start()
    {
        AllModifiers = Resources.LoadAll<AbilityModifier>("AbilityModifiers");
    }


}
