using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Enemy")]
public class Enemy : ScriptableObject
{
    public string enemyName = "Gringus";
    public List<EnemyHealth> enemyHealth = new();
    public Sprite sprite;
    public IntentionLooping looping;
}
[System.Serializable]
public class Intention
{
    public int timer = 5;
    public Vector2Int movement;
    public SmartMovement smartMovement;
    public List<Targeting> attack = new();
    public EffectApplication effect;
}

[System.Serializable]
public class EnemyHealth
{
    public int gateHealth;
    public HealthGateSpecialAction specialAction;
    public bool keepPreviousIntentions = true;
    public List<Intention> intentions = new();
    public List<EffectsEnum> effect = new();
}

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
public enum IntentionLooping
{
    none,
    Loop,
    ReverseLoop,
    PingPong,
    RepeatEnd,
    Random,
    RandomStart,
}