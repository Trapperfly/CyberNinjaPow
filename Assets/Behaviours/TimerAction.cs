using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Timer", story: "[Time] ticks [ProgressedTime]", category: "Action", id: "922810a9cc8237ceb4ea2b48861f147e")]
public partial class TimerAction : Action
{
    [SerializeReference] public BlackboardVariable<int> Time;
    [SerializeReference] public BlackboardVariable<int> ProgressedTime;
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

