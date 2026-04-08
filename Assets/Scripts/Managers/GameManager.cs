using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
public class GameManager : MonoBehaviour
{
    public int playerHealth;
    public int playerHealthMax;
    public TMP_Text MoneyText;
    public List<int> spawnTimerForColumns = new List<int>();
    public Vector2Int spawnDelay;
    public int startTimeProgress;
    public int startTimeTimes;

    public int collisionDamage = 1;

    public int money = 0;

    private void Start()
    {
        AlterMoney(0);
        for (int i = 0; i < Manager.Instance.boardManager.boardSize.x; i++)
        {
            int timer = Random.Range(spawnDelay.x, spawnDelay.y + 1);
            spawnTimerForColumns.Add(timer);
        }
        
        StartCoroutine(AdvanceBoard(startTimeTimes, startTimeProgress));
    }

    public void AlterMoney(int amount)
    {
        money += amount;
        MoneyText.text = money.ToString();
        Manager.Instance.shopManager.moneyText.text = money.ToString() + "$";
    }

    public void ProgressTime(int time)
    {
        Manager.Instance.enemyManager.ProgressTime(time);
    }

    public IEnumerator AdvanceBoard(int amount, int timePerAdvance)
    {
        for(int a = 0; a < amount; a++)
        {
            Manager.Instance.enemyManager.MoveAllEnemies(1, new(0, -1));
            for (int t = 0; t < timePerAdvance; t++)
            {
                ProgressSpawn(1);
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
    }

    public void ProgressSpawn(int time)
    {
        for (int i = 0; i < time; i++)
        {
            for (int j = 0; j < spawnTimerForColumns.Count; j++)
            {
                spawnTimerForColumns[j] -= 1;
                if (spawnTimerForColumns[j] <= 0)
                {
                    if (!Manager.Instance.boardManager.CheckIfEnemyIsOnSpace(new(j, Manager.Instance.boardManager.boardSize.y - 1)))
                    {
                        Manager.Instance.enemyManager.SpawnEnemy(j);
                        //Manager.Instance.enemyManager.ShowIntentionsOfEnemies();
                    }
                    spawnTimerForColumns[j] = Random.Range(spawnDelay.x, spawnDelay.y);
                }
            }
        }
    }
}
