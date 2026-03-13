using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsUnitAlive", story: "Current [Phase] is not more than total Phases of [Enemy]", category: "Conditions", id: "b949004fd1d641928980e4dab17f6974")]
public partial class IsUnitAliveCondition : Condition
{
    [SerializeReference] public BlackboardVariable<int> Phase;
    [SerializeReference] public BlackboardVariable<EnemyData> Enemy;

    public override bool IsTrue()
    {
        if (Phase >= Enemy.Value.phases.Count)
        {
            return false;
        }
        else return true;
    }
}
