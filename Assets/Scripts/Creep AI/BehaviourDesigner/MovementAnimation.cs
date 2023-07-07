using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


public class MovementAnimation : Action
{
    NavMeshAgent agent;
    Animator animator;
    public SharedBool patrolling;
    
    // Start is called before the first frame update
    public override void OnAwake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    public override TaskStatus OnUpdate()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
        if (patrolling.Value)
        {
            if (animator.GetLayerWeight(animator.GetLayerIndex("Casual")) != 1.0f)
                animator.SetLayerWeight(animator.GetLayerIndex("Casual"), 1.0f);
        }
        else if (animator.GetLayerWeight(animator.GetLayerIndex("Casual")) > 0f)
            animator.SetLayerWeight(animator.GetLayerIndex("Casual"), 0f);
        return TaskStatus.Running;
    }
}
