using UnityEngine;
using System.Collections.Generic;
public abstract class Item
{
    public abstract string GiveName();
    public abstract Rarity GiveRarity();

    public virtual ItemResponse OnTimeTick(int stacks)
    {
        return null;
    }
    public virtual ItemResponse OnEnemyAct(int stacks, EnemyUnit enemy)
    {
        return null;
    }
    public virtual ItemResponse OnHit(int stacks, EnemyUnit enemy, Vector2Int position)
    {
        return null;
    }
    public virtual ItemResponse OnMeleeAttack(int stacks)
    {
        return null;
    }
    public virtual ItemResponse OnRangedAttack(int stacks)
    {
        return null;
    }
    public virtual ItemResponse OnRearAttack(int stacks)
    {
        return null;
    }
    public virtual ItemResponse OnAreaAttack(int stacks)
    {
        return null;
    }
    public virtual ItemResponse OnProjectileAttack(int stacks)
    {
        return null;
    }
    public virtual ItemResponse OnTagUse(int stacks)
    {
        return null;
    }
    public virtual ItemResponse OnTakeDamage(int stacks)
    {
        return null;
    }
}
public class ItemResponse
{
    public int integer;
    public List<StatusEffectEntry> statusEffects = new List<StatusEffectEntry>();
}
public class RocketFists : Item
{
    public override string GiveName()
    {
        return "Rocket Fists";
    }
    public override Rarity GiveRarity()
    {
        return Rarity.Common;
    }
    public override ItemResponse OnHit(int stacks, EnemyUnit enemy, Vector2Int position)
    {
        ItemResponse response = new ItemResponse();

        if (position.y > 1) return response;

        response.integer = stacks;

        return response;
    }
}

public class PrecisionAirstrike : Item
{
    public override string GiveName()
    {
        return "Precision Airstrike";
    }
    public override Rarity GiveRarity()
    {
        return Rarity.Common;
    }
    public override ItemResponse OnTimeTick(int stacks)
    {
        ItemResponse response = new ItemResponse();
        int i = 0;

        foreach (EnemyUnit unit in Manager.Instance.enemyManager.enemies)
        {
            if (unit.damageTaken == 0) unit.TakeDamage(stacks);
        }
        response.integer = i;

        return response;
    }
}

public enum Rarity
{
    none,
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
