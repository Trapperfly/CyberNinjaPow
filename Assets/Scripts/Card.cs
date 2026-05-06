using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Card/Card")]
public class Card : ScriptableObject
{
    [Header("Identity")]
    public string cardName;
    public string description;
    public Sprite artwork;
    public List<CardTag> cardTags;
    public int cost;
    public Range range;
    public int extraTagSlots;
    public CardRarity rarity;

    [Tooltip("Each entry defines what happens on one tile of the grid when this card is played.")]
    public List<TileEffect> tileEffects = new();

    public Card playAdditionalCardAfterThisOne;

    public List<AdditionalCardEffect> additionalCardEffects = new();
}

[System.Serializable]
public class AdditionalCardEffect
{
    public OtherCardEffects otherEffect;
    public ConditionalCardEffects conditionalEffect;
    public int amount;
    public Card card;
    public TileEffect effect;
    public StatusEffect statusEffect;
    public float animationTime;

    public AdditionalCardEffectReply Activate(bool fire)
    {
        AdditionalCardEffectReply reply = new AdditionalCardEffectReply();
        switch (otherEffect)
        {
            case OtherCardEffects.None:
                break;
            case OtherCardEffects.Block:
                break;
            case OtherCardEffects.Parry:
                break;
            case OtherCardEffects.DrawCards:
                if (fire) Manager.Instance.deckManager.DrawPile(amount, 0);
                break;
            case OtherCardEffects.DiscardCards:
                //Bring up some UI telling the player to discard cards and the ability to cancel, not discard any cards if applicable, and display information about why to discard. 
                break;
            case OtherCardEffects.UseClassResource:
                if (fire)
                {
                    bool enough = Manager.Instance.playerManager.UseResource(amount);
                    reply.stop = !enough;
                }
                break;
            case OtherCardEffects.AddClassResource:
                if (fire) Manager.Instance.playerManager.AddResource(amount);
                break;
            case OtherCardEffects.CostIsEqualToClassResource:
                if (fire) reply.additionalCost = Manager.Instance.playerManager.playerResource;
                break;
            case OtherCardEffects.UseAllClassResource:
                if (fire) Manager.Instance.playerManager.UseAllResource();
                break;
            case OtherCardEffects.UseAllClassResourceForRepeat:
                int uses = Manager.Instance.playerManager.playerResource;
                reply.repeats = uses;
                break;
            case OtherCardEffects.UseAllClassResourceGainCard:
                int cards = Manager.Instance.playerManager.playerResource;
                if (fire) for (int i = 0; i < cards; i++) Manager.Instance.deckManager.AddCardTo(WhereDoesTheCardGo.Hand, card);
                break;
            case OtherCardEffects.AddCardToHand:
                if (fire) Manager.Instance.deckManager.AddCardTo(WhereDoesTheCardGo.Hand, card);
                break;
            case OtherCardEffects.AddCardToDiscard:
                if (fire) Manager.Instance.deckManager.AddCardTo(WhereDoesTheCardGo.Discard, card);
                break;
            case OtherCardEffects.AddCardToDraw:
                if (fire) Manager.Instance.deckManager.AddCardTo(WhereDoesTheCardGo.Draw, card);
                break;
            case OtherCardEffects.TargetAllEnemies:
                Manager.Instance.boardManager.TargetAllEnemies(fire, effect);
                break;
            case OtherCardEffects.TargetAllEnemiesWithSpecificStatus:
                Manager.Instance.boardManager.TargetAllEnemies(fire, effect, statusEffect);
                break;
            default:
                break;
        }
        return reply;
    }

    public AdditionalCardEffectReply Conditional(CardConditions conditions)
    {
        AdditionalCardEffectReply reply = new AdditionalCardEffectReply();
        switch (conditionalEffect)
        {
            case ConditionalCardEffects.None:
                break;
            case ConditionalCardEffects.GainClassResourceOnKill:
                if (conditions.killed)
                    Manager.Instance.playerManager.AddResource(amount);
                break;
            case ConditionalCardEffects.GainClassResourceOnHit:
                if (conditions.hit)
                    Manager.Instance.playerManager.AddResource(amount);
                break;
            case ConditionalCardEffects.DoubleDamageOnStatus:
                reply.damageMultiplier = 2;
                break;
            default:
                break;
        }
        return reply;
    }
}

public class CardConditions
{
    public bool killed = false;
    public bool hit = false;

}

public class AdditionalCardEffectReply
{
    public bool stop = false;
    public int additionalCost = 0;
    public int repeats = 0;
    public int damage = 0;
    public int damageMultiplier = 1;
    public bool hitEnemy = false;
}

public enum OtherCardEffects
{
    None,
    Block,
    Parry,
    DrawCards,
    DiscardCards,
    UseClassResource,
    AddClassResource,
    CostIsEqualToClassResource,
    UseAllClassResource,
    UseAllClassResourceForRepeat,
    UseAllClassResourceGainCard,
    AddCardToHand,
    AddCardToDiscard,
    AddCardToDraw,
    TargetAllEnemies,
    TargetAllEnemiesWithSpecificStatus,

}

public enum ConditionalCardEffects
{
    None,
    GainClassResourceOnKill,
    GainClassResourceOnHit,
    DoubleDamageOnStatus,
}

/// <summary>
/// Defines everything that happens on a single grid tile when the card is played.
/// </summary>
[System.Serializable]
public class TileEffect
{
    [Tooltip("Position on the grid, relative to the target origin.")]
    public Vector2Int gridPosition;

    [Header("Damage")]
    public int damage;

    [Header("Projectile")]
    public List<ProjectileData> projectiles = new();

    [Header("Push")]
    public int pushDistance;
    public Direction pushDirection;

    [Header("Status Effects")]
    public List<StatusEffectEntry> statusEffects = new();

    [Header("Behaviour Flags")]
    [Tooltip("If true, this tile effect triggers multiple times (e.g. rapid hits or repeating pulses).")]
    public bool repeating;

    [Tooltip("How many times to repeat, if repeating is true.")]
    public int repeatCount;

    [Tooltip("Delay in seconds between repeats.")]
    public float repeatInterval;
}

/// <summary>
/// A single status effect applied to a tile, with its own stacks/duration.
/// </summary>
[System.Serializable]
public class StatusEffectEntry
{
    public StatusEffect effect;

    [Tooltip("Number of stacks or intensity of the effect.")]
    public int stacks = 1;

    [Tooltip("How many turns the effect lasts. 0 = permanent / handled elsewhere.")]
    public int duration = 1;
}

[System.Serializable]
public class ProjectileData
{
    public int projDamage;
    public Direction direction;
    public int pierce;
}

public enum CardTag
{
    None,
    Damage,         //Damage taken by player
    Flame,          //Usually applies or manipulates flame status effect
    Thunder,        //Usually applies or manipulates thunder status effect
    Hacking,        //Usually applies or manipulates hacking status effect
    Projectile,     //Is something that launches projectiles
    Precision,      //Is something that targets one single tile
    Area,           //Is something that targest a lot of tiles
    Cryo,           //Usually applies or manipulates cryo status effect
    Deployment,     //Is something or manipulates stuff on the tactical grid
    Explosive,      //Is something explosive, usually coupled with Area
    Trap,           //Is something placed on the tactical grid that triggers
    Cleave,         //Probably not used, but large melee
    Cards,          //Usually relates to drawing, discarding, and making cards
    Repair,         //Usually relates to healing self or deployment
    TimeWarp,       //Usually relates to changing time in either direction
    Defence,        //Usually relates to blocking damage
}
public enum Range
{
    Anywhere,
    Melee,
    Ranged,
    Rear,
    Projectile

}
[BlackboardEnum]
public enum StatusEffect
{
    None,
    Burning,   
    Hacked,    
    Marked,    
    Conductive,
    Chilled,
    Frozen,    
    Shield,    
}
[BlackboardEnum]
public enum Direction
{
    None,
    North, South, East, West,
    NorthEast, NorthWest, SouthEast, SouthWest,
}