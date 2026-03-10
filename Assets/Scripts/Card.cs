using System.Collections.Generic;
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
    //public bool isProjectile;

    //[Tooltip("The projectile hits does not stop for pierce amount of enemies.")]
    //public int pierce;

    //[Tooltip("Direction the projectile travels.")]
    //public Direction projectileDirection;
    [Tooltip("Projectiles")]
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
    public Direction direction;
    public int pierce;
}

public enum CardTag
{
    None,
    SingleTarget,
    Area,
    Melee,
    Projectile,
    Push,
    Burn,
    Poison,
    Stun,
    Slow,
    Repeating,
}

public enum StatusEffect
{
    None,
    Burn,       // damage over time
    Poison,     // damage over time, different type
    Stun,       // skip turn
    Slow,       // reduced movement
    Weaken,     // reduced damage output
    Vulnerable, // increased damage taken
    Shield,     // absorb damage
    Regen,      // heal over time
}

public enum Direction
{
    None,
    North, South, East, West,
    NorthEast, NorthWest, SouthEast, SouthWest,
}