using UnityEngine;
using System.Collections.Generic;
using Unity.Behavior;
using System.Collections;

public class EnemyUnit : MonoBehaviour
{
    public Vector2Int position;
    //public Enemy enemy;
    public EnemyData enemy;
    BehaviorGraphAgent agent;
    public bool attacking;
    public List<TileEffect> intendedAttack;
    public int iAttackCounter = -1;
    public int attackRange;
    public Vector2Int intendedMovement;
    public int actionTimer = 0;
    public int timer = 0;
    public int phase = 0;
    public int damageTaken = 0;
    public List<EffectInfo> effects;
    bool pong;
    bool dead;
    public bool readyToShowIntentions = false;

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
        agent = GetComponent<BehaviorGraphAgent>();
        
        enemyManager = Manager.Instance.enemyManager;
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = enemy.phases[phase].sprite;

        enemyManager.enemies.Add(this);
        //if (enemy.looping == IntentionLooping.Random) intention = Random.Range(0, intentions.Count);

        Vector2 targetPosition = GetWorldPos(position);
        transform.localPosition = new Vector3(targetPosition.x, targetPosition.y + enemyManager.yOffset, 0);
        
        StartCoroutine(GetAndDisplayIntentionsRightAfterSpawn());

        SetHealthBar();
        //intendedMovement = PlanMovement();

        bobbingOffset = Random.Range(0, 9000f);
    }

    public void Act()
    {
        StartCoroutine(IAct());
    }

    public IEnumerator IAct()
    {
        Vector2Int doingMove = CheckMove();

        List<TileEffect> attacks = new List<TileEffect>();
        foreach (TileEffect effect in intendedAttack)
        {
            attacks.Add(effect);
        }

        GetIntentions();

        StartCoroutine(Move(position)); //Move sprite to actual position
        if (doingMove != new Vector2Int(0, 0))
        {
            enemyManager.addTimeAnim += enemyManager.moveAnimTime;
        }
        yield return new WaitForSeconds(enemyManager.moveAnimTime);

        if (attacking)
        {
            StartCoroutine(Attack());
            if (intendedAttack.Count > 0)
            {
                enemyManager.addTimeAnim += enemyManager.attackAnimTime;
            }
            yield return new WaitForSeconds(enemyManager.attackAnimTime);
        }

        StartCoroutine(ShowIntentionsWhenReady());

        timer = 0;
        SetTimer();

        yield return null;
    }

    private void GetIntentions()
    {
        if (dead) return;

        readyToShowIntentions = false;

        intendedAttack.Clear();
        iAttackCounter = -1;
        agent.Graph = null;
        agent.Graph = Instantiate(enemy.phases[phase].actions);
        agent.Init();

        if (agent.GetVariable("IntentionsReady", out BlackboardVariable<bool> ready))
            ready.Value = false;
        if (agent.GetVariable("Unit", out BlackboardVariable<EnemyUnit> unit))
            unit.Value = this;

        agent.Graph.Restart();
    }
    IEnumerator GetAndDisplayIntentionsRightAfterSpawn()
    {
        yield return new WaitUntil(() =>
        {
            return agent.isActiveAndEnabled;
        });

        GetIntentions();

        StartCoroutine(ShowIntentionsWhenReady());
    }

    IEnumerator ShowIntentionsWhenReady()
    {
        agent.GetVariable("IntentionsReady", out BlackboardVariable<bool> ready);

        yield return new WaitUntil(() =>
        {
            agent.GetVariable("IntentionsReady", out BlackboardVariable<bool> r);
            return r?.Value == true;
        });
        readyToShowIntentions = true;
        ShowIntentions();
        yield return null;
    }

    private void Update()
    {
        //Debug.Log(Mathf.Sin(Time.time));
        spriteRenderer.transform.localPosition = Mathf.Sin(Time.time * enemyManager.bobbingSpeed + bobbingOffset) * enemyManager.bobbing * new Vector3(0, 1, 0);
    }

    public void PaintAttack()
    {
        Manager.Instance.boardManager.PaintAttack(intendedAttack, position);
    }

    public void ShowIntentions()
    {
        attackRange = enemy.phases[phase].attackRange;
        attacking = (position.y <= attackRange) ? true : false;

        if (attacking) PaintAttack();
        Manager.Instance.enemyManager.DisplayMovementArrow(this, position, intendedMovement);
    }

    public Vector2 GetWorldPos(Vector2Int gridPosition)
    {
        return Manager.Instance.boardManager.spaces[gridPosition].transform.position;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log(enemy.enemyName + " took " + damage + " damage");
        damageTaken += damage;
        
        if (damageTaken >= enemy.phases[phase].health)
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
        if (enemy.phases.Count - 1 < phase)
        {
            dead = true;
            SetHealthBar();
            PrepareDie();
            return;
        }
        //Debug.Log(enemy.enemyName + " changed phase to phase #" + phase);

        //Proceed to next phase
        damageTaken = 0;
        spriteRenderer.sprite = enemy.phases[phase].sprite;
        intendedMovement = Vector2Int.zero;
        intendedAttack.Clear();

        ShowIntentions();

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
        for (int i = 0 + phase; i < enemy.phases.Count; i++)
        {
            health += enemy.phases[i].health;
        }
        int currentHealth = health - damageTaken;
        if (currentHealth < 0) currentHealth = 0;
        healthBarRenderer.size = new(currentHealth, 1);
    }

    public Vector2Int PlanSmartMovement(SmartMovement smartMovement)
    {
        Vector2Int movement = new(0, 0);
        switch (smartMovement)
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
        return movement;
    }

    public void Timer()
    {  
        EffectOnTimer();
        if (actionTimer > timer) {
            timer++;
            SetTimer();
            return; 
        }
        enemyManager.actingEnemies.Add(this);
    }

    public void SetTimer()
    {
        timerSpriteRenderer.size = new(actionTimer - timer, 1);
        if (actionTimer <= timer)
        {
            timerSpriteRenderer.sprite = timerDanger;
            timerSpriteRenderer.size = new(1, 1);
        }
        else timerSpriteRenderer.sprite = timerDot;
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

    public Vector2Int CheckMove()
    {
        if (movementArrow != null)
            Destroy(movementArrow);

        if (intendedMovement == new Vector2Int(0, 0))
        {
            return new(0,0);
        }

        EnemyUnit potentialCrash = enemyManager.CheckIfCellIsOccupied(position + intendedMovement);

        if (intendedMovement != new Vector2Int(0, 0) && potentialCrash != null) {
            TakeDamage(Manager.Instance.gameManager.collisionDamage);
            potentialCrash.TakeDamage(Manager.Instance.gameManager.collisionDamage);
            return new(0,0);
        }
        if (enemyManager.CheckIfCellIsOutsideOfBoard(position + intendedMovement)) return new(0,0);

        position += intendedMovement;

        return intendedMovement;

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

        StartCoroutine(Move(position, true));
        //Vector2 targetPosition = Manager.Instance.boardManager.spaces[position].transform.position;
        //transform.localPosition = new Vector3(targetPosition.x, targetPosition.y + enemyManager.yOffset, 0);
    }

    public IEnumerator Move(Vector2Int moveTo, bool forced = false)
    {
        readyToShowIntentions = false;
        Manager.Instance.boardManager.ClearSpaces();
        float seconds = 0.5f;
        float i = 0;
        Vector2 originalPos = transform.position;
        Vector2 targetPos = Manager.Instance.boardManager.spaces[moveTo].transform.position;
        while (i < seconds)
        {
            i += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(originalPos, new Vector3(targetPos.x, targetPos.y + enemyManager.yOffset, 0), i * 2);
            yield return null;
        }
        readyToShowIntentions = true;
        if (forced) { 
            Manager.Instance.boardManager.ClearSpaces();
        }
        //Manager.Instance.boardManager.ClearSpaces();
        yield return null;
    }

    IEnumerator Attack()
    {
        readyToShowIntentions = false;

        foreach (TileEffect attack in intendedAttack)
        {
            foreach (ProjectileData projectile in attack.projectiles)
            {
                Manager.Instance.boardManager.Projectile(
                    true, 
                    GridSpaceSelection.EnemyAttack, 
                    position + attack.gridPosition, projectile.direction, 
                    attack.damage, projectile.pierce, 
                    enemy.phases[phase].damageCard);
            }
        }

        yield return null;
    }
}
