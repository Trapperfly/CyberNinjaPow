using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChangePhase", story: "Process damage for when [totalDamageTaken] makes a [phase] change of [enemy]", category: "Action", id: "c81f8d09a9410021b351a04bf2cfd710")]
public partial class ChangePhaseAction : Action
{
    [SerializeReference] public BlackboardVariable<int> TotalDamageTaken;
    [SerializeReference] public BlackboardVariable<int> Phase;
    [SerializeReference] public BlackboardVariable<EnemyData> Enemy;
    protected override Status OnStart()
    {
        if (TotalDamageTaken >= (BlackboardVariable<int>)Enemy.Value.phases[Phase].health)
        {
            Phase += (BlackboardVariable<int>)1;
            TotalDamageTaken = (BlackboardVariable<int>)0;
        }
        return Status.Success;
    }
}

