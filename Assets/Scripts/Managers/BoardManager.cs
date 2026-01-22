using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public Dictionary<Vector2Int,BoardSpace> spaces = new();
    public Card heldCard = null;
    public ContactFilter2D contactFilter;
    public Vector2Int targetedPosition = new(0, 0);
    public Transform board;

    private void Start()
    {
        Vector2Int key = new(-2, -2);
        for (int i = 0; i < board.transform.childCount; i++)
        {
            BoardSpace space = board.transform.GetChild(i).GetComponent<BoardSpace>();
            space.position = key;
            spaces.TryAdd(key, space);
            key.x += 1;
            if (key.x == 3)
            {
                key.x = -2;
                key.y += 1;
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && heldCard != null && CheckMouseTargeting() != null) DoCardAction();
        else if (Input.GetMouseButtonUp(0) && heldCard != null) ResetHeldCard();
    }

    private void FixedUpdate()
    {
        CheckCardTargeting(CheckMouseTargeting());
    }

    void DoCardAction()
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

    void CheckCardTargeting(BoardSpace targetSpace)
    {
        if (targetSpace == null) return;

        if (targetedPosition == targetSpace.position) return;

        targetedPosition = targetSpace.position;

        ClearSpaces();

        foreach (Targeting target in heldCard.targeting)
        {
            Vector2Int targetPos = target.target;

            int repeats = (target.repeating) ? 10 : 1;

            for (int i = 0; i < repeats; i++)
            {
                Vector2Int space = targetedPosition + targetPos;

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

    void ClearSpaces()
    {
        foreach (var keyValue in spaces)
        {
            keyValue.Value.Colorize(false);
        }
    }
}