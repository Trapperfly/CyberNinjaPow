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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Act();
    }
}
