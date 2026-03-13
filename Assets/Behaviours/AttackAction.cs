using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Attack", story: "Enemy attacks in all [targetCells] Projectile: [bool] Damage: [int]", category: "Action", id: "8e32f945a4e3e2be5b280b4a1f1b9170")]
public partial class AttackAction : Action
{
    [SerializeReference] public BlackboardVariable<List<Vector2Int>> TargetCells;
    [SerializeReference] public BlackboardVariable<bool> Bool;
    [SerializeReference] public BlackboardVariable<int> Int;
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

