using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class CuriousIdle : Action
{
    public float minDuration = 2f;
    public float maxDuration = 7f;
    public float distanceTolerance = 1f;
    
    public SharedBool patrolling;
    public SharedBool curious;
    public SharedBool aggro;
    public SharedVector3 audioPoint;

    float startTime;
    float duration;

    public override void OnStart() {
        startTime = Time.time;
        duration = Random.Range(minDuration, maxDuration);
    }

    public override TaskStatus OnUpdate() {
        if (Time.time - startTime > duration) {
            //idle too long. go back to patrolling
            patrolling.Value = true;
            curious.Value = false;
            aggro.Value = false;

            return TaskStatus.Failure;
        }
        else
        {
            //still idling
            float distance = Vector3.Distance(audioPoint.Value, transform.position);
            if (distance >= distanceTolerance || !curious.Value)
            {
                //audioPoint is too far, or creep is no longer in curious mode
                return TaskStatus.Success;
            }
        }
        return TaskStatus.Running;
    }

}
