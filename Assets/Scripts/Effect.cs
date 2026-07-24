using UnityEngine;

public abstract class Effect
{
    public abstract string GiveName();
    public abstract StatusEffect GiveStatus();
    public virtual void OnAct(EnemyUnit enemyUnit)
    {

    }
    public virtual void OnAfterAct(EnemyUnit enemyUnit)
    {

    }
    public virtual void OnMove(EnemyUnit enemyUnit)
    {

    }
    public virtual void OnAfterMove(EnemyUnit enemyUnit)
    {

    }
    public virtual void OnTrigger(EnemyUnit enemyUnit)
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
    public StatusEffect effect;
    public int amount;
}

public class Burning : Effect
{
    public override string GiveName()
    {
        return "Burning";
    }
    public override StatusEffect GiveStatus()
    {
        return StatusEffect.Burning;
    }
    public override void OnMove(EnemyUnit enemyUnit)
    {
        enemyUnit.TakeDamage(1);
    }

    public override void OnTrigger(EnemyUnit enemyUnit)
    {
        enemyUnit.TakeDamage(1);
    }
}