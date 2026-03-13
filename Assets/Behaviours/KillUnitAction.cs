using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "KillUnit", story: "[self] dies", category: "Action", id: "507a39ef5b0e41a72932a51b2e822d93")]
public partial class KillUnitAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected override Status OnStart()
    {
        Self.Value.GetComponent<EnemyUnit>().Die();
        return Status.Success;
    }
}

