using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Manager/UnitSettings")]

public class UnitSettings : SingletonScriptableObject<UnitSettings>
{
    [Header("Unit Name Parameters")]
    [SerializeField] private List<string> allUnitNames = new List<string>
    {
        "Villager",
        "Defender",
        "Ranger",
        "Creep"
    };

    public List<string> AllUnitNames
    { get { return allUnitNames; } }

    [Header("Respawn Parameters")]
    [SerializeField] private bool canRespawn = false;
    public bool CanRespawn
    { get { return canRespawn; } }

    [SerializeField] private float respawnTime = 5.0f;
    public float RespawnTime
    { get { return respawnTime; } }
}
