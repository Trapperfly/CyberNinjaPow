using UnityEngine;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public int playerHealth;
    public int playerHealthMax;
    public TMPro.TMP_Text TMP_PlayerHealth;
    public List<int> spawnTimerForColumns = new List<int>();
    public Vector2Int spawnDelay;

    private void Start()
    {
        for (int i = 0; i < Manager.Instance.boardManager.boardSize.x; i++)
        {
            int timer = Random.Range(spawnDelay.x, spawnDelay.y + 1);
            spawnTimerForColumns.Add(timer);
        }
        TMP_PlayerHealth.text = playerHealth + "/" + playerHealthMax;
    }

    public void ProgressTime(int time)
    {
        Manager.Instance.enemyManager.ProgressTime(time);
    }

    public void ProgressSpawn(int time)
    {
        for (int i = 0; i < time; i++)
        {
            for (int j = 0; j < spawnTimerForColumns.Count; j++)
            {
                spawnTimerForColumns[j] -= 1;
                if (spawnTimerForColumns[j] == 0)
                {
                    if (!Manager.Instance.boardManager.CheckIfEnemyIsOnSpace(new(j, Manager.Instance.boardManager.boardSize.y - 1)))
                        Manager.Instance.enemyManager.SpawnEnemy(j);
                    spawnTimerForColumns[j] = Random.Range(spawnDelay.x, spawnDelay.y);
                }
            }
        }
    }
}
