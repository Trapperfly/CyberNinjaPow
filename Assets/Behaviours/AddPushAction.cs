using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AddPush", story: "Push [distance] [direction]", category: "Action", id: "a3e22a0c37657bb378f38ebc4ce5fc23")]
public partial class AddPushAction : Action
{
    [SerializeReference] public BlackboardVariable<int> Distance;
    [SerializeReference] public BlackboardVariable<Direction> Direction;
    EnemyUnit unit;
    protected override Status OnStart()
    {
        unit = Manager.Instance.enemyManager.GetBlackBoardVariable(GameObject);

        TileEffect tileEffect = unit.intendedAttack[unit.iAttackCounter];

        tileEffect.pushDistance = Distance.Value;
        tileEffect.pushDirection = Direction.Value;

        return Status.Success;
    }
}

