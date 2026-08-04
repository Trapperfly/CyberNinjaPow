using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;
using Unity.Behavior;
using System.Threading;

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

    public Dictionary<Vector2Int, Sprite> healthSprites = new();

    public List<Sprite> movementSprites = new();

    public Dictionary<Vector2Int, Sprite> attackSprites = new();
    public GameObject attackArrowPrefab;

    public List<Card> damageCards = new();

    public float yOffset;
    public int timeOffset;

    public float timeAnim;
    public float moveAnimTime;
    public float collideAnimTime;
    public float attackAnimTime;
    public float addTimeAnim = 0;

    public float bobbing;
    public float bobbingSpeed;

    public int enemyMoveInterval;
    public int currentEnemyMoveTracker;
    public TMPro.TMP_Text enemyMoveText;

    private void Start()
    {
        var loaded = Resources.LoadAll<Sprite>("Sprites/UI/Grid/Health/HP_2");
        foreach (Sprite sprite in loaded)
        {
            int x = Mathf.RoundToInt(sprite.rect.x / 10);
            int y = Mathf.RoundToInt((loaded[0].texture.height - sprite.rect.y - 10) / 10);
            healthSprites.Add(new Vector2Int(x, y), sprite);
        }
        //loaded = Resources.LoadAll<Sprite>("Sprites/UI/Grid/Indicators/Enemy");
        //foreach (Sprite sprite in loaded)
        //{
        //    int x = Mathf.RoundToInt(sprite.rect.x / 22);
        //    int y = Mathf.RoundToInt((loaded[0].texture.height - sprite.rect.y - 22) / 22);
        //    movementSprites.Add(new Vector2Int(x, y), sprite);
        //}
        loaded = Resources.LoadAll<Sprite>("Sprites/UI/Grid/Indicators/melee_sheet_all");
        foreach (Sprite sprite in loaded)
        {
            int x = Mathf.RoundToInt(sprite.rect.x / 22);
            int y = Mathf.RoundToInt((loaded[0].texture.height - sprite.rect.y - 22) / 22);
            attackSprites.Add(new Vector2Int(x, y), sprite);
        }
        enemyMoveText.text = currentEnemyMoveTracker + " / " + enemyMoveInterval;
    }

    public EnemyUnit GetBlackBoardVariable(GameObject gameObject)
    {
        var agent = gameObject.GetComponent<BehaviorGraphAgent>();
        agent.GetVariable("Unit", out BlackboardVariable<EnemyUnit> unit);
        return (EnemyUnit)unit;
    }
    public void MoveAllEnemies()
    {
        foreach (EnemyUnit enemy in enemies)
        {
            foreach(EffectInfo effect in enemy.effects) {
                effect.effect.OnMove(enemy);
            }
            enemy.Move();
            foreach (EffectInfo effect in enemy.effects)
            {
                effect.effect.OnAfterMove(enemy);
            }
        }
    }

    public IEnumerator TriggerStatusesOnAllEnemies(StatusEffect statusEffect = StatusEffect.None)
    {
        foreach (EnemyUnit enemy in enemies)
        {
            TriggerStatuses(enemy, statusEffect);
            yield return new WaitForSeconds(1);
        }
    }

    public void TriggerStatuses(EnemyUnit enemy, StatusEffect statusEffect = StatusEffect.None)
    {
        
        if (statusEffect == StatusEffect.None)
            foreach (EffectInfo effect in enemy.effects)
            {
                effect.effect.OnTrigger(enemy);
            }
        else
        {
            foreach (EffectInfo effect in enemy.effects)
            {
                if (effect.effect.GiveStatus() == statusEffect) effect.effect.OnTrigger(enemy);
            }
        }
    }

    public IEnumerator IProgressTime()
    {
        Manager.Instance.itemManager.TriggerOnTimeTick();
        foreach (EnemyUnit enemy in enemies)
        {
            enemy.Timer();
        }
        foreach (EnemyUnit enemy in actingEnemies)
        {
            yield return new WaitForSeconds(timeAnim);
            Manager.Instance.itemManager.TriggerOnEnemyAct(enemy);
            //enemy.Act();
            if (!enemy.dead)
                yield return StartCoroutine(enemy.IAct());
            //yield return new WaitForSeconds(addTimeAnim);
            addTimeAnim = 0;
        }
        actingEnemies.Clear();
        KillOffEnemies();
        //Debug.Log(currentEnemyMoveTracker);
        if (currentEnemyMoveTracker >= enemyMoveInterval)
        {
            MoveAllEnemies();
            yield return new WaitForSeconds(moveAnimTime);
            currentEnemyMoveTracker = 0;
            Debug.Log("Moving all enemies");
        }
        else currentEnemyMoveTracker++;
        enemyMoveText.text = currentEnemyMoveTracker + " / " + enemyMoveInterval;
        //yield return new WaitForSeconds(timeAnim);
        yield return null;
    }

    public void AlterTime(int time)
    {
        timeOffset += time;
    }

    public int KillOffEnemies()
    {
        int deadCount = 0;
        while (deadEnemies.Count > 0)
        {
            EnemyUnit enemy = deadEnemies[0];
            enemies.Remove(enemy);
            deadEnemies.Remove(enemy);
            enemy.Die();
            deadCount++;
        }
        return deadCount;
    }

    public List<Vector2Int> GetEnemyPositions(TargetAll targetAll)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        foreach (EnemyUnit enemy in enemies)
        {
            switch (targetAll.condition)
            {
                case TargetAllCondition.None:
                    positions.Add(enemy.position);
                    break;
                case TargetAllCondition.StatusEffect:
                    if (ContainsEffect(enemy, targetAll.statusEffect)) positions.Add(enemy.position);
                    break;
                case TargetAllCondition.LowerThanNumber:
                    if (enemy.GetCurrentHealth() < targetAll.number) positions.Add(enemy.position);
                    break;
                case TargetAllCondition.HigherThanNumber:
                    if (enemy.GetCurrentHealth() > targetAll.number) positions.Add(enemy.position);
                    break;
                case TargetAllCondition.FullHealth:
                    if (enemy.GetCurrentHealth() == enemy.GetTotalHealth()) positions.Add(enemy.position);
                    break;
                default:
                    break;
            }
        }
        return positions;
    }

    public bool ContainsEffect(EnemyUnit unit, StatusEffect effect)
    {
        foreach (EffectInfo e in unit.effects)
        {
            if (e.effect.GiveStatus() == effect) return true;
        }
        return false;
    }
    public void DisplayMovementArrow(EnemyUnit unit, Vector2Int origin, Vector2Int movement)
    {
        if (unit.movementArrow != null)  Destroy(unit.movementArrow);
        if (movement == Vector2Int.zero || CheckIfCellIsOutsideOfBoard(origin + movement)) return;
        unit.movementArrow = Instantiate(enemyMovementArrowPrefab, Vector3.zero, Quaternion.identity, null);
        SpriteRenderer sprite = unit.movementArrow.GetComponent<SpriteRenderer>();
        unit.movementArrow.transform.position = Vector3.Lerp(GetWorldPos(origin), GetWorldPos(origin + movement), 0.5f);
        if (movement.x == 0 || movement.y == 0) sprite.sprite = movementSprites[0];
        else sprite.sprite = movementSprites[1];

        if (movement.x > 0 && movement.y == 0) //East
            unit.movementArrow.transform.eulerAngles = new(0, 0, 0);
        if (movement.x == 0 && movement.y < 0) //South
            unit.movementArrow.transform.eulerAngles = new(0, 0, -90);
        if (movement.x < 0 && movement.y == 0) //West
            unit.movementArrow.transform.eulerAngles = new(0, 0, -180);
        if (movement.x == 0 && movement.y > 0) //North
            unit.movementArrow.transform.eulerAngles = new(0, 0, -270);
        if (movement.x > 0 && movement.y < 0) //SouthEast
            unit.movementArrow.transform.eulerAngles = new(0, 0, 0);
        if (movement.x < 0 && movement.y < 0) //SouthWest
            unit.movementArrow.transform.eulerAngles = new(0, 0, -90);
        if (movement.x < 0 && movement.y > 0) //NorthWest
            unit.movementArrow.transform.eulerAngles = new(0, 0, -180);
        if (movement.x > 0 && movement.y > 0) //NorthEast
            unit.movementArrow.transform.eulerAngles = new(0, 0, -270);

        unit.movementArrow.transform.SetParent(unit.transform);
        unit.movementArrow.transform.SetAsFirstSibling();
    }

    public void ShowIntentionsOfEnemies()
    {
        foreach (EnemyUnit unit in Manager.Instance.enemyManager.enemies)
        {
            unit.ShowIntentions();
            //if (unit.readyToShowIntentions) 
        }
    }
    public void ShowIntentionsOfEnemy(EnemyUnit unit)
    {
        unit.ShowIntentions();
    }
    public Vector2 GetWorldPos(Vector2Int gridPosition)
    {
        BoardSpace space = null;
        Manager.Instance.boardManager.spaces.TryGetValue(gridPosition, out space);
        return space.transform.position;
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
        if (cell.x < 0) { 
            //Debug.Log("x was lower than 0"); 
            return true; }
        if (cell.x > Manager.Instance.boardManager.boardSize.x - 1) { 
            //Debug.Log("x was higher than board size"); 
            return true; }
        if (cell.y < 0) { 
            //Debug.Log("x was lower than 0"); 
            return true; }
        if (cell.y > Manager.Instance.boardManager.boardSize.y - 1) { 
            //Debug.Log("y was higher than board size"); 
            return true; }
        return false;
    }
    public bool CheckIfCellIsOutsideOfBoard(int x, int y)
    {
        //Debug.Log("Checking cell " + cell);
        if (x < 0) { 
            //Debug.Log("x was lower than 0"); 
            return true; }
        if (x > Manager.Instance.boardManager.boardSize.x - 1) { 
            //Debug.Log("x was higher than board size"); 
            return true; }
        if (y < 0) { 
            //Debug.Log("x was lower than 0"); 
            return true; }
        if (y > Manager.Instance.boardManager.boardSize.y - 1) { 
            //Debug.Log("y was higher than board size"); 
            return true; }
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
    public void SpawnEnemy(int column, int row = -1)
    {
        row = (row == -1) ? Manager.Instance.boardManager.boardSize.y - 1 : row;

        if (CheckIfCellIsOutsideOfBoard(column, row)) return;

        EnemyUnit potentialBlock = CheckIfCellIsOccupied(new(column, row));
        if (potentialBlock != null)
        {
            SpawnEnemy(column, row-1);
            return;
        }

        GameObject unitGO = Instantiate(enemyPrefab, enemyParent);

        EnemyUnit unit = unitGO.GetComponent<EnemyUnit>();

        unit.position = new(column, row);

        unit.enemy = GetRandomEnemy();

        Manager.Instance.gameManager.ChangeThreat(unit.enemy.threat);
    }

    public void ClearEnemies()
    {
        while (enemies.Count > 0)
        {
            EnemyUnit enemy = enemies[0];
            enemies.Remove(enemy);
            enemy.Die(false);
        }
        deadEnemies.Clear();
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
            EnemyInfo enemyCheck = enemyRepertoire[Random.Range(0, enemyRepertoire.Count)];
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