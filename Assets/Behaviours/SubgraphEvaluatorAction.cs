using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SubgraphEvaluator", story: "Returns [result]", category: "Action", id: "98a4f88693d179facbb00f456d2c2f98")]
public partial class SubgraphEvaluatorAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> Result;

    protected override Status OnStart()
    {
        if (Result)
            return Status.Success;
        else return Status.Failure;
    }
}

