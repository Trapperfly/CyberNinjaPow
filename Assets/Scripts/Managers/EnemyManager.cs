using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform enemyParent;
    public List<EnemyUnit> enemies = new List<EnemyUnit>();
    public List<EnemyUnit> deadEnemies = new List<EnemyUnit>();

    public float yOffset;
    public int timeOffset;

    public float timeAnim;

    public void ProgressTime(int time = 1)
    {
        StartCoroutine(IProgressTime(time));
    }

    IEnumerator IProgressTime(int time = 1)
    {
        for (int i = 0; i < time; i++)
        {
            foreach (EnemyUnit enemy in enemies)
            {
                enemy.Timer();
                yield return new WaitForSeconds(timeAnim);
            }
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
        yield return null;
    }

    public void AlterTime(int time)
    {
        timeOffset += time;
    }

    public void CardFinished()
    {
        while (deadEnemies.Count > 0)
        {
            EnemyUnit enemy = deadEnemies[0];
            enemies.Remove(enemy);
            deadEnemies.Remove(enemy);
            enemy.Die();
        }
    }
    public bool CheckIfCellIsOccupied(Vector2Int cell)
    {
        foreach (EnemyUnit enemy in enemies)
        {
            if(enemy.position == cell) return true;
        }
        return false;
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
}
