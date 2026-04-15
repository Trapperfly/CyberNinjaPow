using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using JetBrains.Annotations;
public class GameManager : MonoBehaviour
{
    public bool waveInProgress = false;

    public int playerHealth;
    public int playerHealthMax;
    public TMP_Text MoneyText;
    //public List<int> spawnTimerForColumns = new List<int>();
    public Vector2Int spawnDelay;

    public int startEnemyYPosition = 2;
    public float enemySpawnDelay = 0.2f;
    public int startEnemyCount = 5;

    public int startTimeProgress;
    public int startTimeTimes;

    public int collisionDamage = 1;

    public int money = 0;

    public float currentThreat = 0;
    public float maxThreat;
    public float threatCalculation = 2f;

    public Objective mainObjective;
    public List<SideObjective> sideObjectives;

    public int mainObjectiveTracker;
    public int mainObjectiveGoal;

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

    private void Update()
    {
        if (!waveInProgress && Input.GetKeyDown(KeyCode.Space)) StartWave();
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

    public void AfterTimeProgress()
    {
        bool finished = CheckIfWaveIsFinished();

        if (finished) { FinishWave(); return; }

        //If not finished
        ProgressSpawn();
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

    public void KilledAnEnemy(float threat, EnemyStrength strength = EnemyStrength.none)
    {
        ChangeThreat(-threat);
        switch (mainObjective)
        {
            case Objective.KillCertainAmountOfEnemies:
            case Objective.KillAllEnemies:
                mainObjectiveTracker += 1;
                break;
            case Objective.KillTheEliteUnit:
            case Objective.SurviveThenKillElite:
                if (strength == EnemyStrength.Elite) mainObjectiveTracker += 1;
                break;
            case Objective.KillTheBoss:
            case Objective.SurviveThenKillBoss:
                if (strength == EnemyStrength.Boss) mainObjectiveTracker += 1;
                break;
        }
    }

    public void ProgressSpawn()
    {
        float spawningBias = Mathf.Pow(1f - (currentThreat / maxThreat), threatCalculation);
        Debug.Log("Bias for spawning an enemy is " + spawningBias * 100 + "%");
        if (Random.Range(0f,1f) < spawningBias)
        {
            Debug.Log("Spawning an enemy. It was " + spawningBias * 100 + "% chance for it to spawn.");

            Manager.Instance.enemyManager.SpawnEnemy(Random.Range(0,5));
        }
    }

    public void StartWave()
    {
        StartCoroutine(IStartWave());
    }
    public IEnumerator IStartWave()
    {
        waveInProgress = true;
        Manager.Instance.busy = true;
        yield return null;
        mainObjectiveTracker = 0;
        switch (mainObjective)
        {
            case Objective.None:
                break;
            case Objective.KillCertainAmountOfEnemies:
                mainObjectiveGoal = 10;
                break;
            case Objective.KillAllEnemies:
                mainObjectiveGoal = 10;
                break;
            case Objective.SurviveCertainAmountOfTime:
                mainObjectiveGoal = 50;
                break;
            case Objective.KillTheEliteUnit:
                mainObjectiveGoal = 1;
                break;
            case Objective.KillTheBoss:
                mainObjectiveGoal = 1;
                break;
            case Objective.SurviveThenKillElite:
                mainObjectiveGoal = 1;
                break;
            case Objective.SurviveThenKillBoss:
                mainObjectiveGoal = 1;
                break;
            default:
                break;
        }

        for (int i = 0; i < Manager.Instance.deckManager.handSize; i++)
        {
            Manager.Instance.deckManager.DrawCard();
            yield return new WaitForSeconds(Manager.Instance.deckManager.drawAnimTime);
        }

        for (int i = 0; i < startEnemyCount; i++)
        {
            int column = Random.Range(0, Manager.Instance.boardManager.boardSize.x);
            int row = Random.Range(Manager.Instance.boardManager.boardSize.y - startEnemyYPosition, Manager.Instance.boardManager.boardSize.y);

            Manager.Instance.enemyManager.SpawnEnemy(column, row);
            yield return new WaitForSeconds(enemySpawnDelay);
        }

        Manager.Instance.busy = false;
        yield return null;
    }

    public bool CheckIfWaveIsFinished()
    {
        switch (mainObjective)
        {
            case Objective.None:
                break;
            case Objective.KillCertainAmountOfEnemies:
            case Objective.KillAllEnemies:
            case Objective.SurviveCertainAmountOfTime:
            case Objective.KillTheEliteUnit:
            case Objective.KillTheBoss:
            case Objective.SurviveThenKillElite:
            case Objective.SurviveThenKillBoss:
                if (mainObjectiveTracker >= mainObjectiveGoal) return true;
                break;
            default:
                break;
        }
        return false;
    }

    public void FinishWave()
    {
        waveInProgress = false;
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

public enum SideObjectiveEnum
{
    None,
    
}
[System.Serializable]
public class SideObjective
{
    public SideObjectiveEnum objective;
    public bool completed = false;
}
