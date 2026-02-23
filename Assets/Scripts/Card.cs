using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Card/Card")]
public class Card : ScriptableObject
{
    public string cardName;
    public List<Tag> cardTags;
    public int cost;
    public int generalDamage;
    public int extraAmount;
    public List<Targeting> targeting = new();
    
}
[System.Serializable]
public class Targeting
{
    public Vector2Int target;
    public bool repeating;
    public int damage;
    //[Header("Projectile")]
    public bool projectile;
    //public bool4 directionNESW;
    [Header("Push")]
    public int push = 0;
    public Direction pushDirection;
    //public bool contextualPush;
}
public enum Tag
{
    none,
    SingleTarget,
    Area,
    Melee,
    Projectile,
    Push,
}
public enum Direction
{
    none,
    North,
    South,
    East,
    West,
}
