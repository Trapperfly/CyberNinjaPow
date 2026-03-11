using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Act", story: "If [time] is less than or equal to [zero]", category: "Action", id: "f26dbdf2a275527a4fd4b3db515848a0")]
public partial class ActAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Time;
    [SerializeReference] public BlackboardVariable<int> Zero;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

