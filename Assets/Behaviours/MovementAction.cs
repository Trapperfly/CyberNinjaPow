using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Movement", story: "Move either to [position] or use [smartMovement]", category: "Action", id: "5b9718ecaee461376390f709796cfa5e")]
public partial class MovementAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector2Int> Position;
    [SerializeReference] public BlackboardVariable<SmartMovement> SmartMovement;
    EnemyUnit unit;
    protected override Status OnStart()
    {
        unit = Manager.Instance.enemyManager.GetBlackBoardVariable(GameObject);
        if (unit.position.y <= unit.attackRange) return Status.Success;
        Vector2Int movement;
        if (Position.Value != Vector2Int.zero)
            movement = Position.Value;
        else
            movement = unit.PlanSmartMovement((SmartMovement)SmartMovement);

        unit.intendedMovement = movement;

        unit.DisplayMovementArrow(movement);

        return Status.Success;
    }
}

