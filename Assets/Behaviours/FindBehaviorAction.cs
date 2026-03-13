using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FindBehavior", story: "Gives a [behavior] based on current [phase] of [enemy]", category: "Action", id: "df14b3e90291a955fbb58264d10edd4f")]
public partial class FindBehaviorAction : Action
{
    [SerializeReference] public BlackboardVariable<BehaviorGraph> Behavior;
    [SerializeReference] public BlackboardVariable<int> Phase;
    [SerializeReference] public BlackboardVariable<EnemyData> Enemy;
    protected override Status OnStart()
    {
        Behavior = (BlackboardVariable<BehaviorGraph>)Enemy.Value.phases[Phase.Value].actions;
        return Status.Success;
    }
}

