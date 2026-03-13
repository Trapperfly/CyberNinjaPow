using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AddToTime", story: "Add [Increment] to [Time]", category: "Action", id: "e562077598551d61462334512b81ff85")]
public partial class AddToTimeAction : Action
{
    [SerializeReference] public BlackboardVariable<int> Increment;
    [SerializeReference] public BlackboardVariable<int> Time;

    protected override Status OnStart()
    {
        Time += Increment;
        return Status.Success;
    }
}

