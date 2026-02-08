using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public List<EnemyUnit> enemies = new List<EnemyUnit>();

    public void Act()
    {
        foreach (EnemyUnit enemy in enemies)
        {
            enemy.Act();
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
        Debug.Log("Checking cell " + cell);
        if (cell.x < 0) { Debug.Log("x was lower than 0"); return true; }
        if (cell.x > Manager.Instance.boardManager.boardSize.x - 1) { Debug.Log("x was higher than board size"); return true; }
        if (cell.y < 0) { Debug.Log("x was lower than 0"); return true; }
        if (cell.y > Manager.Instance.boardManager.boardSize.y - 1) { Debug.Log("y was higher than board size"); return true; }
        return false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Act();
    }
}
