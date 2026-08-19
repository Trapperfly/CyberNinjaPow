using JetBrains.Annotations;
using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;
using static UnityEngine.Audio.ProcessorInstance;
using static UnityEngine.GraphicsBuffer;

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
    public float cardExtraEffectAnimTime;
    public bool inCardAction = false;

    public List<CardTargeting> additionalCardQueue = new();

    public GameObject cardTargetingLinePrefab;
    public GameObject cardTargetingLine;

    public List<Sprite> targetingSprites = new();
    public List<Sprite> projectileDisplays = new();

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
        DoCard(heldCard, target);
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

        Manager.Instance.deckManager.AlignCards();
        Manager.Instance.deckManager.AlignCardsAsSiblings();

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

        if (!targetSpace) return;

        if (targetedPosition == targetSpace.position) return;

        ClearSpaces();

        EnemyUnit enemyOnSpace = CheckIfEnemyIsOnSpace(targetSpace.position);
        //Debug.Log("There is " + (enemyOnSpace ? "an enemy" : "no enemy") + " on this space");
        if (enemyOnSpace)
        {
            //Debug.Log("Showing intentions of " + enemyOnSpace.enemy.enemyName + " on space " + enemyOnSpace.position);
            enemyOnSpace.ShowIntentions(true);
        }

        targetedPosition = targetSpace.position;

        if (targetedPosition == new Vector2Int(-99, -99)) return;

        if (!heldCard) return;

        //Debug.Log("Checking card Targeting");

        //TargetAdditionalCardEffects(heldCard);

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
                    PlayerProjectile(false, space, projectile, heldCard);
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
                    PlayerProjectile(true, space, projectile, heldCard);
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
                    Vector2Int pushDir = GetVector2IntFromDirection(effect.pushDirection);
                    enemy.ForceMove(pushDir, effect.pushDistance);
                }
            }
        }
    }

    public Vector2Int GetVector2IntFromDirection(Direction direction)
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

    public Direction GetDirectionFromVector2Int(Vector2Int vector)
    {
        if (vector.x == 0 && vector.y > 0) return Direction.North;       //North
        if (vector.x > 0 && vector.y > 0) return Direction.NorthEast;    //NorthEast
        if (vector.x > 0 && vector.y == 0) return Direction.East;        //East
        if (vector.x > 0 && vector.y < 0) return Direction.SouthEast;    //SouthEast
        if (vector.x == 0 && vector.y < 0) return Direction.South;       //South
        if (vector.x < 0 && vector.y < 0) return Direction.SouthWest;    //SouthWest
        if (vector.x < 0 && vector.y == 0) return Direction.West;        //West
        if (vector.x < 0 && vector.y > 0) return Direction.NorthWest;    //NorthWest
        else return Direction.South;
    }

    public void PlayerProjectile (bool fire, Vector2Int origin, ProjectileData projectile, Card card = null)
    {
        Vector2Int space = origin;
        ProjectileData data = new();

        data.projDamage = projectile.projDamage;
        data.pierce = projectile.pierce;
        data.direction = projectile.direction;

        Vector2Int dirVector = GetVector2IntFromDirection(data.direction);

        for (int i = 0; i < 10; i++)
        {
            if (Manager.Instance.enemyManager.CheckIfCellIsOutsideOfBoard(space))
            {
                break; // always break when out of bounds
            }

            spaces.TryGetValue(space, out BoardSpace targetedSpace);
            if (targetedSpace == null)
            {
                continue;
            }

            EnemyUnit enemy = CheckIfEnemyIsOnSpace(space);
            if (enemy)
            {

                ItemResponse response = new();
                response = Manager.Instance.itemManager.TriggerOnHit(enemy);

                if (fire)
                {
                    int bonusDamage = OnHit(data.projDamage + response.integer, card, enemy);
                    //Debug.Log("Dealing " + data.projDamage + " plus item " + response.integer + " plus card " + bonusDamage + " damage");
                    enemy.TakeDamage(data.projDamage);
                }
                else
                {
                    targetedSpace.Colorize(GridSpaceSelection.CardTargeting, data.projDamage + response.integer);
                }
                if (data.pierce == 0) break;
                else data.pierce--;
            }
            else
            {
                targetedSpace.RangedColorize(dirVector, GridSpaceSelection.CardTargeting);
            }
            space += dirVector;
        }
    }

    public void EnemyProjectile(bool fire, Vector2Int origin, ProjectileData projectile, Card damageCard = null)
    {
        Vector2Int space = origin;
        ProjectileData data = new();

        data.projDamage = projectile.projDamage;
        data.pierce = projectile.pierce;
        data.direction = projectile.direction;

        Vector2Int dirVector = GetVector2IntFromDirection(data.direction);

        for (int i = 0; i < 10; i++)
        {
            if (space.y < 0)
            {
                if (!fire)
                    dangerSymbolsParent.GetChild(space.x).gameObject.SetActive(true);

                if (fire)
                {
                    Manager.Instance.playerManager.TakeDamage(damageCard);
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
                if (fire)
                    enemy.TakeDamage(data.projDamage);
                else
                    targetedSpace.Colorize(GridSpaceSelection.EnemyAttack, data.projDamage);
                if (data.pierce == 0) break;
                else data.pierce--;
            }
            else
            {
                targetedSpace.RangedColorize(dirVector, GridSpaceSelection.EnemyAttack);
            }
            space += dirVector;
        }
    }

    public IEnumerator DoAdditionalCardEffects(Card card)
    {
        foreach (AdditionalCardEffect cardEffect in card.additionalCardEffects)
        {
            for (int i = 0; i < cardEffect.doXTimes; i++)
            {
                switch (cardEffect.otherEffect)
                {
                    case OtherCardEffects.None:
                        break;
                    case OtherCardEffects.Block://REWORK THIS INTO BASE CARD EFFECT
                        Manager.Instance.playerManager.block += 2;
                        break;
                    case OtherCardEffects.Parry://REWORK THIS INTO BASE CARD EFFECT
                        break;
                    case OtherCardEffects.DrawCards:
                        StartCoroutine(Manager.Instance.deckManager.IDrawCard(cardEffect.amount));
                        break;
                    case OtherCardEffects.DiscardCards:
                        //Bring up some UI telling the player to discard cards and the ability to cancel, not discard any cards if applicable, and display information about why to discard. 
                        break;
                    case OtherCardEffects.AddClassResource:
                        Manager.Instance.playerManager.ChangeResource(cardEffect.amount);
                        break;
                    case OtherCardEffects.AddCardToHand:
                        Manager.Instance.deckManager.AddCardTo(WhereDoesTheCardGo.Hand, card);
                        break;
                    case OtherCardEffects.AddCardToDiscard:
                        Manager.Instance.deckManager.AddCardTo(WhereDoesTheCardGo.Discard, card);
                        break;
                    case OtherCardEffects.AddCardToDraw:
                        Manager.Instance.deckManager.AddCardTo(WhereDoesTheCardGo.Draw, card);
                        break;
                    case OtherCardEffects.ActivateBurn:
                        StartCoroutine(Manager.Instance.enemyManager.TriggerStatusesOnAllEnemies(StatusEffect.Burning));
                        break;
                    default:
                        break;
                }
                yield return null;
            }
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

    //public void TargetAdditionalCardEffects(Card card)
    //{
    //    foreach (AdditionalCardEffect cardEffect in card.additionalCardEffects)
    //    {
    //        cardEffect.Activate(false);
    //    }
    //}

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

    public void DoCard(Card card, BoardSpace targetSpace = null)
    {
        StartCoroutine(IDoCard(card, true, targetSpace));
    }

    IEnumerator IDoCard(Card card, bool firstCard, BoardSpace targetSpace = null)
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
            yield return StartCoroutine(IDoCard(card.playAdditionalCardAfterThisOne, false, targetSpace));
        }
        if (firstCard)
        {
            for (int i = 0; i < additionalCardQueue.Count; i++)
            {
                CardTargeting ct = additionalCardQueue[i];
                yield return StartCoroutine(IDoCard(ct.card, false, ct.boardSpace));
            }
            additionalCardQueue.Clear();

            Manager.Instance.busy = false;
            int cost = (heldCard.cost == -1) ? r : heldCard.cost; 
            FinishCardAction(cost);
        }
        yield return null;
    }

    public IEnumerator IDoCardEffect(Card card)
    {
        yield return StartCoroutine(DoAdditionalCardEffects(card));
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

                yield return Attack(card, effect, space);
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
                yield return Attack(card, effect, space);
            }
            yield return new WaitForSeconds(waitBetweenCardActions);
        }
        yield return new WaitForSeconds(cardAnimExtraTime);
    }

    public IEnumerator Attack(Card card, TileEffect effect, Vector2Int space)
    {
        if (effect.projectiles.Count > 0)
        {
            foreach (ProjectileData projectile in effect.projectiles)
                PlayerProjectile(true, space, projectile, card);
        }
        else
        {
            EnemyUnit enemy = CheckIfEnemyIsOnSpace(space);
            if (enemy)
            {
                ItemResponse response = Manager.Instance.itemManager.TriggerOnHit(enemy);
                int bonusDamage = OnHit(effect.damage + response.integer, card, enemy);
                //Debug.Log("Dealing base " + effect.damage + " plus item " + response.integer + " plus card " + bonusDamage + " damage");
                enemy.TakeDamage(effect.damage + response.integer);
            }
            if (enemy && effect.pushDirection != Direction.None)
            {
                Vector2Int pushDir = GetVector2IntFromDirection(effect.pushDirection);
                enemy.ForceMove(pushDir, effect.pushDistance);
                yield return new WaitForSeconds(Manager.Instance.enemyManager.collideAnimTime);
            }
        }
        if (Manager.Instance.enemyManager.KillOffEnemies() > 0) OnKill(heldCard);
        yield return new WaitForSeconds(effect.repeatInterval);
    }

    public int OnHit(int damage, Card card, EnemyUnit unit = null)
    {
        CardConditions conditions = new();
        conditions.hit = true;
        conditions.rawDamage = damage;
        conditions.enemy = unit;

        int bonusDamage = 0;

        foreach (AdditionalCardEffect effect in card.additionalCardEffects)
        {
            bonusDamage += effect.Conditional(conditions, effect);
        }
        return bonusDamage;
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
                    break;
                case Range.Melee:
                    if (keyValue.Key.y <= 1) keyValue.Value.Colorize(GridSpaceSelection.CardAvailableTargeting);
                    break;
                case Range.Ranged:
                    break;
                case Range.Rear:
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