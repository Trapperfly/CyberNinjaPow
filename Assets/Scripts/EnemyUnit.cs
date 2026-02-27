using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System.Collections;

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

    SpriteRenderer spriteRenderer;

    public SpriteRenderer timerSpriteRenderer;
    public Sprite timerDot;
    public Sprite timerDanger;

    public SpriteRenderer healthBarRenderer;

    public GameObject movementArrow;

    EnemyManager enemyManager;

    float bobbingOffset;

    private void Start()
    {
        enemyManager = Manager.Instance.enemyManager;
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = enemy.sprite;

        enemyManager.enemies.Add(this);
        intentions = enemy.enemyHealth[phase].intentions;
        if (enemy.looping == IntentionLooping.Random) intention = Random.Range(0, intentions.Count);

        Vector2 targetPosition = GetWorldPos(position);
        transform.localPosition = new Vector3(targetPosition.x, targetPosition.y + enemyManager.yOffset, 0);

        SetTimer();
        SetHealthBar();
        intendedMovement = PlanMovement();

        bobbingOffset = Random.Range(0, 9000f);
    }

    private void Update()
    {
        //Debug.Log(Mathf.Sin(Time.time));
        spriteRenderer.transform.localPosition = Mathf.Sin(Time.time * enemyManager.bobbingSpeed + bobbingOffset) * enemyManager.bobbing * new Vector3(0, 1, 0);
    }

    void DisplayMovementArrow(Vector2Int movement)
    {
        movementArrow = Instantiate(enemyManager.enemyMovementArrowPrefab, Vector3.zero, Quaternion.identity, null);
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
        //Debug.Log(enemy.enemyName + " took " + damage + " damage");
        damageTaken += damage;
        
        if (damageTaken >= enemy.enemyHealth[phase].gateHealth)
        {
            //Debug.Log(enemy.enemyName + " took enough damage to go to next phase. Had taken " + (damageTaken - damage) + " and it took " + damage + " damage");
            NextPhase();
            return;
        }
        SetHealthBar();
    }

    public void NextPhase()
    {
        //Debug.Log(enemy.enemyName + " is at phase " + phase + ", and is going to phase " + (phase + 1) + ". Its max phases is " + (enemy.enemyHealth.Count - 1));
        phase++;
        if (enemy.enemyHealth.Count - 1 < phase)
        {
            SetHealthBar();
            PrepareDie();
            return;
        }
        //Debug.Log(enemy.enemyName + " changed phase to phase #" + phase);

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
        enemyManager.deadEnemies.Add(this);
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
                    movement = enemyManager.CheckMoveDirection(position, new(0, -1));
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
        else
        {
            movement = intentions[intention].movement;
        }

        if (movement != new Vector2Int(0, 0))
            DisplayMovementArrow(movement);

        return movement;
    }

    public void Timer()
    {  
        EffectOnTimer();
        if (intentions[intention].timer > timer) {
            timer++;
            SetTimer();
            return; 
        }
        enemyManager.actingEnemies.Add(this);
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
        StartCoroutine(IAct());
    }

    public IEnumerator IAct()
    {
        EffectOnAct();
        Move();
        if (intendedMovement != new Vector2Int(0, 0))
        {
            enemyManager.addTimeAnim += enemyManager.moveAnimTime;
            yield return new WaitForSeconds(enemyManager.moveAnimTime);
        }
        Attack();
        if (intentions[intention].attack.Count != 0)
        {
            Debug.Log(enemy.enemyName + " is attacking with " + intentions[intention].attack.Count + " attacks");
            enemyManager.addTimeAnim += enemyManager.attackAnimTime;
            yield return new WaitForSeconds(enemyManager.attackAnimTime);
        }
        ApplyEffect();
        EffectOnAfterAct();
        FindLooping();
        timer = 0;
        SetTimer();
        intendedMovement = PlanMovement();
        yield return null;
    }

    public void FindLooping()
    {
        switch (enemy.looping)
        {
            case IntentionLooping.none:
                if (intentions.Count - 1 > intention) intention += 1;
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

        if (intendedMovement == new Vector2Int(0, 0))
        {
            return;
        }

        EnemyUnit potentialCrash = enemyManager.CheckIfCellIsOccupied(position + intendedMovement);

        if (intendedMovement != new Vector2Int(0, 0) && potentialCrash != null) {
            TakeDamage(Manager.Instance.gameManager.collisionDamage);
            potentialCrash.TakeDamage(Manager.Instance.gameManager.collisionDamage);
            return;
        }
        if (enemyManager.CheckIfCellIsOutsideOfBoard(position + intendedMovement)) return;

        position += intendedMovement;

        StartCoroutine(MoveLerp(position));
        //Vector2 targetPosition = Manager.Instance.boardManager.spaces[position].transform.position;
        //transform.localPosition = new Vector3(targetPosition.x, targetPosition.y + enemyManager.yOffset, 0);
    }

    public void ForceMove(Vector2Int direction)
    {
        if (movementArrow != null)
            Destroy(movementArrow);
        EnemyUnit potentialCrash = enemyManager.CheckIfCellIsOccupied(position + direction);
        if (potentialCrash != null)
        {
            TakeDamage(Manager.Instance.gameManager.collisionDamage);
            potentialCrash.TakeDamage(Manager.Instance.gameManager.collisionDamage);
            return;
        }
        if (enemyManager.CheckIfCellIsOutsideOfBoard(position + direction)) return;

        position += direction;

        StartCoroutine(MoveLerp(position, true));
        //Vector2 targetPosition = Manager.Instance.boardManager.spaces[position].transform.position;
        //transform.localPosition = new Vector3(targetPosition.x, targetPosition.y + enemyManager.yOffset, 0);
    }

    public IEnumerator MoveLerp(Vector2Int gridPos, bool forced = false)
    {
        float seconds = 0.5f;
        float i = 0;
        Vector2 originalPos = transform.position;
        Vector2 targetPos = Manager.Instance.boardManager.spaces[gridPos].transform.position;
        while (i < seconds)
        {
            i += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(originalPos, new Vector3(targetPos.x, targetPos.y + enemyManager.yOffset, 0), i * 2);
            yield return null;
        }
        if (forced) intendedMovement = PlanMovement();
        yield return null;
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
                enemyManager.AlterTime(intentions[intention].effect.amount);
                break;
            default:
                break;
        }
    }
}
