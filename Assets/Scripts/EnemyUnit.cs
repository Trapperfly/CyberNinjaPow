using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyUnit : MonoBehaviour
{
    public Vector2Int position;
    //public Enemy enemy;
    public Enemy enemy;
    public bool attacking;
    public bool inRange;
    public List<TileEffect> intendedAttack;
    public List<TileEffect> attack;
    public int iAttackCounter = -1;
    public int attackRange;
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
    public float healthBarSpread = 1f; //0.03125
    public int segmentWidth = 1;
    public int gateWidth = 1;
    public int afterGateWidth = 1;

    public GameObject movementArrow;
    public List<GameObject> attackArrows = new();

    EnemyManager enemyManager;

    float bobbingOffset;

    private void Start()
    {
        enemyManager = Manager.Instance.enemyManager;
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = enemy.sprite[phase];

        enemyManager.enemies.Add(this);

        Vector2 targetPosition = GetWorldPos(position);
        transform.localPosition = new Vector3(targetPosition.x, targetPosition.y + enemyManager.yOffset, 0);

        CreateHealthBar();

        bobbingOffset = Random.Range(0, 9000f);

        attackRange = (enemy.range == Range.Melee) ? 2 : 20;

        SetTimer();

        CreateAttack();

        Manager.Instance.boardManager.ClearSpaces();
    }

    public void Act()
    {
        if (dead) return;
        StartCoroutine(IAct());
    }

    void CreateAttack()
    {
        TileEffect effect = new TileEffect();
        
        switch (enemy.range)
        {
            case Range.Anywhere:
                break;
            case Range.Melee:
                effect.gridPosition = Vector2Int.down;
                effect.damage = (int)enemy.damage + 1;
                break;
            case Range.Ranged:
                ProjectileData projectile = new ProjectileData();
                projectile.direction = Direction.South;
                projectile.projDamage = effect.damage = (int)enemy.damage + 1;
                effect.gridPosition = Vector2Int.down;
                effect.projectiles.Add(projectile);
                break;
            case Range.Rear:
                break;
            case Range.Projectile:
                break;
            default:
                break;
        }

        for (int i = 0; i < enemy.attacks; i++)
        {
            //Debug.Log("Adding attack");
            attack.Add(effect);
        }
    }

    public IEnumerator IAct()
    {
        StartCoroutine(Attack());

        yield return new WaitForSeconds(enemyManager.attackAnimTime);

        timer = 0;
        SetTimer();

        yield return null;
    }

    private void Update()
    {
        //Debug.Log(Mathf.Sin(Time.time));
        spriteRenderer.transform.localPosition = Mathf.Sin(Time.time * enemyManager.bobbingSpeed + bobbingOffset) * enemyManager.bobbing * new Vector3(0, 1, 0);
    }

    public void PaintAttack()
    {
        //Debug.Log("Trying to paint attack");
        //Manager.Instance.boardManager.PaintAttack(intendedAttack, position);
        for (int i = 0; i < attackArrows.Count; i++)
        {
            Destroy(attackArrows[i]);
        }
        attackArrows.Clear();
        foreach (TileEffect target in attack)
        {
            if (target.projectiles.Count > 0)
            {
                foreach (ProjectileData projectile in target.projectiles)
                    Manager.Instance.boardManager.EnemyProjectile(false, position + target.gridPosition, projectile);
            }
            else
            {
                if ((position + target.gridPosition).y < 0)
                {
                    //dangerSymbolsParent.GetChild(position.x + target.gridPosition.x).gameObject.SetActive(true);
                    continue;
                }
                Manager.Instance.boardManager.spaces.TryGetValue(position + target.gridPosition, out BoardSpace targetSpace);
                if (targetSpace == null) continue;

                targetSpace.Colorize(GridSpaceSelection.EnemyAttack, target.damage);
            }
            attackArrows.Add(PaintAttackArrow(target));
        }
    }

    public GameObject PaintAttackArrow(TileEffect target)
    {
        Transform arrow = Instantiate(Manager.Instance.enemyManager.attackArrowPrefab, Vector3.zero, Quaternion.identity, null).transform;
        arrow.position =
            Vector3.Lerp(
                Manager.Instance.enemyManager.GetWorldPos(position),
                Manager.Instance.enemyManager.GetWorldPos(position) + target.gridPosition, 0.7f);
        SpriteRenderer renderer = arrow.GetComponent<SpriteRenderer>();
        Sprite sprite = null;
        if (target.gridPosition.x == 0 && target.gridPosition.y > 0) //North
            Manager.Instance.enemyManager.attackSprites.TryGetValue(new(0, 0), out sprite);
        if (target.gridPosition.x > 0 && target.gridPosition.y > 0) //NorthEast
            Manager.Instance.enemyManager.attackSprites.TryGetValue(new(1, 0), out sprite);
        if (target.gridPosition.x > 0 && target.gridPosition.y == 0) //East
            Manager.Instance.enemyManager.attackSprites.TryGetValue(new(2, 0), out sprite);
        if (target.gridPosition.x > 0 && target.gridPosition.y < 0) //SouthEast
            Manager.Instance.enemyManager.attackSprites.TryGetValue(new(3, 0), out sprite);
        if (target.gridPosition.x == 0 && target.gridPosition.y < 0) //South
            Manager.Instance.enemyManager.attackSprites.TryGetValue(new(4, 0), out sprite);
        if (target.gridPosition.x < 0 && target.gridPosition.y < 0) //SouthWest
            Manager.Instance.enemyManager.attackSprites.TryGetValue(new(5, 0), out sprite);
        if (target.gridPosition.x < 0 && target.gridPosition.y == 0) //West
            Manager.Instance.enemyManager.attackSprites.TryGetValue(new(6, 0), out sprite);
        if (target.gridPosition.x < 0 && target.gridPosition.y > 0) //NorthWest
            Manager.Instance.enemyManager.attackSprites.TryGetValue(new(7, 0), out sprite);
        renderer.sprite = sprite;
        return renderer.gameObject;
    }

    public void ShowIntentions()
    {
        if (dead) return;
        inRange = (position.y < attackRange);
        //Debug.Log("I am " + enemy.enemyName + " at " + position.x + "," + position.y + " and I am " + (inRange ? "in range" : "not in range"));
        if (inRange) PaintAttack();
        SortSprites();
    }

    public void SortSprites()
    {
        int i = 0;
        foreach (Transform child in transform)
        {
            if (child.childCount > 0)
            {
                foreach (Transform grandChild in child)
                {
                    grandChild.GetComponent<SpriteRenderer>().sortingOrder = 1000 - position.y * 100 + i++;
                }
            }
            else
            {
                child.GetComponent<SpriteRenderer>().sortingOrder = 1000 - position.y * 100 + i++;
            }
        }
    }

    public Vector2 GetWorldPos(Vector2Int gridPosition)
    {
        return Manager.Instance.boardManager.spaces[gridPosition].transform.position;
    }

    public void TakeDamage(int damage)
    {
        if (dead || damage == 0) { return; }

        Debug.Log(enemy.enemyName + " took " + damage + " damage");
        damageTaken += damage;
        
        if (damageTaken >= enemy.health[phase])
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
        
        if (enemy.health.Count - 1 <= phase)
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
        spriteRenderer.sprite = enemy.sprite[phase];

        ShowIntentions();

        SetHealthBar();
    }

    public void PrepareDie()
    {
        enemyManager.deadEnemies.Add(this);
    }

    public void Die(bool rewards = true)
    {
        if (rewards) Manager.Instance.gameManager.AlterMoney((int)enemy.threat);
        Manager.Instance.gameManager.KilledAnEnemy(enemy.threat);
        //Destroy(movementArrow);
        for (int i = 0; i < attackArrows.Count; i++) {
            Destroy(attackArrows[i]);
        }
        attackArrows.Clear();
        Destroy(gameObject);
    }

    void CreateHealthBar()
    {
        Sprite sprite;
        for (int p = 0; p < enemy.health.Count; p++)
        {
            for (int h = 0; h < enemy.health[p]; h++)
            {
                if (p == 0) //First phase
                {
                    if (p == 0 && h == 0) //Start of healthbar
                    {
                        enemyManager.healthSprites.TryGetValue(new(0,0), out sprite);
                    }
                    else if (enemy.health.Count == 1 && h == enemy.health[p] - 1) //End of health bar if 1 phase only
                    {
                        enemyManager.healthSprites.TryGetValue(new(2, 0), out sprite);
                    }
                    else enemyManager.healthSprites.TryGetValue(new(1, 0), out sprite);
                }
                else
                {
                    if (p == enemy.health.Count - 1 && h == enemy.health[p] - 1) //End of health bar
                    {
                        enemyManager.healthSprites.TryGetValue(new(4, 0), out sprite);
                    }
                    enemyManager.healthSprites.TryGetValue(new(3, 0), out sprite);
                }
                Instantiate(healthbarSegmentPrefab, healthParent).GetComponent<SpriteRenderer>().sprite = sprite;
            }
            enemyManager.healthSprites.TryGetValue(new(0, 6), out sprite);
            if (p != enemy.health.Count - 1)
                Instantiate(healthbarSegmentPrefab, healthParent).GetComponent<SpriteRenderer>().sprite = sprite; //Gate
        }

        AlignHealthBar();
    }
    void BreakPhase()
    {
        Sprite sprite;
        for (int i = 0; i < enemy.health[phase] + 1; i++)
        {
            GameObject o = healthParent.GetChild(0).gameObject;
            o.transform.parent = null;
            Destroy(o);
        }

        for (int i = 0; i < enemy.health[phase + 1]; i++)
        {
            if (i == 0)
                enemyManager.healthSprites.TryGetValue(new(0, 0), out sprite); //first segment
            else if (phase == enemy.health.Count - 1 && enemy.health[phase] == i)
                enemyManager.healthSprites.TryGetValue(new(2, 0), out sprite); //end segment
            else
                enemyManager.healthSprites.TryGetValue(new(1, 0), out sprite); //mid segment
            healthParent.GetChild(i).GetComponent<SpriteRenderer>().sprite = sprite;
        }
        AlignHealthBar();
    }
    void SetHealthBar()
    {
        Sprite sprite;
        if (damageTaken > enemy.health[phase]) damageTaken = enemy.health[phase];
        for (int i = 0; i < damageTaken; i++)
        {
            if (i == 0)
                enemyManager.healthSprites.TryGetValue(new(5, 0), out sprite); //damaged first segment
            else if (enemy.health[phase] == i)
                enemyManager.healthSprites.TryGetValue(new(7, 0), out sprite); //damaged end segment
            else
                enemyManager.healthSprites.TryGetValue(new(6, 0), out sprite); //damaged mid segment
            healthParent.GetChild(i).GetComponent<SpriteRenderer>().sprite = sprite;
        }
        AlignHealthBar();
    }

    void AlignHealthBar()
    {
        int count = healthParent.childCount;
        bool gate = false;
        for (int i = 0; i < count; i++)
        {
            if (i == 0) continue;

            float previousX = healthParent.GetChild(i-1).localPosition.x;

            Sprite currentSprite = healthParent.GetChild(i).GetComponent<SpriteRenderer>().sprite;
            enemyManager.healthSprites.TryGetValue(new(0, 6), out Sprite sprite);


            float x; //= (currentSprite == sprite) ? previousX + (healthBarSpread * gateWidth) : ; // - (((count - 1) * healthBarSpread) * 0.5f);
            if (currentSprite == sprite) x = previousX + (healthBarSpread * gateWidth);
            else if (gate) x = previousX + (healthBarSpread * afterGateWidth);
            else x = previousX + (healthBarSpread * segmentWidth);
            healthParent.GetChild(i).localPosition = new Vector3(x, 0, 0);

            gate = (currentSprite == sprite) ? true : false;
        }
        float firstChildPos = healthParent.GetChild(0).localPosition.x;
        float lastChildPos = healthParent.GetChild(healthParent.childCount - 1).localPosition.x;
        float xAlign = (lastChildPos + firstChildPos) * 0.5f;
        healthParent.localPosition = new(-xAlign, healthParent.localPosition.y, 0);

        SortSprites();
    }

    public int GetTotalHealth()
    {
        int health = 0;
        for (int i = 0 + phase; i < enemy.health.Count; i++)
        {
            health += enemy.health[i];
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
        if (!inRange) return;
        if (enemy.readySpeed > timer) {
            timer++;
            SetTimer();
            return; 
        }
        enemyManager.actingEnemies.Add(this);
    }

    public void SetTimer()
    {
        timerSpriteRenderer.size = new(enemy.readySpeed - timer, 1);
        if (enemy.readySpeed <= timer)
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

    public bool CheckSpace(Vector2Int specificSpace)
    {
        bool occupied = enemyManager.CheckIfCellIsOccupied(specificSpace);
        bool outside = enemyManager.CheckIfCellIsOutsideOfBoard(specificSpace);

        if (occupied || outside)
        {
            return false;
        }
        return true;
    }

    public bool CheckSpace(Vector2Int origin, int movement)
    {
        bool occupied = enemyManager.CheckIfCellIsOccupied(origin - new Vector2Int(0, movement));
        bool outside = enemyManager.CheckIfCellIsOutsideOfBoard(origin - new Vector2Int(0, movement));

        if (occupied || outside)
        {
            return false;
        }
        return true;
    }
    public void Move()
    {
        StartCoroutine(IMove(enemy.movement));
    }

    public IEnumerator IMove(int movement)
    {
        int spaces = CheckMove();
        //Debug.Log("Trying to move " + spaces + " spaces.");

        if (spaces != 0)
        {
            StartCoroutine(MoveUnit(spaces));

            yield return new WaitForSeconds(enemyManager.moveAnimTime * spaces);
        }
    }
    public int CheckMove()
    {
        int movement = 0;
        if (enemy.movement < 0) movement = -1; else movement = 1;
        int distance = 0;
        Vector2Int pos = position;
        for (int i = 0; i < Mathf.Abs(enemy.movement); i++) 
        {
            if (CheckSpace(pos, movement)) 
            {
                distance++;
                pos = pos - new Vector2Int(0, movement);
            }
            else return distance * movement;
        }
        return distance * movement;
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
        }
        StartCoroutine(MoveUnit(1, direction, true));
    }

    public IEnumerator MoveUnit(int movement, Vector2Int? direction = null, bool forced = false)
    {
        if (direction == null) direction = Vector2Int.down;
        float seconds = Manager.Instance.enemyManager.collideAnimTime;
        float i = 0;
        Vector2 originalPos = transform.position;
        Vector2 targetPos = Manager.Instance.boardManager.spaces[position + movement * (Vector2Int)direction].transform.position;
        while (i < seconds)
        {
            i += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(originalPos, new Vector3(targetPos.x, targetPos.y + enemyManager.yOffset, 0), i / seconds);
            yield return null;
        }
        position = Manager.Instance.boardManager.spaces[position + (new Vector2Int(0, movement) * (Vector2Int)direction)].position;
        Manager.Instance.boardManager.ClearSpaces();
        SortSprites();
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
        {
            transform.position = originalPos;
        }
        Manager.Instance.boardManager.ClearSpaces();
        yield return null;
    }

    IEnumerator Attack()
    {
        readyToShowIntentions = false;

        Debug.Log("Attacking");
        foreach (TileEffect a in attack)
        {
            Debug.Log("one attack");
            foreach (ProjectileData projectile in a.projectiles)
            {
                Debug.Log("sending projectile");
                Manager.Instance.boardManager.EnemyProjectile(
                    true, 
                    position + a.gridPosition, 
                    projectile, 
                    enemyManager.damageCards[(int)enemy.damage]);
            }
            Debug.Log("trying to damage specific tile");
            DamageTile(position + a.gridPosition, a.damage);
            PushTile(position + a.gridPosition, a.pushDirection, a.pushDistance);
        }
        Manager.Instance.boardManager.ClearSpaces();
        SortSprites();
        yield return null;
    }
    void DamageTile(Vector2Int targetTile, int damage, List<StatusEffect> statusEffects = null)
    {
        if (targetTile.y < 0)
        {
            Manager.Instance.deckManager.AddCardTo(WhereDoesTheCardGo.Hand, enemyManager.damageCards[(int)enemy.damage]);
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
        Vector2Int dirVector = Manager.Instance.boardManager.GetVector2IntFromDirection(direction);
        unit.ForceMove(dirVector, amount);
    }
}
