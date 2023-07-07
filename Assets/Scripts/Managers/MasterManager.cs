using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Singletons/MasterManager")]

public class MasterManager : SingletonScriptableObject<MasterManager>
{
    [SerializeField]
    private GameSettings gameSettings;
    public static GameSettings GameSettings
    {
        get
        {
            return Instance.gameSettings;
        }
    }

    [SerializeField]
    private UnitSettings unitSettings;
    public static UnitSettings UnitSettings
    {
        get
        {
            return Instance.unitSettings;
        }
    }
}
