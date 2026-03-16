using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AddStatus", story: "Applies [stacks] stacks [effect] for [duration] duration", category: "Action", id: "68c254beab015832fa2f540d4243d93d")]
public partial class AddStatusAction : Action
{
    [SerializeReference] public BlackboardVariable<int> Stacks;
    [SerializeReference] public BlackboardVariable<StatusEffect> Effect;
    [SerializeReference] public BlackboardVariable<int> Duration;
    EnemyUnit unit;
    protected override Status OnStart()
    {
        unit = Manager.Instance.enemyManager.GetBlackBoardVariable(GameObject);

        StatusEffectEntry status = new StatusEffectEntry();
        status.effect = Effect.Value;
        status.stacks = Stacks.Value;
        status.duration = Duration.Value;

        unit.intendedAttack[unit.iAttackCounter].statusEffects.Add(status);

        return Status.Success;
    }
}

