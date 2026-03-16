using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BasicAttack", story: "Hit at [position] with damage [damage]", category: "Action", id: "b77d8bf87cad1dff94c6c736ae779ec7")]
public partial class BasicAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector2Int> Position;
    [SerializeReference] public BlackboardVariable<int> Damage;
    EnemyUnit unit;
    protected override Status OnStart()
    {
        unit = Manager.Instance.enemyManager.GetBlackBoardVariable(GameObject);

        unit.iAttackCounter++;

        TileEffect tileEffect = new();
        tileEffect.gridPosition = Position.Value;
        tileEffect.damage = Damage.Value;

        unit.intendedAttack.Add(tileEffect);

        return Status.Success;
    }
}

