using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    [Header("Ability Basic Info")]
    public new string name;
    [Multiline] public string description;
    public Sprite icon;
    public int abilityIndex = 0;
    [Header("Ability Timers")]
    public float cooldown;
    public float swapInTime;
    public float swapOutTime;
    public State currentState = State.Ready;

    public enum State
    {
        Ready,
        Active,
        OnCooldown
    }

    public string AbilityName
    {
        get { return name; }
    }
    public string AbilityDescription
    {
        get { return description; }
    }
    public Sprite AbilityIcon
    {
        get { return icon; }
    }
    public int AbilityIndex
    {
        get { return abilityIndex; }
    }

    public float AbilityCooldown
    {
        get { return cooldown; }
        set { cooldown = value; }
    }
    public float SwapInTime
    {
        get { return swapInTime; }
    }
    public float SwapOutTime
    {
        get { return swapOutTime; }
    }
    public State AbilityState
    {
        get { return currentState; }
    }

    public abstract void Initialize(GameObject obj);

    public abstract void Update();

    public abstract void StartAbility();

    public abstract void TriggerAbility();

    public abstract void CancelAbility();

    public abstract bool IsInComboAnimation();



}
