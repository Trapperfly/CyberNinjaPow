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

        foreach (Targeting target in heldCard.targeting)
        {
            Vector2Int targetPos = target.target;

            int repeats = (target.repeating || target.projectile) ? 10 : 1;

            for (int i = 0; i < repeats; i++)
            {
                Vector2Int space = targetedPosition + targetPos;

                if (target.projectile && CheckIfEnemyIsOnSpace(space)) repeats = 0;

                spaces.TryGetValue(space, out BoardSpace cardTargetedSpace);

                if (cardTargetedSpace != null)
                {
                    cardTargetedSpace.Colorize(true);
                }

                if (repeats > 1)
                    targetPos += target.target;
            }
        }
    }

    IEnumerator DoCardTargeting(BoardSpace targetSpace)
    {
        if (targetSpace == null) yield break;
        if (waitBetweenCardActions > 0) Manager.Instance.busy = true;
        for (int i = 0; i <= heldCard.extraAmount; i++)
        {
            foreach (Targeting target in heldCard.targeting)
            {
                Vector2Int targetPos = target.target;

                int repeats = (target.repeating || target.projectile) ? 10 : 1;

                for (int r = 0; r < repeats; r++)
                {
                    Vector2Int space = targetSpace.position + targetPos;

                    if (target.projectile && CheckIfEnemyIsOnSpace(space)) repeats = 0;

                    EnemyUnit enemy = CheckIfEnemyIsOnSpace(space);
                    if (enemy)
                        enemy.TakeDamage(heldCard.generalDamage + target.damage);

                    if (repeats > 1)
                        targetPos += target.target;

                    Manager.Instance.enemyManager.CardFinished();
                }
            }
            yield return new WaitForSeconds(waitBetweenCardActions);
        }
        Manager.Instance.busy = false;
        FinishCardAction();
        yield return null;
    }

    void ClearSpaces()
    {
        foreach (var keyValue in spaces)
        {
            keyValue.Value.Colorize(false);
        }
    }
}