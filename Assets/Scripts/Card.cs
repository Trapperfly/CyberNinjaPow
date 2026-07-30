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
    public List<CardTag> cardTags = new();
    public List<Tag> tags = new();
    public int cost = 0;
    public int classResourceCost = 0;
    public int repeats = 1;
    public Range range;
    public int extraTagSlots;
    public CardRarity rarity;

    [HideInInspector] public Card original;

    [Tooltip("Each entry defines what happens on one tile of the grid when this card is played.")]
    public List<TileEffect> tileEffects = new();

    public TargetAll targetAll;

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
    public float animationTime;

    public AdditionalCardEffectReply Activate(bool fire)
    {
        AdditionalCardEffectReply reply = new AdditionalCardEffectReply();
        switch (otherEffect)
        {
            case OtherCardEffects.None:
                break;
            case OtherCardEffects.Block://REWORK THIS INTO BASE CARD EFFECT
                break;
            case OtherCardEffects.Parry://REWORK THIS INTO BASE CARD EFFECT
                break;
            case OtherCardEffects.DrawCards:
                if (fire) Manager.Instance.deckManager.DrawPile(amount, 0, true);
                break;
            case OtherCardEffects.DiscardCards:
                //Bring up some UI telling the player to discard cards and the ability to cancel, not discard any cards if applicable, and display information about why to discard. 
                break;
            case OtherCardEffects.AddClassResource:
                if (fire) Manager.Instance.playerManager.ChangeResource(amount);
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
            case OtherCardEffects.ActivateBurn:
                if (fire) Manager.Instance.enemyManager.TriggerStatusesOnAllEnemies(StatusEffect.Burning);
                break;
            default:
                break;
        }
        return reply;
    }

    public int Conditional(CardConditions conditions, AdditionalCardEffect additionalCardEffect = null)
    {
        int bonusDamage = 0;
        switch (conditionalEffect)
        {
            case ConditionalCardEffects.None:
                break;
            case ConditionalCardEffects.GainClassResourceOnKill:
                if (conditions.killed)
                    Manager.Instance.playerManager.ChangeResource(amount);
                break;
            case ConditionalCardEffects.GainClassResourceOnHit:
                if (conditions.hit)
                    Manager.Instance.playerManager.ChangeResource(amount);
                break;
            case ConditionalCardEffects.DoubleDamageOnStatus:
                if (conditions.enemy == null) return 0;
                else
                    bonusDamage = conditions.rawDamage;
                break;
            case ConditionalCardEffects.PlayCardOnHitLocation:
                //Debug.Log(conditions.hit);
                //Debug.Log(conditions.enemy);
                //Debug.Log(additionalCardEffect);
                if (conditions.hit && conditions.enemy && additionalCardEffect != null)
                {
                    Debug.Log("Adding card to queue due to all checks being positive (" + additionalCardEffect + conditions.enemy + conditions.hit + ")");
                    Manager.Instance.boardManager.spaces.TryGetValue(conditions.enemy.position, out BoardSpace target);
                    if (target)
                    {
                        CardTargeting targeting = new();
                        targeting.card = additionalCardEffect.card;
                        targeting.boardSpace = target;
                        Manager.Instance.boardManager.additionalCardQueue.Add(targeting);
                    }
                }
                break;
            case ConditionalCardEffects.PlayCardOnHitSameTargeting:
                break;
            default:
                break;
        }
        return bonusDamage;
    }
}
[System.Serializable]
public class TargetAll
{
    public bool doThis = false;
    public TileEffect effect;
    public TargetAllCondition condition;
    public int number;
    public StatusEffect statusEffect;
}
[System.Serializable]
public class CardTargeting
{
    public Card card;
    public BoardSpace boardSpace;
}

public enum TargetAllCondition
{
    None,
    StatusEffect,
    LowerThanNumber,
    HigherThanNumber,
    FullHealth,

}

public class CardConditions
{
    public EnemyUnit enemy;
    public bool killed = false;
    public bool hit = false;
    public int rawDamage;
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
    AddClassResource,
    AddCardToHand,
    AddCardToDiscard,
    AddCardToDraw,
    ActivateBurn
}

public enum ConditionalCardEffects
{
    None,
    GainClassResourceOnKill,
    GainClassResourceOnHit,
    PlayCardOnHitLocation,
    PlayCardOnHitSameTargeting,
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

public enum Range
{
    Anywhere,
    Melee,
    Ranged,
    Rear,
    Projectile,
    NotTarget

}
[BlackboardEnum]
public enum StatusEffect
{
    None,
    Burning,
    Acid,
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