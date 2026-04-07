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
    public override ItemResponse OnMeleeAttack(int stacks)
    {
        ItemResponse response = new ItemResponse();
        response.integer = 1 + stacks;

        return response;
    }
}
public class ItemResponse
{
    public int integer;
    public List<StatusEffectEntry> statusEffects = new List<StatusEffectEntry>();
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
