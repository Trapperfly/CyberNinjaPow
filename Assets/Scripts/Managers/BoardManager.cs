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
    public bool inCardAction = false;

    public GameObject cardTargetingLinePrefab;
    public GameObject cardTargetingLine;

    public GameObject discard;

    public float xSpace;
    public float ySpace;

    public bool draggingCard;
    public bool clickingCard;

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
        Debug.Log("Not busy!");
        if (heldCard == null) return;
        Debug.Log("Holding card!");
        if (clickingCard)
        {
            if (!Input.GetMouseButtonDown(0)) return;
            Debug.Log("Letting go of clicked card!");
            //If the player has clicked a card and is clicking a tile
            if (CheckMouseTargeting() != null) DoCardAction();

            //If the player has clicked a card and is clicking outside of the board
            else ResetCards();
        }
        else if (draggingCard)
        {
            if (!Input.GetMouseButtonUp(0)) return;
            Debug.Log("Letting go of dragged card!");
            //If the player is dragging a card and letting go on a tile
            if (CheckMouseTargeting() != null) DoCardAction();

            //If the player is dragging a card and letting go outside of the board
            else ResetCards();
        }
    }

    private void FixedUpdate()
    {
        CheckCardTargeting(CheckMouseTargeting());
    }

    public void PaintAttack(List<TileEffect> targets, Vector2Int origin)
    {
        foreach (TileEffect target in targets)
        {
            spaces.TryGetValue(origin + target.gridPosition, out BoardSpace targetSpace);
            if (targetSpace == null) continue;

            targetSpace.Colorize(GridSpaceSelection.EnemyAttack);
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
        StartCoroutine(DoCardTargeting(target));
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
                        Projectile(false, space, projectile.direction, effect.damage, projectile.pierce);
                }
                else
                    cardTargetedSpace.Colorize(GridSpaceSelection.CardTargeting);
            }
        }
    }

    Vector2Int GetProjectileDirection(Direction direction)
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

    void Projectile(bool fire, Vector2Int origin, Direction direction, int damage = 0, int pierce = 0)
    {
        Vector2Int space = origin;
        Vector2Int dirVector = GetProjectileDirection(direction);
        
        int checks = 10;

        if (fire) //If attacking
        {
            for (int i = 0; i < checks; i++)
            {
                spaces.TryGetValue(space, out BoardSpace cardTargetedSpace);

                if (cardTargetedSpace == null) continue;

                EnemyUnit enemy = CheckIfEnemyIsOnSpace(space);
                if (enemy)
                {
                    enemy.TakeDamage(damage);
                    if (pierce == 0)
                        checks = 0;
                    else pierce--;
                }

                space += dirVector;
            }
        }
        else //If only targeting
        {
            for (int i = 0; i < checks; i++)
            {
                spaces.TryGetValue(space, out BoardSpace cardTargetedSpace);

                if (cardTargetedSpace == null) continue;

                cardTargetedSpace.Colorize(GridSpaceSelection.CardTargeting);

                if (CheckIfEnemyIsOnSpace(space))
                {
                    if (pierce == 0)
                        checks = 0;
                    else pierce--;
                }

                space += dirVector;
            }
        }
    }

    IEnumerator DoCardTargeting(BoardSpace targetSpace)
    {
        if (targetSpace == null) yield break;
        if (waitBetweenCardActions > 0) Manager.Instance.busy = true;
        foreach (TileEffect effect in heldCard.tileEffects)
        {
            Vector2Int targetPos = effect.gridPosition;

            for (int r = 0; r < effect.repeatCount + 1; r++)
            {
                Vector2Int space = targetSpace.position + targetPos;

                if (effect.projectiles.Count > 0)
                {
                    foreach (ProjectileData projectile in effect.projectiles)
                        Projectile(true, space, projectile.direction, effect.damage, projectile.pierce);
                }
                else
                {
                    EnemyUnit enemy = CheckIfEnemyIsOnSpace(space);
                    if (enemy)
                        enemy.TakeDamage(effect.damage);
                }

                Manager.Instance.enemyManager.KillOffEnemies();
                yield return new WaitForSeconds(effect.repeatInterval);
            }
        }
        yield return new WaitForSeconds(waitBetweenCardActions);
        Manager.Instance.busy = false;
        FinishCardAction();
        yield return null;
    }

    public void ClearSpaces()
    {
        foreach (var keyValue in spaces)
        {
            keyValue.Value.Colorize(GridSpaceSelection.None);
        }
        foreach (EnemyUnit unit in Manager.Instance.enemyManager.enemies)
        {
            unit.PaintAttack();
        }
    }
}