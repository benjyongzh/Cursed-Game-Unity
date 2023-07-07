using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class MoveToWaypoint : Action
{
    public float speed = 1.5f;
    public float stoppingDistance = 0.2f;
    public bool updateRotation = true;
    public float acceleration = 40.0f;
    public float tolerance = 1.0f;
    private float distanceLeft;
    
    NavMeshAgent agent;
    AIWaypointController waypointController;
    BehaviorTree tree;

    public SharedInt waypointID;
    public SharedVector3 randpos;
    public SharedBool patrolling;

    public override void OnAwake()
    {
        agent = GetComponent<NavMeshAgent>();
        waypointController = GetComponent<AIWaypointController>();
        tree = GetComponent<BehaviorTree>();
    }

    public override void OnStart()
    {
        agent.stoppingDistance = stoppingDistance;
        agent.speed = speed;
        agent.updateRotation = updateRotation;
        agent.acceleration = acceleration;

        if (waypointID.Value >= waypointController.waypoints.Count)
        {
            waypointID.Value = 0;
        }
        Vector3 pos = waypointController.waypoints[waypointID.Value].position;
        pos = new Vector3(pos.x + randpos.Value.x, pos.y, pos.z + randpos.Value.z);
        agent.destination = pos;
    }

    // Update is called once per frame
    public override TaskStatus OnUpdate()
    {
        //reached waypoint
        distanceLeft = Vector3.Distance(transform.position, agent.destination);
        //Debug.Log("remaining distance is " + distanceLeft);
        if (distanceLeft < stoppingDistance + tolerance)
        {
            if (waypointID.Value < waypointController.waypoints.Count)
            {
                waypointID.Value += 1;
                Debug.Log("Pending waypoint increased. Now at " + waypointID.Value + ".");
            }
            else
            {
                waypointID.Value = 0;
                Debug.Log("Pending waypoint now at " + waypointID.Value + ".");
            }
            return TaskStatus.Success;
        }

        //going to waypoint
        if (agent.pathPending && patrolling.Value) {
            return TaskStatus.Running;
        }

        //IsPatrolling switched off
        else if (agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid || !patrolling.Value)
        {
            //clear current path
            agent.isStopped = true;
            return TaskStatus.Failure;
        }

        return TaskStatus.Running;
    }
}
