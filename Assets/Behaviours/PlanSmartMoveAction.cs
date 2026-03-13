using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.UIElements;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlanSmartMove", story: "Use [smartMove] to change [expectedMovement] based on [position]", category: "Action", id: "10ff581f87463564ea7c7ca8ea9a1cc9")]
public partial class PlanSmartMoveAction : Action
{
    [SerializeReference] public BlackboardVariable<SmartMovement> SmartMove;
    [SerializeReference] public BlackboardVariable<Vector2Int> ExpectedMovement;
    [SerializeReference] public BlackboardVariable<Vector2Int> Position;

    protected override Status OnStart()
    {
        PlanMovement();
        return Status.Success;
    }
    public void PlanMovement()
    {
        switch ((SmartMovement)SmartMove)
        {
            case SmartMovement.None:
                break;
            case SmartMovement.SmartDown:
                ExpectedMovement = (BlackboardVariable<Vector2Int>)Manager.Instance.enemyManager.CheckMoveDirection(Position, new(0, -1));
                break;
            case SmartMovement.SmartUp:
                break;
            case SmartMovement.SmartLeft:
                break;
            case SmartMovement.SmartRight:
                break;
            case SmartMovement.SmartDownX2:
                break;
            case SmartMovement.CoverDown:
                break;
            case SmartMovement.CoverDownX2:
                break;
            default:
                break;
        }
        if (ExpectedMovement != new Vector2Int(0, 0)) { }
            Manager.Instance.enemyManager.DisplayMovementArrow(Position, ExpectedMovement);
    }
}

