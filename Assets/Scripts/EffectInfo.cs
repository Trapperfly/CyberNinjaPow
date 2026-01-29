using System.Diagnostics;
using UnityEngine;

[System.Serializable]
public class EffectInfo
{
    public Effect effect;
    public string name;
    public int stacks;
    public EffectInfo(Effect effect, string name, int stacks)
    {
        this.effect = effect;
        this.name = name;
        this.stacks = stacks;
    }
}
