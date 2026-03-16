using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Unity.VisualScripting;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AddRepeat", story: "Repeat action [repeatCount] times with a interval of [interval]", category: "Action", id: "8cc1c91134e1968c6a64ae5a799b25f2")]
public partial class AddRepeatAction : Action
{
    [SerializeReference] public BlackboardVariable<int> RepeatCount;
    [SerializeReference] public BlackboardVariable<float> Interval;
    EnemyUnit unit;
    protected override Status OnStart()
    {
        unit = Manager.Instance.enemyManager.GetBlackBoardVariable(GameObject);

        TileEffect tileEffect = unit.intendedAttack[unit.iAttackCounter];

        tileEffect.repeating = true;
        tileEffect.repeatCount = RepeatCount.Value;
        tileEffect.repeatInterval = Interval.Value;

        return Status.Success;
    }
}

