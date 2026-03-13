using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Movement", story: "Enemy moves [Vector2Int] or uses [smartMove]", category: "Action", id: "7a7d0555d06d85472692b57b56b784f8")]
public partial class MovementAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector2Int> Vector2Int;
    [SerializeReference] public BlackboardVariable<SmartMovement> SmartMove;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

