using Unity.Behavior;
using UnityEngine;
[System.Serializable]
[CreateAssetMenu(fileName = "New Phase", menuName = "Enemy/Phase")]
public class EnemyPhase : ScriptableObject
{
    public int health;
    public BehaviorGraph actions;
}
