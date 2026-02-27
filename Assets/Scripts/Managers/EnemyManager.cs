using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject enemyMovementArrowPrefab;
    public Transform enemyParent;
    public int spawningFunds = 10;
    public int boardCost = 0;
    public List<EnemyInfo> enemyRepertoire = new List<EnemyInfo>();
    public List<Enemy> enemyQueue = new List<Enemy>();
    public List<EnemyUnit> enemies = new List<EnemyUnit>();
    public List<EnemyUnit> actingEnemies = new();
    public List<EnemyUnit> deadEnemies = new List<EnemyUnit>();

    public float yOffset;
    public int timeOffset;

    public float timeAnim;
    public float moveAnimTime;
    public float attackAnimTime;
    public float addTimeAnim = 0;

    public float bobbing;
    public float bobbingSpeed;

    public void MoveAllEnemies(int times, Vector2Int direction)
    {
        for (int t = 0; t < times; t++)
        {
            for (int y = 0; y < Manager.Instance.boardManager.boardSize.y; y++)
            {
                foreach (EnemyUnit enemy in enemies)
                {
                    if (enemy.position.y == y) enemy.ForceMove(direction);
                }
            }
        }
    }

    public void ProgressTime(int time = 1)
    {
        StartCoroutine(IProgressTime(time));
    }

    IEnumerator IProgressTime(int time = 1)
    {
        Manager.Instance.busy = true;
        for (int i = 0; i < time; i++)
        {
            foreach (EnemyUnit enemy in enemies)
            {
                enemy.Timer();
            }
            foreach (EnemyUnit enemy in actingEnemies)
            {
                yield return new WaitForSeconds(timeAnim);
                enemy.Act();
                yield return new WaitForSeconds(addTimeAnim);
                addTimeAnim = 0;
            }
            actingEnemies.Clear();
            KillOffEnemies();
            yield return new WaitForSeconds(timeAnim);
        }
        foreach (EnemyUnit enemy in enemies)
        {
            enemy.timer += timeOffset;
            //Debug.Log("Added " + timeOffset + " to " + enemy.enemy.enemyName);
            enemy.SetTimer();
        }
        timeOffset = 0;
        Manager.Instance.gameManager.ProgressSpawn(time);
        Manager.Instance.busy = false;
        yield return null;
    }

    public void AlterTime(int time)
    {
        timeOffset += time;
    }

    public void KillOffEnemies()
    {
        while (deadEnemies.Count > 0)
        {
            EnemyUnit enemy = deadEnemies[0];
            enemies.Remove(enemy);
            deadEnemies.Remove(enemy);
            enemy.Die();
        }
    }
    public EnemyUnit CheckIfCellIsOccupied(Vector2Int cell)
    {
        foreach (EnemyUnit enemy in enemies)
        {
            if(enemy.position == cell) return enemy;
        }
        return null;
    }

    public bool CheckIfCellIsOutsideOfBoard(Vector2Int cell)
    {
        //Debug.Log("Checking cell " + cell);
        if (cell.x < 0) { Debug.Log("x was lower than 0"); return true; }
        if (cell.x > Manager.Instance.boardManager.boardSize.x - 1) { Debug.Log("x was higher than board size"); return true; }
        if (cell.y < 0) { Debug.Log("x was lower than 0"); return true; }
        if (cell.y > Manager.Instance.boardManager.boardSize.y - 1) { Debug.Log("y was higher than board size"); return true; }
        return false;
    }

    public Vector2Int CheckMoveDirection(Vector2Int pos, Vector2Int direction)
    {
        if (CheckIfCellIsOccupied(pos + direction)) { }
        else if (CheckIfCellIsOutsideOfBoard(pos + direction)) { }
        else
        {
            //Debug.Log("Down works");
            return direction;
        }
        Vector2Int nextCheck = new Vector2Int(0, 0);
        float value = Random.value;
        if (direction.x == 0)
        {
            nextCheck = (value < 0.5f) ? new(-1, direction.y) : new(1, direction.y);
        }
        else if (direction.y == 0)
        {
            nextCheck = (value < 0.5f) ? new(direction.x, -1) : new(direction.x, 1);
        }

        if (CheckIfCellIsOccupied(pos + nextCheck)) { }
        else if (CheckIfCellIsOutsideOfBoard(pos + nextCheck)) { }
        else
        {
            //Debug.Log("Next test works");
            return nextCheck;
        }

        Vector2Int lastCheck = new Vector2Int(0, 0);
        if (direction.x == 0)
        {
            lastCheck = (value > 0.5f) ? new(1, direction.y) : new(-1, direction.y);
        }
        else if (direction.y == 0)
        {
            lastCheck = (value > 0.5f) ? new(direction.x, 1) : new(direction.x, -1);
        }

        if (CheckIfCellIsOccupied(pos + lastCheck) != null) { }
        else if (CheckIfCellIsOutsideOfBoard(pos + lastCheck)) { }
        else
        {
            //Debug.Log("Last check works");
            return lastCheck;
        }
        //Debug.Log("Nothing works, probably do nothing");
        return direction;
    }
    public void SpawnEnemy(int column)
    {
        //if (enemyQueue.Count == 0) return;

        EnemyUnit potentialBlock = CheckIfCellIsOccupied(new(column, Manager.Instance.boardManager.boardSize.y - 1));
        if (potentialBlock != null)
        {
            potentialBlock.TakeDamage(Manager.Instance.gameManager.collisionDamage);
            return;
        }

        GameObject unitGO = Instantiate(enemyPrefab, enemyParent);

        EnemyUnit unit = unitGO.GetComponent<EnemyUnit>();

        unit.position = new(column, Manager.Instance.boardManager.boardSize.y - 1);

        unit.enemy = GetRandomEnemy();

        //unit.enemy = enemyQueue[0];

        //enemyQueue.RemoveAt(0);
    }

    public List<Enemy> GetRandomEnemies(int amount = 0, int funds = 0)
    {
        List<Enemy> enemyList = new List<Enemy>();

        Vector2Int minMax = GetMinCostMaxCost();

        if (funds == 0) { funds = minMax.y; }
        if (amount == 0) { amount = 1; }

        int i = 0;
        while (i < amount || funds > 0)
        {
            EnemyInfo enemyCheck = enemyRepertoire[Random.Range(0, enemyRepertoire.Count - 1)];
            if (enemyCheck.cost <= funds) {
                enemyList.Add(enemyCheck.enemy); 
                i++;
                funds -= enemyCheck.cost;
            }
        }

        return enemyList;
    }
    public Enemy GetRandomEnemy(int funds = 0)
    {
        Enemy enemy;

        Vector2Int minMax = GetMinCostMaxCost();

        if (funds == 0) { funds = minMax.y; }

        while (funds > 0)
        {
            EnemyInfo enemyCheck = enemyRepertoire[Random.Range(0, enemyRepertoire.Count - 1)];
            if (enemyCheck.cost <= funds)
            {
                enemy = enemyCheck.enemy;
                return enemy;
            }
        }
        Debug.Log(minMax + " " + funds);
        return null;
    }

    public Vector2Int GetMinCostMaxCost()
    {
        int minCost = 0;
        int maxCost = 0;
        foreach (EnemyInfo eInfo in enemyRepertoire)
        {
            if (minCost == 0) { minCost = eInfo.cost; }
            else if (minCost > eInfo.cost) { minCost = eInfo.cost; }

            if (maxCost == 0) { maxCost = eInfo.cost; }
            else if (maxCost < eInfo.cost) { maxCost = eInfo.cost; }
        }

        return new(minCost, maxCost);
    }
}