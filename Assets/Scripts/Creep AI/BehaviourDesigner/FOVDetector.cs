using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class FOVDetector : Action
{
    public float targetDistanceTolerance = 1f;
    public SharedBool patrolling;
    public SharedBool curious;
    public SharedBool aggro;
    public SharedVector3 targetPoint;
    
    FOVDetection FovDetection;
    private float FOVHeight;
    private float maxAngle;
    private float maxRadius;
    private LayerMask layerMask;

    private GameObject FOVTarget;

    public override void OnAwake()
    {
        FovDetection = gameObject.GetComponentInChildren<FOVDetection>();
        FOVHeight = FovDetection.GetFOVHeight();
        maxAngle = FovDetection.GetMaxAngle();
        maxRadius = FovDetection.GetMaxRadius();
        layerMask = FovDetection.GetLayerMask();
    }

    public override void OnStart() {
    }

    public override TaskStatus OnUpdate() {
        FOVTarget = FovDetection.anyPlayerInFOV(transform, FOVHeight, maxAngle, maxRadius, layerMask);
        if (FOVTarget != null)
        //theres a player in FOV
        {
            Debug.Log("player in FOV");
            if (!aggro.Value)
            //new aggression
            {
                curious.Value = false;
                patrolling.Value = false;
                aggro.Value = true;
            }

            //this player in FOV is not what is recorded in blackboard
            if (targetPoint.Value != FOVTarget.transform.position)
            //new FOVtarget
            {
                //check how far off this new player in FOV is, compared to blackboard's record
                float distance = Vector3.Distance(targetPoint.Value, FOVTarget.transform.position);
                if (distance <= targetDistanceTolerance)
                {
                    //similar target
                    targetPoint.Value = FOVTarget.transform.position;
                }
                else
                {
                    //different target detected. compare distances to transform (check to see how it affects creep going to empty point just because it is nearer)
                    float distanceNewTarget = Vector3.Distance(FOVTarget.transform.position, transform.position);
                    float distanceBlackboardTarget = Vector3.Distance(targetPoint.Value, transform.position);
                    if (distanceNewTarget < distanceBlackboardTarget)
                    {
                        //new target is closer
                        targetPoint.Value = FOVTarget.transform.position;
                    }
                    else
                    {
                        //new target is further. make sure there is still someone at the old target before pursuing old targetpoint. use tolerance and overlapspherenonalloc
                        Collider[] hitColliders = new Collider[15];
                        int numColliders = Physics.OverlapSphereNonAlloc(targetPoint.Value, targetDistanceTolerance, hitColliders, layerMask);
                        if (numColliders > 0)
                        {
                            //theres someone at the old target spot. choose closest player to set blackboard target point to
                            float distanceToNewTarget;
                            float distanceClosestTarget = maxRadius;
                            Vector3 ClosestTarget = targetPoint.Value;
                            for (int i = 0; i < numColliders; i++)
                            {
                                if (hitColliders[i] != null)
                                {
                                    distanceToNewTarget = Vector3.Distance(hitColliders[i].transform.position, transform.position);
                                    if (distanceToNewTarget < distanceClosestTarget)
                                    {
                                        distanceClosestTarget = distanceNewTarget;
                                        ClosestTarget = hitColliders[i].transform.position;
                                    }
                                }
                            }
                            //set blackboard targetpoint to closesttarget nearby old targetpoint
                            targetPoint.Value = ClosestTarget;
                        }
                        else
                        {
                            //theres no one at old target spot. set blackboard target point to new FOVplayerpos
                            targetPoint.Value = FOVTarget.transform.position;
                        }

                    }
                }
                
            }
            //this player in FOV is already the one record in blackboard. update blackboard info
            else
            {
                targetPoint.Value = FOVTarget.transform.position;
            }
            
        }

        return TaskStatus.Success;
    }
}
