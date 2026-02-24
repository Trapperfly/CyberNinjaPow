using UnityEngine;

public abstract class Effect
{
    public abstract string GiveName();
    public virtual void OnAct(EnemyUnit enemyUnit)
    {

    }
    public virtual void OnAfterAct(EnemyUnit enemyUnit)
    {

    }
}
public enum EffectsEnum
{
    none,
    Poison,
    Regeneration,
    TimeWarp,
}
[System.Serializable]
public class EffectApplication
{
    public EffectsEnum effect;
    public int amount;
}

public class Poison : Effect
{
    public override string GiveName()
    {
        return "Poison";
    }
    public override void OnAct(EnemyUnit enemyUnit)
    {
        base.OnAct(enemyUnit);
    }
}