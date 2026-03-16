using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.UIElements;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AddProjectile", story: "Sends a projectile [direction] with pierce [pierce]", category: "Action", id: "c260d3d4c5205a45bf50fcceab868a0b")]
public partial class AddProjectileAction : Action
{
    [SerializeReference] public BlackboardVariable<Direction> Direction;
    [SerializeReference] public BlackboardVariable<int> Pierce;
    EnemyUnit unit;
    protected override Status OnStart()
    {
        unit = Manager.Instance.enemyManager.GetBlackBoardVariable(GameObject);

        TileEffect tileEffect = unit.intendedAttack[unit.iAttackCounter];

        ProjectileData projectileData = new ProjectileData();
        projectileData.direction = Direction.Value;
        projectileData.pierce = Pierce.Value;

        unit.intendedAttack[unit.iAttackCounter].projectiles.Add(projectileData);

        return Status.Success;
    }
}

