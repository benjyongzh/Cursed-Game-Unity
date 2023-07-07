using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class MoveToTargetPointWhileInState : Action
{
    public float speed = 2.6f;
    public float stoppingDistance = 0.1f;
    public bool updateRotation = true;
    public float acceleration = 40.0f;
    public float tolerance = 2.0f;
    private float distanceLeft;
    
    public SharedBool requiredState;
    public SharedVector3 targetPoint;

    NavMeshAgent agent;

    public override void OnAwake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public override void OnStart() {
        agent.stoppingDistance = stoppingDistance;
        agent.speed = speed;
        agent.destination = targetPoint.Value;
        agent.updateRotation = updateRotation;
        agent.acceleration = acceleration;
        agent.isStopped = false;
    }

    public override TaskStatus OnUpdate() {

        //update destionation to any updated audiopoint
        if (agent.destination != targetPoint.Value)
        {
            agent.destination = targetPoint.Value;
        }

        if (agent.pathPending && requiredState.Value) {
            //still on the way to audiopoint
            return TaskStatus.Running;
        }

        distanceLeft = Vector3.Distance(transform.position, agent.destination);
        if (distanceLeft < stoppingDistance + tolerance || agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid || !requiredState.Value) {
            //reached attentionpoint

            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }
}
