using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetTimer", story: "Set timer to [timer]", category: "Action", id: "059486bcb5e2e2d2d2a1e432e1d6df27")]
public partial class SetTimerAction : Action
{
    [SerializeReference] public BlackboardVariable<int> Timer;
    EnemyUnit unit;
    protected override Status OnStart()
    {
        unit = Manager.Instance.enemyManager.GetBlackBoardVariable(GameObject);

        unit.actionTimer = Timer.Value;

        unit.SetTimer();

        return Status.Success;
    }
}

