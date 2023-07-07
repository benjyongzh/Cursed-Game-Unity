using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class UpdateAudioPoint : Action
{
    private Vector3 newAudioPoint;
    public float locationTolerance = 2.0f;
    public SharedVector3 currentAudioPoint;
    public SharedBool patrolling;
    public SharedBool curious;
    public SharedBool aggro;

    CreepAIAudioDetector detector;

    public override void OnAwake()
    {
        detector = GetComponent<CreepAIAudioDetector>();
    }

    public override void OnStart()
    {
        newAudioPoint = detector.GetAudioAttentionPoint();
    }

    public override TaskStatus OnUpdate() {
        float distance = Vector3.Distance(newAudioPoint, currentAudioPoint.Value);
        if (distance > locationTolerance)
        {
            //recent attentionpoint is some distance away from last recorded attentionpoint. update blackboard
            currentAudioPoint.Value = newAudioPoint;
            if (patrolling.Value)//only can go from patrolling to curious. not from aggressive
            {
                curious.Value = true;
                patrolling.Value = false;
                aggro.Value = false;
            }
        }
        //else
        //{
            //recent attentionpoint is close enough to blackboard's recorded point. regard it as the same point
        //}
        return TaskStatus.Success;
    }
}
