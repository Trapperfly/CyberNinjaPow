using UnityEngine;
using System.Collections.Generic;

public class EnemyData : ScriptableObject
{
    public string enemyName;
    public Sprite enemySprite;
    public List<EnemyPhase> phases = new List<EnemyPhase>();
}
