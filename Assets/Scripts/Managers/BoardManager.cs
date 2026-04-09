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
        Manager.Instance.deckManager.DiscardOrUseCard(Manager.Instance.boardManager.heldCard, true);
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
        StartCoroutine(DoCard(target));
    }
    void FinishCardAction()
    {
        Manager.Instance.deckManager.DiscardOrUseCard(heldCard);
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

        foreach (TileEffect effect in heldCard.tileEffects)
        {
            Vector2Int space = targetedPosition + effect.gridPosition;

            spaces.TryGetValue(space, out BoardSpace cardTargetedSpace);

            if (cardTargetedSpace == null) continue;

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
                }
                else
                {
                    targetedSpace.Colorize(source, data.projDamage + response.integer);
                }

                if (data.pierce == 0) break;
                else data.pierce--;
            }

        }
    }

    IEnumerator DoCard(BoardSpace targetSpace = null)
    {
        if (targetSpace == null)
        {
            //Do effect of card
            Manager.Instance.enemyManager.KillOffEnemies();
            Manager.Instance.busy = false;
            FinishCardAction();
            yield break;
        }
        Manager.Instance.busy = true;
        foreach (TileEffect effect in heldCard.tileEffects)
        {
            Vector2Int targetPos = effect.gridPosition;

            for (int r = 0; r < effect.repeatCount + 1; r++)
            {
                Vector2Int space = targetSpace.position + targetPos;

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

                Manager.Instance.enemyManager.KillOffEnemies();
                yield return new WaitForSeconds(effect.repeatInterval);
            }
            yield return new WaitForSeconds(waitBetweenCardActions);
        }
        yield return new WaitForSeconds(cardAnimExtraTime);
        Manager.Instance.busy = false;
        FinishCardAction();
        yield return null;
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
        }
        Manager.Instance.enemyManager.ShowIntentionsOfEnemies();
    }
}