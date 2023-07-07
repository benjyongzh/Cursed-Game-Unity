using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CreepAIController : MonoBehaviour
{
    [HideInInspector] public bool isPatrolling = true;
    [HideInInspector] public bool isCurious = false;
    [HideInInspector] public bool isAggressive = false;
    [HideInInspector] public int PendingWaypointID = 0;
    [HideInInspector] public Vector3 MoveToPosition;
    [HideInInspector] public Vector3 RandomPosition;
    [HideInInspector] public Vector3 AudioAttentionPoint;
    [HideInInspector] public Vector3 FOVPlayerTargetPoint;

    public bool IsPatrolling()
    {
        return isPatrolling;
    }

    public bool IsCurious()
    {
        return isCurious;
    }

    public bool IsAggressive()
    {
        return isAggressive;
    }

    public Vector3 GetMoveToPosition()
    {
        return MoveToPosition;
    }

    public void SetMoveToPosition(Vector3 pos)
    {
        MoveToPosition = new Vector3(pos.x, pos.y, pos.z);
    }
}
