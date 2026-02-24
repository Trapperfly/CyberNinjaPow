using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform enemyParent;
    public List<EnemyUnit> enemies = new List<EnemyUnit>();
    public List<EnemyUnit> deadEnemies = new List<EnemyUnit>();

    public float yOffset;

    public void ProgressTime(int time = 1)
    {
        for (int i = 0; i < time; i++)
        {
            foreach (EnemyUnit enemy in enemies)
            {
                enemy.Timer();
            }
        }
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
