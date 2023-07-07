using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class CheckCreepState : Conditional
{
    public SharedBool desiredState;
    
    // Update is called once per frame
    public override TaskStatus OnUpdate()
    {
        if (desiredState.Value)
            return TaskStatus.Success;
        else
        {
            return TaskStatus.Failure;
        }
    }
}
