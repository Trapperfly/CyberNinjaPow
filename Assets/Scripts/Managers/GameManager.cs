using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System.Globalization;
public class GameManager : MonoBehaviour
{
    EnemyManager enemyManager;
    public string seed = "";

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

    public System.Random gameSeed = new();

    private void Awake()
    {

        if (seed != "") gameSeed = new System.Random(seed.GetHashCode());

    }
    public float NextFloat(float lowerBound, float upperBound)
    {
        float result = NextFloat(lowerBound, upperBound, gameSeed);
        return result;
    }
    static float NextFloat(float lowerBound, float upperBound, System.Random seed)
    {
        int tenths = 10000000;

        float result = seed.Next(Mathf.RoundToInt(lowerBound * tenths), Mathf.RoundToInt(upperBound * tenths));

        result /= tenths;
        
        return result;
    }

    //public static int DecimalSpaces(float value)
    //{
    //    string text = value.ToString("G10", CultureInfo.InvariantCulture);

    //    text.TrimEnd('0');

    //    int index = text.IndexOf('.');

    //    if (index == -1) return 0;

    //    return text.Length - index - 1;
    //}

    private void Start()
    {
        enemyManager = Manager.Instance.enemyManager;
        AlterMoney(0);
        //StartWave();
        Manager.Instance.tutorialManager.ShowTutorial(Tutorials.StartOfGame);
        //for (int i = 0; i < Manager.Instance.boardManager.boardSize.x; i++)
        //{
        //    int timer = NextFloat(spawnDelay.x, spawnDelay.y + 1);
        //    spawnTimerForColumns.Add(timer);
        //}

        //StartCoroutine(AdvanceBoard(startTimeTimes, startTimeProgress));
    }

    private void Update()
    {
        //if (!waveInProgress && Input.GetKeyDown(KeyCode.Space)) StartWave();
        if (waveInProgress && Input.GetKeyDown(KeyCode.S)) FinishWave();
    }

    public void AlterMoney(int amount)
    {
        money += amount;
        MoneyText.text = money.ToString();
        Manager.Instance.shopManager.moneyText.text = money.ToString() + "$";
    }

    public void ProgressTime(int time)
    {

        StartCoroutine(IProgressTimeProgressively(time));
    }

    public IEnumerator IProgressTimeProgressively(int time)
    {
        Manager.Instance.busy = true;
        for (int i = 0; i < time; i++)
        {
            switch (mainObjective)
            {
                case Objective.SurviveCertainAmountOfTime:
                case Objective.SurviveThenKillElite:
                case Objective.SurviveThenKillBoss:
                    mainObjectiveTracker++;
                    break;
            }

            yield return StartCoroutine(enemyManager.IProgressTime());
            yield return new WaitForSeconds(0.1f);
            yield return null;
        }
        AfterTimeProgress();
        yield return null;

        Manager.Instance.playerManager.ResetBlock();

        Manager.Instance.busy = false;
    }

    public void AfterTimeProgress()
    {
        foreach (EnemyUnit enemy in enemyManager.enemies)
        {
            enemy.timer += enemyManager.timeOffset;
            //Debug.Log("Added " + timeOffset + " to " + enemy.enemy.enemyName);
            enemy.SetTimer();
        }
        enemyManager.timeOffset = 0;

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
    //                spawnTimerForColumns[j] = NextFloat(spawnDelay.x, spawnDelay.y);
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
        Manager.Instance.boardManager.enemyKilled = true;

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

    public void HitAnEnemy(EnemyUnit unit)
    {
        Manager.Instance.boardManager.enemyTakenDamage = true;
    }

    public void ProgressSpawn()
    {
        float spawningBias = Mathf.Pow(1f - (currentThreat / maxThreat), threatCalculation);
        //Debug.Log("Bias for spawning an enemy is " + spawningBias * 100 + "%");
        if (NextFloat(0f,1f) < spawningBias)
        {
            Debug.Log("Spawning an enemy. It was " + spawningBias * 100 + "% chance for it to spawn.");

            enemyManager.SpawnEnemy(gameSeed.Next(0,5));
        }
    }

    public void StartWave()
    {
        if (waveInProgress) return;

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
                mainObjectiveGoal = 30;
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

        Manager.Instance.deckManager.LoadDeck();

        for (int i = 0; i < Manager.Instance.deckManager.handSize; i++)
        {
            Manager.Instance.deckManager.DrawCard();
            yield return new WaitForSeconds(Manager.Instance.deckManager.drawAnimTime);
        }

        for (int i = 0; i < startEnemyCount; i++)
        {
            int column = gameSeed.Next(0, Manager.Instance.boardManager.boardSize.x);
            int row = gameSeed.Next(Manager.Instance.boardManager.boardSize.y - startEnemyYPosition, Manager.Instance.boardManager.boardSize.y);

            enemyManager.SpawnEnemy(column, row);
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

        Manager.Instance.deckManager.SaveDeck();

        Manager.Instance.enemyManager.ClearEnemies();

        Manager.Instance.boardManager.ClearSpaces();

        OpenShop();
    }

    public void OpenRewards(ShopQuality quality = ShopQuality.Normal)
    {

    }
    public void OpenShop()
    {
        Manager.Instance.shopManager.GenerateShop();
        Manager.Instance.shopManager.shopCanvas.gameObject.SetActive(true);
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