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