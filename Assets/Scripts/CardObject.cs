using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CardObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public RectTransform hand;
    public Card card = null;

    public float scale = 1.25f;
    bool target;

    public TMP_Text cardName;
    public TMP_Text cost;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Manager.Instance.deckManager.cardRedied || Manager.Instance.busy) return;
        target = true;
        transform.localScale *= scale;
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Manager.Instance.deckManager.cardRedied || Manager.Instance.busy) return;
        target = false;
        transform.localScale *= 1 / scale;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!target) { return; }

        if (card == null) { return; }

        Manager.Instance.deckManager.cardRedied = true;

        Manager.Instance.boardManager.BeginCardTargeting(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        Manager.Instance.boardManager.heldCard = card;
        Manager.Instance.deckManager.physicalCardHeld = this;
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //if (!target) { return; }

        //if (card == null) { return; }

        //Manager.Instance.boardManager.BeginCardTargeting(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        //Manager.Instance.boardManager.heldCard = card;
        //Manager.Instance.deckManager.physicalCardHeld = this;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.hovered.Contains(Manager.Instance.boardManager.discard))
        {
            if (Manager.Instance.boardManager.heldCard == null) { return; }
            Manager.Instance.boardManager.EndCardTargeting();
            Manager.Instance.deckManager.cardRedied = false;
            Manager.Instance.deckManager.DiscardOrUseCard(Manager.Instance.boardManager.heldCard, true);
        }
    }
}
