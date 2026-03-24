using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FinishBehaviour", story: "Finish the behaviour graph and tell it is [ready]", category: "Action", id: "e1b7ff07c07d1933dc98f310cfb04b9d")]
public partial class FinishBehaviourAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> Ready;

    protected override Status OnStart()
    {
        Ready.Value = true;
        Debug.Log("Graph Finished");
        return Status.Success;
    }
}