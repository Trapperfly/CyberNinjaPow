using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using JetBrains.Annotations;
public class GameManager : MonoBehaviour
{
    public int playerHealth;
    public int playerHealthMax;
    public TMP_Text MoneyText;
    //public List<int> spawnTimerForColumns = new List<int>();
    public Vector2Int spawnDelay;
    public int startTimeProgress;
    public int startTimeTimes;

    public int collisionDamage = 1;

    public int money = 0;

    public float currentThreat = 0;
    public float maxThreat;
    public float threatCalculation = 2f;

    public Objective mainObjective;
    public List<SideObjective> sideObjectives;

    private void Start()
    {
        AlterMoney(0);
        //for (int i = 0; i < Manager.Instance.boardManager.boardSize.x; i++)
        //{
        //    int timer = Random.Range(spawnDelay.x, spawnDelay.y + 1);
        //    spawnTimerForColumns.Add(timer);
        //}
        
        //StartCoroutine(AdvanceBoard(startTimeTimes, startTimeProgress));
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

    //public IEnumerator AdvanceBoard(int amount, int timePerAdvance)
    //{
    //    for(int a = 0; a < amount; a++)
    //    {
    //        Manager.Instance.enemyManager.MoveAllEnemies(1, new(0, -1));
    //        for (int t = 0; t < timePerAdvance; t++)
    //        {
    //            ProgressSpawn();
    //            yield return new WaitForSeconds(0.1f);
    //        }
    //        yield return new WaitForSeconds(0.1f);
    //    }
    //    yield return null;
    //}

    //public void ProgressSpawn(int time)
    //{
    //    for (int i = 0; i < time; i++)
    //    {
    //        for (int j = 0; j < spawnTimerForColumns.Count; j++)
    //        {
    //            spawnTimerForColumns[j] -= 1;
    //            if (spawnTimerForColumns[j] <= 0)
    //            {
    //                if (!Manager.Instance.boardManager.CheckIfEnemyIsOnSpace(new(j, Manager.Instance.boardManager.boardSize.y - 1)))
    //                {
    //                    Manager.Instance.enemyManager.SpawnEnemy(j);
    //                    //Manager.Instance.enemyManager.ShowIntentionsOfEnemies();
    //                }
    //                spawnTimerForColumns[j] = Random.Range(spawnDelay.x, spawnDelay.y);
    //            }
    //        }
    //    }
    //}
    public void ChangeThreat(float change)
    {
        currentThreat += change;
    }

    public void ProgressSpawn()
    {
        float spawningBias = Mathf.Pow(1f - (currentThreat / maxThreat), threatCalculation);
        Debug.Log("Bias for spawning an enemy is " + spawningBias * 100 + "%");
        if (Random.Range(0f,1f) < spawningBias)
        {
            Debug.Log("Spawning an enemy. It was " + spawningBias * 100 + "% chance for it to spawn.");

            float threat = Manager.Instance.enemyManager.SpawnEnemy(Random.Range(0,5));
            ChangeThreat(threat);
        }
    }
}

public enum Objective
{
    None,
    KillCertainAmountOfEnemies,
    KillAllEnemies,
    SurviveCertainAmountOfTime,
    KillTheEliteUnit,
    KillTheBoss,
    SurviveThenKillElite,
    SurviveThenKillBoss,
}

public enum SideObjective
{
    None,
    
}

//public class SideObjectiveTracking
