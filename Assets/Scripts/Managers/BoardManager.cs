using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    bool busy = false;

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
            space.localPosition = new(key.x - ((boardSize.x - 1) * 0.5f), key.y - ((boardSize.y - 1) * 0.5f), 0);

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
        if (busy) return;
        if (Input.GetMouseButtonUp(0) && heldCard != null && CheckMouseTargeting() != null) DoCardAction();
        else if (Input.GetMouseButtonUp(0) && heldCard != null) ResetHeldCard();
    }

    private void FixedUpdate()
    {
        CheckCardTargeting(CheckMouseTargeting());
    }

    void DoCardAction()
    {
        spaces.TryGetValue(targetedPosition, out BoardSpace target);
        StartCoroutine(DoCardTargeting(target));
        
    }
    void FinishCardAction()
    {
        ClearSpaces();
        Manager.Instance.deckManager.DiscardOrUseCard(heldCard);
        heldCard = null;
    }
    void ResetHeldCard()
    {
        ClearSpaces();
        heldCard = null;
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
        if (busy) return;

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
        if (waitBetweenCardActions > 0) busy = true;
        for (int i = 0; i < heldCard.extraAmount; i++)
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
        busy = false;
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