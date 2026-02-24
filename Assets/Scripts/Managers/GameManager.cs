using UnityEngine;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public List<Enemy> enemyQueue = new List<Enemy>();
    public List<int> spawnTimerForColumns = new List<int>();

    private void Start()
    {
        for (int i = 0; i < Manager.Instance.boardManager.boardSize.x; i++)
        {
            int timer = Random.Range(6,20);
            spawnTimerForColumns.Add(timer);
        }
    }

    public void ProgressTime(int time)
    {
        Manager.Instance.enemyManager.ProgressTime(time);
        for (int i = 0; i < time; i++) 
        {
            for(int j = 0; j < spawnTimerForColumns.Count; j++)
            {
                spawnTimerForColumns[j] -= 1;
                if (spawnTimerForColumns[j] == 0) 
                {
                    if (!Manager.Instance.boardManager.CheckIfEnemyIsOnSpace(new(j, Manager.Instance.boardManager.boardSize.y - 1)))
                        SpawnEnemy(j); 
                    spawnTimerForColumns[j] = Random.Range(6, 12); 
                }
            }
        }
    }

    public void SpawnEnemy(int column)
    {
        if (enemyQueue.Count == 0) return;
        GameObject unitGO = Instantiate(Manager.Instance.enemyManager.enemyPrefab, Manager.Instance.enemyManager.enemyParent);

        EnemyUnit unit = unitGO.GetComponent<EnemyUnit>();

        unit.position = new(column,Manager.Instance.boardManager.boardSize.y - 1);

        unit.enemy = enemyQueue[0];

        enemyQueue.RemoveAt(0);
    }
}
