using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Behavior;
using UnityEngine;

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
    public bool dead;
    public bool readyToShowIntentions = false;

    SpriteRenderer spriteRenderer;

    public SpriteRenderer timerSpriteRenderer;
    public Sprite timerDot;
    public Sprite timerDanger;

    public Transform healthParent;
    public GameObject healthbarSegmentPrefab;
    public GameObject healthbarGatePrefab;
    public List<Sprite> healthSprites;
    public float healthBarSpread = 1f;

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


        CreateHealthBar();
        //intendedMovement = PlanMovement();

        bobbingOffset = Random.Range(0, 9000f);
    }

    public void Act()
    {
        if (dead) return;
        StartCoroutine(IAct());
    }

    public IEnumerator IAct()
    {
        Vector2Int doingMove = CheckMove();
        //if (doingMove != new Vector2Int(0, 0))
        //{
        //    enemyManager.addTimeAnim += enemyManager.moveAnimTime;
        //}
        //if (attacking && intendedAttack.Count > 0)
        //{
        //    enemyManager.addTimeAnim += enemyManager.attackAnimTime * intendedAttack.Count;
        //}
        List<TileEffect> attacks = new List<TileEffect>();
        foreach (TileEffect effect in intendedAttack)
        {
            attacks.Add(effect);
        }

        GetIntentions();

        StartCoroutine(Move(position)); //Move sprite to actual position

        if (doingMove != new Vector2Int(0, 0))
        {
            yield return new WaitForSeconds(enemyManager.moveAnimTime);
        }

        if (attacking)
        {
            StartCoroutine(Attack());

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
        attackRange = enemy.phases[phase].attackRange;
        attacking = (position.y <= attackRange) ? true : false;

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
        if (dead) return;
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
        if (dead || damage == 0) { return; }

        //Debug.Log(enemy.enemyName + " took " + damage + " damage");
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
        
        if (enemy.phases.Count - 1 <= phase)
        {
            dead = true;
            SetHealthBar();
            PrepareDie();
            return;
        }
        //Debug.Log(enemy.enemyName + " changed phase to phase #" + phase);

        //Proceed to next phase
        BreakPhase();

        phase++;

        damageTaken = 0;
        spriteRenderer.sprite = enemy.phases[phase].sprite;
        intendedMovement = Vector2Int.zero;
        intendedAttack.Clear();

        ShowIntentions();

        SetHealthBar();
    }

    public void PrepareDie()
    {
        enemyManager.deadEnemies.Add(this);
    }

    public void Die(bool rewards = true)
    {
        if (rewards) Manager.Instance.gameManager.AlterMoney((int)enemy.strength);
        Manager.Instance.gameManager.KilledAnEnemy(enemy.threat, enemy.strength);
        Destroy(movementArrow);
        Destroy(gameObject);
    }

    void CreateHealthBar()
    {
        for (int p = 0; p < enemy.phases.Count; p++)
        {
            for (int h = 0; h < enemy.phases[p].health; h++)
            {
                if (p == 0) //First phase
                {
                    if (p == 0 && h == 0) //Start of healthbar
                    {
                        Instantiate(healthbarSegmentPrefab, healthParent).GetComponent<SpriteRenderer>().sprite = healthSprites[0];
                    }
                    else if (enemy.phases.Count == 1 && h == enemy.phases[p].health - 1) //End of health bar if 1 phase only
                    {
                        Instantiate(healthbarSegmentPrefab, healthParent).GetComponent<SpriteRenderer>().sprite = healthSprites[2];
                    }
                    else Instantiate(healthbarSegmentPrefab, healthParent).GetComponent<SpriteRenderer>().sprite = healthSprites[1];
                }
                else
                {
                    if (p == enemy.phases.Count - 1 && h == enemy.phases[p].health - 1) //End of health bar
                    {
                        Instantiate(healthbarSegmentPrefab, healthParent).GetComponent<SpriteRenderer>().sprite = healthSprites[5];
                    }
                    else Instantiate(healthbarSegmentPrefab, healthParent).GetComponent<SpriteRenderer>().sprite = healthSprites[4];
                }
            }
            if (p != enemy.phases.Count - 1)
                Instantiate(healthbarSegmentPrefab, healthParent).GetComponent<SpriteRenderer>().sprite = healthSprites[3]; //Gate
        }

        AlignHealthBar();
    }
    void BreakPhase()
    {
        for (int i = 0; i < enemy.phases[phase].health + 1; i++)
        {
            GameObject o = healthParent.GetChild(i).gameObject;
            o.transform.parent = null;
            Destroy(o);
        }

        for (int i = 0; i < enemy.phases[phase + 1].health; i++)
        {
            if (i == 0)
                healthParent.GetChild(i).GetComponent<SpriteRenderer>().sprite = healthSprites[0]; //first segment
            else if (phase == enemy.phases.Count - 1 && enemy.phases[phase].health == i)
                healthParent.GetChild(i).GetComponent<SpriteRenderer>().sprite = healthSprites[2]; //end segment
            else
                healthParent.GetChild(i).GetComponent<SpriteRenderer>().sprite = healthSprites[1]; //mid segment
        }
        AlignHealthBar();
    }
    void SetHealthBar()
    {
        if (damageTaken > enemy.phases[phase].health) damageTaken = enemy.phases[phase].health;
        for (int i = 0; i < damageTaken; i++)
        {
            if (i == 0)
                healthParent.GetChild(i).GetComponent<SpriteRenderer>().sprite = healthSprites[6]; //damaged first segment
            else if (enemy.phases[phase].health == i) 
                healthParent.GetChild(i).GetComponent<SpriteRenderer>().sprite = healthSprites[8]; //damaged end segment
            else 
                healthParent.GetChild(i).GetComponent<SpriteRenderer>().sprite = healthSprites[7]; //damaged mid segment
        }
        AlignHealthBar();
    }

    void AlignHealthBar()
    {
        int count = healthParent.childCount;
        for (int i = 0; i < count; i++)
        {
            float x = (healthBarSpread * i) - (((count - 1) * healthBarSpread) * 0.5f);
            healthParent.GetChild(i).localPosition = new Vector3(x, 0, 0);
        }
    }

    public int GetTotalHealth()
    {
        int health = 0;
        for (int i = 0 + phase; i < enemy.phases.Count; i++)
        {
            health += enemy.phases[i].health;
        }
        return health;
    }

    public int GetCurrentHealth()
    {
        int health = GetTotalHealth();
        int currentHealth = health - damageTaken;
        return currentHealth;
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

    public void ForceMove(Vector2Int direction, int amount)
    {
        if (movementArrow != null)
            Destroy(movementArrow);
        for (int i = 0; i < amount; i++)
        {
            EnemyUnit potentialCrash = enemyManager.CheckIfCellIsOccupied(position + direction);
            if (potentialCrash != null)
            {
                StartCoroutine(Crash(direction, potentialCrash));
                return;
            }
            if (enemyManager.CheckIfCellIsOutsideOfBoard(position + direction)) return;

            position += direction;
        }
        StartCoroutine(Move(position, true));

        //Vector2 targetPosition = Manager.Instance.boardManager.spaces[position].transform.position;
        //transform.localPosition = new Vector3(targetPosition.x, targetPosition.y + enemyManager.yOffset, 0);
    }

    public IEnumerator Move(Vector2Int moveTo, bool forced = false)
    {
        readyToShowIntentions = false;
        Manager.Instance.boardManager.ClearSpaces();
        float seconds = Manager.Instance.enemyManager.collideAnimTime;
        float i = 0;
        Vector2 originalPos = transform.position;
        Vector2 targetPos = Manager.Instance.boardManager.spaces[moveTo].transform.position;
        while (i < seconds)
        {
            i += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(originalPos, new Vector3(targetPos.x, targetPos.y + enemyManager.yOffset, 0), i / seconds);
            yield return null;
        }
        readyToShowIntentions = true;
        if (forced) { 
            Manager.Instance.boardManager.ClearSpaces();
        }
        //Manager.Instance.boardManager.ClearSpaces();
        yield return null;
    }
    public IEnumerator Crash(Vector2Int moveTo, EnemyUnit crashInto)
    {
        float seconds = Manager.Instance.enemyManager.collideAnimTime;
        float i = 0;
        Vector2 originalPos = transform.position;
        Manager.Instance.boardManager.spaces.TryGetValue(position + moveTo, out BoardSpace targetSpace);
        Vector2 targetPos =  targetSpace.transform.position;
        while (i < seconds)
        {
            i += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(originalPos, new Vector3(targetPos.x, targetPos.y + enemyManager.yOffset, 0), i / seconds * 0.75f);
            yield return null;
        }
        TakeDamage(Manager.Instance.gameManager.collisionDamage);
        crashInto.TakeDamage(Manager.Instance.gameManager.collisionDamage);

        if (!dead)
            transform.position = originalPos;

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
                    position + attack.gridPosition, 
                    projectile, 
                    enemy.phases[phase].damageCard);
            }
            DamageTile(position + attack.gridPosition, attack.damage);
            PushTile(position + attack.gridPosition, attack.pushDirection, attack.pushDistance);
        }

        yield return null;
    }
    void DamageTile(Vector2Int targetTile, int damage, List<StatusEffect> statusEffects = null)
    {
        if (targetTile.y < 0)
        {
            Manager.Instance.deckManager.AddCardTo(WhereDoesTheCardGo.Hand, enemy.phases[phase].damageCard);
        }
        EnemyUnit unit = Manager.Instance.boardManager.CheckIfEnemyIsOnSpace(targetTile);
        if (unit == null) return;
        //unit.AddStatus(statusEffects);
        unit.TakeDamage(damage);
    }

    void PushTile(Vector2Int targetTile, Direction direction, int amount)
    {
        EnemyUnit unit = Manager.Instance.boardManager.CheckIfEnemyIsOnSpace(targetTile);
        if (unit == null) return;
        Vector2Int dirVector = Manager.Instance.boardManager.GetDirection(direction);
        unit.ForceMove(dirVector, amount);
    }
}
