using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardManager : MonoBehaviour
{
    public Vector2Int boardSize;
    public GameObject boardSpacePrefab;
    public Dictionary<Vector2Int,BoardSpace> spaces = new();
    public Card heldCard = null;
    public ContactFilter2D contactFilter;
    public Vector2Int targetedPosition = new(0, 0);
    public Transform board;
    public float waitBetweenCardActions;
    public float cardAnimExtraTime;
    public bool inCardAction = false;

    public GameObject cardTargetingLinePrefab;
    public GameObject cardTargetingLine;

    public GameObject discard;
    public bool hoveringDiscard;

    public float xSpace;
    public float ySpace;

    public bool draggingCard;
    public bool clickingCard;

    public Transform dangerSymbolsParent;

    public Transform boardInformation;
    public GameObject damageNumberForTilePrefab;
    public Vector2 damageNumberOffset;

    private void Start()
    {
        BuildBoard();
    }

    private void BuildBoard()
    {
        Vector2Int key = new(0, 0);
        while (key.y < boardSize.y)
        {
            Transform space = Instantiate(boardSpacePrefab, Vector3.zero, Quaternion.identity, board).transform;
            space.localPosition = new((key.x * xSpace) - ((boardSize.x - 1) * 0.5f), (key.y * ySpace) - ((boardSize.y - 1) * 0.5f), 0);

            BoardSpace boardSpace = space.GetComponent<BoardSpace>();
            boardSpace.position = key;
            spaces.TryAdd(key, boardSpace);
            key.x += 1;
            if (key.x == boardSize.x)
            {
                key.x = 0;
                key.y += 1;
            }
        }
    }

    private void Update()
    {
        if (Manager.Instance.busy) return;
        //Debug.Log("Not busy!");
        if (heldCard == null) return;
        //Debug.Log("Holding card!");
        if (clickingCard)
        {
            if (!Input.GetMouseButtonDown(0)) return;
            //Debug.Log("Letting go of clicked card!");
            //If the player was hovering the discard when clicked
            if (hoveringDiscard) { Discard(); return; }
            //If the player has clicked a card and is clicking a tile
            if (CheckMouseTargeting() != null || heldCard.tileEffects.Count == 0) DoCardAction();

            //If the player has clicked a card and is clicking outside of the board
            else ResetCards();
        }
        else if (draggingCard)
        {
            if (!Input.GetMouseButtonUp(0)) return;
            //Debug.Log("Letting go of dragged card!");
            //If the player was hovering the discard when letting go
            if (hoveringDiscard) { Discard(); return; }
            //If the player is dragging a card and letting go on a tile
            if (CheckMouseTargeting() != null || heldCard.tileEffects.Count == 0) DoCardAction();

            //If the player is dragging a card and letting go outside of the board
            else ResetCards();
        }
    }

    void Discard()
    {
        Manager.Instance.deckManager.DiscardOrUseCard(Manager.Instance.boardManager.heldCard, 0, true);
        ResetCards();
    }

    private void FixedUpdate()
    {
        CheckCardTargeting(CheckMouseTargeting());
    }

    public void PaintAttack(List<TileEffect> targets, Vector2Int origin)
    {
        foreach (TileEffect target in targets)
        {
            if (target.projectiles.Count > 0)
            {
                foreach (ProjectileData projectile in target.projectiles)
                    Projectile(false, GridSpaceSelection.EnemyAttack, origin + target.gridPosition, projectile);
            }
            else
            {
                if ((origin + target.gridPosition).y < 0)
                {
                    dangerSymbolsParent.GetChild(origin.x + target.gridPosition.x).gameObject.SetActive(true);
                    continue;
                }
                spaces.TryGetValue(origin + target.gridPosition, out BoardSpace targetSpace);
                if (targetSpace == null) continue;

                targetSpace.Colorize(GridSpaceSelection.EnemyAttack, target.damage);
            }
        }
    }

    public void BeginCardTargeting(Vector2 cardPos)
    {
        cardTargetingLine = Instantiate(cardTargetingLinePrefab, Vector3.zero, Quaternion.identity, null);
        cardTargetingLine.GetComponent<CardTargetingLine>().startPos = cardPos;
    }

    public void EndCardTargeting()
    {
        if (cardTargetingLine == null) return;
        Destroy(cardTargetingLine);
        cardTargetingLine = null;
    }

    void DoCardAction()
    {
        inCardAction = true;
        spaces.TryGetValue(targetedPosition, out BoardSpace target);
        StartCoroutine(DoCard(heldCard, target));
    }
    void FinishCardAction(int cost = -100)
    {
        if (cost == -100)
        {
            cost = heldCard.cost;
        }
        Manager.Instance.deckManager.DiscardOrUseCard(heldCard, cost);
        ResetCards();
    }

    void ResetCards()
    {
        heldCard = null;
        Manager.Instance.deckManager.cardRedied = false;
        inCardAction = false;

        ClearSpaces();
        EndCardTargeting();
        ResetCardSizes();

        clickingCard = false;
        draggingCard = false;
    }
    void ResetCardSizes()
    {
        foreach (Transform card in Manager.Instance.deckManager.handTransform)
        {
            CardObject co = card.GetComponent<CardObject>();
            if (!co.target)
                card.localScale = Vector3.one;
        }
    }

    BoardSpace CheckMouseTargeting()
    {
        if (heldCard == null) return null;

        Vector2 mousePos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

        Collider2D[] collidersUnderMouse = new Collider2D[4];
        int numCollidersUnderMouse = Physics2D.OverlapPoint(mousePos, contactFilter, collidersUnderMouse);

        if (numCollidersUnderMouse > 0)
        {
            collidersUnderMouse[0].TryGetComponent(out BoardSpace boardSpace);
            return boardSpace;
        }
        else
        {
            targetedPosition = new(-99, -99);
            ClearSpaces();
            return null;
        }
    }

    public EnemyUnit CheckIfEnemyIsOnSpace(Vector2Int space)
    {
        foreach (EnemyUnit enemy in Manager.Instance.enemyManager.enemies)
        {
            if (enemy.position == space)
                return enemy;
        }
        return null;
    }
    void CheckCardTargeting(BoardSpace targetSpace)
    {
        if (Manager.Instance.busy) return;

        if (targetSpace == null) return;

        if (targetedPosition == targetSpace.position) return;

        targetedPosition = targetSpace.position;

        ClearSpaces();

        TargetAdditionalCardEffects(heldCard);

        if (heldCard.targetAll.doThis)
        {
            List<Vector2Int> positions = Manager.Instance.enemyManager.GetEnemyPositions(heldCard.targetAll);
            foreach (Vector2Int space in positions)
            {
                TileEffect effect = heldCard.targetAll.effect;

                TargetAndColorizeSpaces(effect, space);
            }
        }
        foreach (TileEffect effect in heldCard.tileEffects)
        {
            int repeat = (effect.repeatCount < 1) ? 1 : effect.repeatCount;
            for (int i = 0; i < repeat; i++)
            {
                TargetAndColorizeSpaces(effect, targetedPosition + effect.gridPosition);
            }
        }
        if (heldCard.playAdditionalCardAfterThisOne == null) return;
        foreach (TileEffect effect in heldCard.playAdditionalCardAfterThisOne.tileEffects)
        {
            TargetAndColorizeSpaces(effect, targetedPosition + effect.gridPosition);
        }
    }

    void TargetAndColorizeSpaces(TileEffect effect, Vector2Int space)
    {
        spaces.TryGetValue(space, out BoardSpace cardTargetedSpace);

        if (cardTargetedSpace == null) return;

        for (int i = 0; i < effect.repeatCount + 1; i++)
        {
            if (effect.projectiles.Count > 0)
            {
                foreach (ProjectileData projectile in effect.projectiles)
                    Projectile(false, GridSpaceSelection.CardTargeting, space, projectile);
            }
            else
            {
                EnemyUnit enemy = CheckIfEnemyIsOnSpace(space);
                ItemResponse response = new();
                if (enemy != null)
                    response = Manager.Instance.itemManager.TriggerOnHit(enemy);
                cardTargetedSpace.Colorize(GridSpaceSelection.CardTargeting, effect.damage + response.integer);
            }
        }
    }

    void TargetAndAttackSpaces(TileEffect effect, Vector2Int space)
    {
        int repetitions = (effect.repeating) ? effect.repeatCount : 1;

        for (int r = 0; r < repetitions; r++)
        {
            if (effect.projectiles.Count > 0)
            {
                foreach (ProjectileData projectile in effect.projectiles)
                    Projectile(true, GridSpaceSelection.CardTargeting, space, projectile);
            }
            else
            {
                EnemyUnit enemy = CheckIfEnemyIsOnSpace(space);
                if (enemy)
                {
                    ItemResponse response = Manager.Instance.itemManager.TriggerOnHit(enemy);
                    Debug.Log("Dealing base " + effect.damage + " plus item " + response.integer + " damage");
                    enemy.TakeDamage(effect.damage + response.integer);
                }
                if (enemy && effect.pushDirection != Direction.None)
                {
                    Vector2Int pushDir = GetDirection(effect.pushDirection);
                    enemy.ForceMove(pushDir, effect.pushDistance);
                }
            }
        }
    }

    public Vector2Int GetDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.None:
                return new(0, 1);
            case Direction.North:
                return new(0, 1);
            case Direction.South:
                return new(0, -1);
            case Direction.East:
                return new(1, 0);
            case Direction.West:
                return new(-1, 0);
            case Direction.NorthEast:
                return new(1, 1);
            case Direction.NorthWest:
                return new(-1, 1);
            case Direction.SouthEast:
                return new(1, -1);
            case Direction.SouthWest:
                return new(-1, -1);
            default:
                return new(0, 0);
        }
    }

    public void Projectile(bool fire, GridSpaceSelection source, Vector2Int origin, ProjectileData projectile, Card damageCard = null)
    {
        Vector2Int space = origin;
        ProjectileData data = new();

        data.projDamage = projectile.projDamage;
        data.pierce = projectile.pierce;
        data.direction = projectile.direction;

        Vector2Int dirVector = GetDirection(data.direction);

        for (int i = 0; i < 10; i++)
        {
            space += dirVector;
            if (space.y < 0)
            {
                if (!fire && source == GridSpaceSelection.EnemyAttack)
                    dangerSymbolsParent.GetChild(space.x).gameObject.SetActive(true);

                if (fire && source == GridSpaceSelection.EnemyAttack)
                {
                    Manager.Instance.deckManager.AddCardTo(WhereDoesTheCardGo.Hand, damageCard);
                }
                break; // always break when out of bounds
            }

            spaces.TryGetValue(space, out BoardSpace targetedSpace);
            if (targetedSpace == null) {
                continue;
            }

            EnemyUnit enemy = CheckIfEnemyIsOnSpace(space);
            if (enemy)
            {
                ItemResponse response = new();
                if (source == GridSpaceSelection.CardTargeting)
                    response = Manager.Instance.itemManager.TriggerOnHit(enemy);

                if (fire)
                {
                    Debug.Log("Dealing base " + data.projDamage + " plus item " + response.integer + " damage");
                    enemy.TakeDamage(data.projDamage + response.integer);
                    OnHit(heldCard);
                }
                else
                {
                    targetedSpace.Colorize(source, data.projDamage + response.integer);
                }

                if (data.pierce == 0) break;
                else data.pierce--;
            }
            else
            {
                if (source == GridSpaceSelection.EnemyAttack)
                {
                    if (dirVector.x == 0 && dirVector.y != 0) targetedSpace.Colorize(GridSpaceSelection.EnemyProjectileVertical);
                    if (dirVector.x != 0 && dirVector.y == 0) targetedSpace.Colorize(GridSpaceSelection.EnemyProjectileHorizontal);
                    if (dirVector.x != 0 && dirVector.y != 0) targetedSpace.Colorize(GridSpaceSelection.EnemyProjectileDiagonal);
                }
                if (source == GridSpaceSelection.CardTargeting)
                {
                    if (dirVector.x == 0 && dirVector.y != 0) targetedSpace.Colorize(GridSpaceSelection.PlayerProjectileVertical);
                    if (dirVector.x != 0 && dirVector.y == 0) targetedSpace.Colorize(GridSpaceSelection.PlayerProjectileHorizontal);
                    if (dirVector.x != 0 && dirVector.y != 0) targetedSpace.Colorize(GridSpaceSelection.PlayerProjectileDiagonal);
                }
            }
        }
    }

    public void DoAdditionalCardEffects(Card card)
    {
        foreach (AdditionalCardEffect cardEffect in card.additionalCardEffects)
        {
            cardEffect.Activate(true);
        }
    }
    //public IEnumerator IDoAdditionalCardEffects(Card card)
    //{
    //    reply = new AdditionalCardEffectReply();
    //    foreach (AdditionalCardEffect cardEffect in card.additionalCardEffects)
    //    {
    //        AdditionalCardEffectReply effectReply = cardEffect.Activate(true);
    //        if (effectReply.stop)
    //        {
    //            reply.stop = true;
    //            yield break;
    //        }
    //        yield return new WaitForSeconds(cardEffect.animationTime);
    //    }
    //    yield return null;
    //}

    public void TargetAdditionalCardEffects(Card card)
    {
        foreach (AdditionalCardEffect cardEffect in card.additionalCardEffects)
        {
            cardEffect.Activate(false);
        }
    }

    public void DoConditionalCardEffects(Card card, CardConditions conditions) //Needs a better way to check for completions of conditions
    {
        foreach (AdditionalCardEffect cardEffect in card.additionalCardEffects)
        {
            cardEffect.Conditional(conditions);
        }
    }

    public void CheckCardConditions(Card card)
    {
        CardConditions conditions = new CardConditions();
        conditions.killed = (Manager.Instance.enemyManager.KillOffEnemies() > 0) ? true : false;
        //conditions.hit = reply.hitEnemy;
        //DoConditionalCardEffects(card, conditions);
    }

    IEnumerator DoCard(Card card, BoardSpace targetSpace = null)
    {
        if (card.classResourceCost > Manager.Instance.playerManager.playerResource)
        {
            ResetCards();
            yield break;
        }
        Manager.Instance.busy = true;
        int r = (card.classResourceCost == -1) ? Manager.Instance.playerManager.playerResource : card.repeats;
        for (int i = 0; i < r; i++)
        {
            //Do effect of card
            yield return StartCoroutine(IDoCardEffect(card));

            //Check conditional
            //CheckCardConditions(card);

            //Do tile-effects / attacks
            yield return StartCoroutine(IDoCardAttack(card, targetSpace));
        }
        
        yield return new WaitForSeconds(cardAnimExtraTime);
        if (card.playAdditionalCardAfterThisOne != null)
        {
            StartCoroutine(DoCard(card.playAdditionalCardAfterThisOne, targetSpace));
        }
        else
        {
            Manager.Instance.busy = false;
            int cost = (heldCard.cost == -1) ? r : heldCard.cost; 
            FinishCardAction(cost);
        }
        yield return null;
    }

    public IEnumerator IDoCardEffect(Card card)
    {
        DoAdditionalCardEffects(card);
        //yield return new WaitForSeconds(cardAnimExtraTime);
        yield return null;
    }

    public IEnumerator IDoCardAttack(Card card, BoardSpace targetSpace = null)
    {
        CardConditions conditions = new();
        if (card.targetAll.doThis)
        {
            List<Vector2Int> positions = Manager.Instance.enemyManager.GetEnemyPositions(card.targetAll);
            foreach (Vector2Int space in positions)
            {
                TileEffect effect = card.targetAll.effect;

                yield return Attack(effect, space);
            }
        }

        foreach (TileEffect effect in card.tileEffects)
        {
            Vector2Int targetPos = effect.gridPosition;

            int repetitions = (effect.repeating) ? effect.repeatCount : 1;

            Vector2Int space;
            if (targetSpace != null)
                space = targetSpace.position + targetPos;
            else space = new(0, 0);

            for (int r = 0; r < repetitions; r++)
            {
                yield return Attack(effect, space);
            }
            yield return new WaitForSeconds(waitBetweenCardActions);
        }
        yield return new WaitForSeconds(cardAnimExtraTime);
    }

    public IEnumerator Attack(TileEffect effect, Vector2Int space)
    {
        if (effect.projectiles.Count > 0)
        {
            foreach (ProjectileData projectile in effect.projectiles)
                Projectile(true, GridSpaceSelection.CardTargeting, space, projectile);
        }
        else
        {
            EnemyUnit enemy = CheckIfEnemyIsOnSpace(space);
            if (enemy)
            {
                ItemResponse response = Manager.Instance.itemManager.TriggerOnHit(enemy);
                Debug.Log("Dealing base " + effect.damage + " plus item " + response.integer + " damage");
                enemy.TakeDamage(effect.damage + response.integer);
                OnHit(heldCard);
            }
            if (enemy && effect.pushDirection != Direction.None)
            {
                Vector2Int pushDir = GetDirection(effect.pushDirection);
                enemy.ForceMove(pushDir, effect.pushDistance);
                yield return new WaitForSeconds(Manager.Instance.enemyManager.collideAnimTime);
            }
        }
        if (Manager.Instance.enemyManager.KillOffEnemies() > 0) OnKill(heldCard);
        yield return new WaitForSeconds(effect.repeatInterval);
    }

    public void OnHit(Card card, EnemyUnit unit = null)
    {
        CardConditions conditions = new();
        conditions.hit = true;
        foreach (AdditionalCardEffect effect in card.additionalCardEffects)
        {
            effect.Conditional(conditions);
        }
    }
    public void OnKill(Card card)
    {
        CardConditions conditions = new();
        conditions.killed = true;
        foreach (AdditionalCardEffect effect in card.additionalCardEffects)
        {
            effect.Conditional(conditions);
        }
    }

    public void TargetAllEnemies(bool fire, TileEffect tileEffect, StatusEffect conditionalTarget = StatusEffect.None)
    {
        foreach (EnemyUnit unit in Manager.Instance.enemyManager.enemies)
        {
            Debug.Log("Targeting space " + unit.position);
            if (conditionalTarget == StatusEffect.None)
            {
                if (fire) TargetAndAttackSpaces(tileEffect, unit.position);
                else TargetAndColorizeSpaces(tileEffect, unit.position);
            }
            else
            {
                foreach (EffectInfo effect in unit.effects)
                {
                    StatusEffect status = effect.effect.GiveStatus();
                    if (status == conditionalTarget)
                    {
                        if (fire) TargetAndAttackSpaces(tileEffect, unit.position);
                        else TargetAndColorizeSpaces(tileEffect, unit.position);
                    }
                }
            }
        }
    }

    public void ClearSpaces()
    {
        foreach (Transform child in dangerSymbolsParent)
        {
            child.gameObject.SetActive(false);
        }
        foreach (var keyValue in spaces)
        {
            keyValue.Value.Colorize(GridSpaceSelection.None);

            if (!heldCard) continue;

            switch (heldCard.range)
            {
                case Range.Anywhere:
                    keyValue.Value.Colorize(GridSpaceSelection.CardAvailableTargeting);
                    break;
                case Range.Melee:
                    if (keyValue.Key.y <= 1) keyValue.Value.Colorize(GridSpaceSelection.CardAvailableTargeting);
                    break;
                case Range.Ranged:
                    if (keyValue.Key.y > 1 && keyValue.Key.y < 4) keyValue.Value.Colorize(GridSpaceSelection.CardAvailableTargeting);
                    break;
                case Range.Rear:
                    if (keyValue.Key.y > 3) keyValue.Value.Colorize(GridSpaceSelection.CardAvailableTargeting);
                    break;
                case Range.Projectile:
                    if (keyValue.Key.y == 0) keyValue.Value.Colorize(GridSpaceSelection.CardAvailableTargeting);
                    break;
                default:
                    keyValue.Value.Colorize(GridSpaceSelection.None);
                    break;
            }
        }
        Manager.Instance.enemyManager.ShowIntentionsOfEnemies();
    }
}