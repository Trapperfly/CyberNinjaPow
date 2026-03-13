using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "ReadyToAct", story: "If [Value] is less or equal to [Zero]", category: "Conditions", id: "ce4ad7625406a96b8d02122d9e1b73ac")]
public partial class ReadyToActCondition : Condition
{
    [SerializeReference] public BlackboardVariable<int> Value;
    [SerializeReference] public BlackboardVariable<int> Zero;

    public override bool IsTrue()
    {
        if (Value <= Zero) return true;
        return false;
    }
}
