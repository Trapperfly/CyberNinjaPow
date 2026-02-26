using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class EnemyUnit : MonoBehaviour
{
    public Vector2Int position;
    public Enemy enemy;
    public List<Intention> intentions;
    public Vector2Int intendedMovement;
    public int phase = 0;
    public int intention = 0;
    public int timer = 0;
    public int damageTaken = 0;
    public List<EffectInfo> effects;
    bool pong;

    public SpriteRenderer timerSpriteRenderer;
    public Sprite timerDot;
    public Sprite timerDanger;

    public SpriteRenderer healthBarRenderer;

    public GameObject movementArrow;

    private void Start()
    {
        SpriteRenderer spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = enemy.sprite;

        Manager.Instance.enemyManager.enemies.Add(this);
        intentions = enemy.enemyHealth[phase].intentions;
        if (enemy.looping == IntentionLooping.Random) intention = Random.Range(0, intentions.Count);

        Vector2 targetPosition = GetWorldPos(position);
        transform.localPosition = new Vector3(targetPosition.x, targetPosition.y + Manager.Instance.enemyManager.yOffset, 0);

        SetTimer();
        SetHealthBar();
        intendedMovement = PlanMovement();
    }

    void DisplayMovementArrow(Vector2Int movement)
    {
        movementArrow = Instantiate(Manager.Instance.enemyManager.enemyMovementArrowPrefab, Vector3.zero, Quaternion.identity, null);
        LineRenderer line = movementArrow.GetComponent<LineRenderer>();
        line.SetPosition(0, GetWorldPos(position));
        line.SetPosition(1, GetWorldPos(position + movement));
    }

    public Vector2 GetWorldPos(Vector2Int gridPosition)
    {
        return Manager.Instance.boardManager.spaces[gridPosition].transform.position;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log(enemy.enemyName + " took " + damage + " damage");
        damageTaken += damage;
        
        if (damageTaken >= enemy.enemyHealth[phase].gateHealth)
        {
            Debug.Log(enemy.enemyName + " took enough damage to go to next phase. Had taken " + (damageTaken - damage) + " and it took " + damage + " damage");
            NextPhase();
            return;
        }
        SetHealthBar();
    }

    public void NextPhase()
    {
        Debug.Log(enemy.enemyName + " is at phase " + phase + ", and is going to phase " + (phase + 1) + ". Its max phases is " + (enemy.enemyHealth.Count - 1));
        phase++;
        if (enemy.enemyHealth.Count - 1 < phase)
        {
            SetHealthBar();
            PrepareDie();
            return;
        }
        Debug.Log(enemy.enemyName + " changed phase to phase #" + phase);

        //Proceed to next phase
        damageTaken = 0;
        if (!enemy.enemyHealth[phase].keepPreviousIntentions)
        {
            intentions.Clear();
        }
        foreach (var intention in enemy.enemyHealth[phase].intentions)
        {
            intentions.Add(intention);
        }
        if (intention > intentions.Count) intention = 0;
        SetHealthBar();
    }

    void PrepareDie()
    {
        Manager.Instance.enemyManager.deadEnemies.Add(this);
    }

    public void Die()
    {
        Destroy(movementArrow);
        Destroy(gameObject);
    }

    void SetHealthBar()
    {
        int health = 0;
        for (int i = 0 + phase; i < enemy.enemyHealth.Count; i++)
        {
            health += enemy.enemyHealth[i].gateHealth;
        }
        healthBarRenderer.size = new(health - damageTaken, 1);
    }

    public Vector2Int PlanMovement()
    {
        Vector2Int movement = new(0, 0);
        if (intentions[intention].smartMovement != SmartMovement.None)
        {
            switch (intentions[intention].smartMovement)
            {
                case SmartMovement.None:
                    break;
                case SmartMovement.SmartDown:
                    movement = CheckMoveDirection(position, new(0, -1));
                    break;
                case SmartMovement.SmartUp:
                    break;
                case SmartMovement.SmartLeft:
                    break;
                case SmartMovement.SmartRight:
                    break;
                case SmartMovement.SmartDownX2:
                    break;
                case SmartMovement.CoverDown:
                    break;
                case SmartMovement.CoverDownX2:
                    break;
                default:
                    break;
            }
        }
        else if (intentions[intention].movement != new Vector2Int(0, 0))
        {
            movement = intentions[intention].movement;
        }

        if (movement != new Vector2Int(0, 0))
            DisplayMovementArrow(movement);

        return movement;
    }

    Vector2Int CheckMoveDirection(Vector2Int pos, Vector2Int direction)
    {
        if (Manager.Instance.enemyManager.CheckIfCellIsOccupied(pos + direction)) { }
        else if (Manager.Instance.enemyManager.CheckIfCellIsOutsideOfBoard(pos + direction)) { }
        else
        {
            Debug.Log("Down works");
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

        if (Manager.Instance.enemyManager.CheckIfCellIsOccupied(pos + nextCheck)) { }
        else if (Manager.Instance.enemyManager.CheckIfCellIsOutsideOfBoard(pos + nextCheck)) { }
        else
        {
            Debug.Log("Next test works");
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

        if (Manager.Instance.enemyManager.CheckIfCellIsOccupied(pos + lastCheck) != null) { }
        else if (Manager.Instance.enemyManager.CheckIfCellIsOutsideOfBoard(pos + lastCheck)) { }
        else
        {
            Debug.Log("Last check works");
            return lastCheck;
        }
        Debug.Log("Nothing works, probably do nothing");
        return direction;
    }

    public void Timer()
    {  
        if (intentions[intention].timer > timer) {
            timer++;
            SetTimer();
            return; 
        }
        EffectOnTimer();
        Act();
        EffectOnAfterTimer();
        timer = 0;

        SetTimer();
    }

    public void SetTimer()
    {
        int curTimer = intentions[intention].timer;
        timerSpriteRenderer.size = new(curTimer - timer, 1);
        if (curTimer <= timer)
        {
            timerSpriteRenderer.sprite = timerDanger;
            timerSpriteRenderer.size = new(1, 1);
        }
        else timerSpriteRenderer.sprite = timerDot;
    }

    public void Act()
    {
        Manager.Instance.enemyManager.addTimeAnim = 1;
        EffectOnAct();
        Move();
        Attack();
        ApplyEffect();
        EffectOnAfterAct();
        switch (enemy.looping)
        {
            case IntentionLooping.none:
                if(intentions.Count - 1 > intention) intention += 1;
                break;
            case IntentionLooping.Loop:
                if (intentions.Count - 1 <= intention)
                    intention = 0;
                else intention += 1;
                break;
            case IntentionLooping.ReverseLoop:
                if (intention == 0)
                    intention = intentions.Count - 1;
                else intention -= 1;
                break;
            case IntentionLooping.PingPong:
                if (!pong)
                {
                    if (intentions.Count - 1 == intention)
                    {
                        pong = true;
                        intention -= 1;
                    }
                    else intention += 1;
                }
                else
                {
                    if (intention == 0)
                    {
                        pong = false;
                        intention += 1;
                    }
                    else intention -= 1;
                }
                break;
            case IntentionLooping.RepeatEnd:
                if (intentions.Count - 1 > intention) intention += 1;
                break;
            case IntentionLooping.Random:
                intention = Random.Range(0, intentions.Count);
                break;
            case IntentionLooping.RandomStart:
                break;
            default:
                break;
        }
        intendedMovement = PlanMovement();
    }

    public void EffectOnAct()
    {
        foreach (EffectInfo effect in effects)
        {
            effect.effect.OnAct(this);
        }
    }
    public void EffectOnAfterAct()
    {
        foreach (EffectInfo effect in effects)
        {
            effect.effect.OnAfterAct(this);
        }
    }
    public void EffectOnTimer()
    {
        foreach (EffectInfo effect in effects)
        {
            effect.effect.OnAct(this);
        }
    }
    public void EffectOnAfterTimer()
    {
        foreach (EffectInfo effect in effects)
        {
            effect.effect.OnAfterAct(this);
        }
    }

    public void Move()
    {
        if (movementArrow != null)
            Destroy(movementArrow);
        EnemyUnit potentialCrash = Manager.Instance.enemyManager.CheckIfCellIsOccupied(position + intendedMovement);
        if (intendedMovement != new Vector2Int(0, 0) && potentialCrash != null) {
            TakeDamage(Manager.Instance.gameManager.collisionDamage);
            potentialCrash.TakeDamage(Manager.Instance.gameManager.collisionDamage);
            return;
        }
        if (Manager.Instance.enemyManager.CheckIfCellIsOutsideOfBoard(position + intendedMovement)) return;

        position += intendedMovement;
        Vector2 targetPosition = Manager.Instance.boardManager.spaces[position].transform.position;
        transform.localPosition = new Vector3(targetPosition.x, targetPosition.y + Manager.Instance.enemyManager.yOffset, 0);
    }

    public void ForceMove(Vector2Int direction)
    {
        if (movementArrow != null)
            Destroy(movementArrow);
        EnemyUnit potentialCrash = Manager.Instance.enemyManager.CheckIfCellIsOccupied(position + direction);
        if (potentialCrash != null)
        {
            TakeDamage(Manager.Instance.gameManager.collisionDamage);
            potentialCrash.TakeDamage(Manager.Instance.gameManager.collisionDamage);
            return;
        }
        if (Manager.Instance.enemyManager.CheckIfCellIsOutsideOfBoard(position + direction)) return;

        position += direction;
        Vector2 targetPosition = Manager.Instance.boardManager.spaces[position].transform.position;
        transform.localPosition = new Vector3(targetPosition.x, targetPosition.y + Manager.Instance.enemyManager.yOffset, 0);

        DisplayMovementArrow(intendedMovement);
    }

    public void Attack()
    {

    }

    public void ApplyEffect()
    {
        switch (intentions[intention].effect.effect)
        {
            case EffectsEnum.none:
                break;
            case EffectsEnum.Poison:
                break;
            case EffectsEnum.Regeneration:
                break;
            case EffectsEnum.TimeWarp:
                Manager.Instance.enemyManager.AlterTime(intentions[intention].effect.amount);
                break;
            default:
                break;
        }
    }
}
