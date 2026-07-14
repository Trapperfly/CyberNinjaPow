using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Enemy/OldEnemy")]
public class Enemy : ScriptableObject
{
    public string enemyName = "Gringus";
    public List<int> health = new();
    public List<Sprite> sprite;
    public int movement = 0;
    public int readySpeed = 0;
    public Range range;
    public Damage damage;
    public int attacks = 1;
    public float threat = 0;
}
[System.Serializable]
//public class Intention
//{
//    public int timer = 5;
//    public Vector2Int movement;
//    public SmartMovement smartMovement;
//    public List<TileEffect> attack = new();
//    public EffectApplication effect;
//}

//[System.Serializable]
//public class EnemyHealth
//{
//    public int gateHealth;
//    public HealthGateSpecialAction specialAction;
//    public bool keepPreviousIntentions = true;
//    public List<Intention> intentions = new();
//}
[BlackboardEnum]
public enum SmartMovement
{
    None,
    SmartDown,
    SmartUp,
    SmartLeft,
    SmartRight,
    SmartDownX2,
    CoverDown,
    CoverDownX2,
}

public enum HealthGateSpecialAction
{
    none,
    Dead,
    Reincarnate,
    Explode
}

[System.Serializable]
public class EnemyInfo
{
    public Enemy enemy;
    public int cost;

}
public enum Damage
{
    Small,
    Medium,
    Large,
}