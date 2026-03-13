using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move", story: "[EnemyUnit] moves [ExpectedMovement]", category: "Action", id: "8803077867ef9f12a976fd94469cc290")]
public partial class MoveAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyUnit> EnemyUnit;
    [SerializeReference] public BlackboardVariable<Vector2Int> ExpectedMovement;

    protected override Status OnStart()
    {
        EnemyUnit.Value.Move();
        return Status.Success;
    }
}

