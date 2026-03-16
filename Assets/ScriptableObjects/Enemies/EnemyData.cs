using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public Sprite enemySprite;
    public List<EnemyPhase> phases = new List<EnemyPhase>();
}
