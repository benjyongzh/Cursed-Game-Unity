using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityModifier : ScriptableObject
{
    [Header("Modifier Basic Info")]
    public new string name;
    [Multiline] public string description;
    public Sprite icon;
    string type;

    [Header("Modifier Timers")]
    public float duration;
    public float elapsedTime = 0f;
    public bool hasIntervalEffect = false;
    public float effectInterval;
    public float elapsedTimeInterval = 0f;

    [Header("Modifier Stacks")]
    public bool stackable = false;
    public int stacks = 1;

    [Header("Modifier Misc. Options")]
    [Range(0, 100), Tooltip("Set to 100 for modifiers without proc chances.")] public float procChancePercentage = 100;

    // Start is called before the first frame update
    public string AbilityModifierName { get { return name; } }
    public string AbilityModifierDescription { get { return description; } }
    public Sprite AbilityModifierIcon { get { return icon; } }
    public string AbilityModifierType { get { return type; } set { type = value; } }
    public float Duration { get { return duration; } set { duration = value; } }
    public float Interval { get { return effectInterval; } }
    public float ElapsedTime { get { return elapsedTime; } }
    public float ElapsedTimeInterval { get { return elapsedTimeInterval; } }
    public float ProcChancePercentage { get { return procChancePercentage; } set { procChancePercentage = value; } }

    public abstract void OnCreated(GameObject obj);//initializing
    public abstract void Update();
    public abstract void ApplyInitialEffect();
    public abstract void ApplyIntervalEffect();
    public abstract void OnRefresh();
    public abstract void StopModifier();
    public abstract void Destroy();
    
}
