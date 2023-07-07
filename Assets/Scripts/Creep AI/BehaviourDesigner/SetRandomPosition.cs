using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class SetRandomPosition : Action
{
    public Vector2 min = Vector2.one * -10;
    public Vector2 max = Vector2.one * 10;
    BehaviorTree tree;

    public override void OnAwake()
    {
        tree = GetComponent<BehaviorTree>();
    }

    public override TaskStatus OnUpdate()
    {
        //RandomPosition.x = Random.Range(min.x, max.x);
        //RandomPosition.z = Random.Range(min.y, max.y);
        tree.SetVariableValue("RandomPosition", new Vector3(Random.Range(min.x, max.x), 0, Random.Range(min.y, max.y)));
        return TaskStatus.Success;
    }
}
