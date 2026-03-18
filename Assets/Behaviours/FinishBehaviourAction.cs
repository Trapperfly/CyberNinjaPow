using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FinishBehaviour", story: "Finish the behaviour graph", category: "Action", id: "e1b7ff07c07d1933dc98f310cfb04b9d")]
public partial class FinishBehaviourAction : Action
{

    protected override Status OnStart()
    {
        Manager.Instance.enemyManager.ShowIntentionsOfEnemies();
        return Status.Success;
    }
}

