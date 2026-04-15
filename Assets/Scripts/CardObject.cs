using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CardObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler
{
    public RectTransform hand;
    public Card card = null;

    public float scale = 1.25f;
    public float offset = 1f;

    public bool target;
    public bool scaled = false;

    public TMP_Text cardName;
    public TMP_Text cardDescription;

    public Transform time;
    public Transform range;
    public Transform tags;

    public bool clicked = false;
    public bool display = false;
    private void FixedUpdate()
    {
        if (Manager.Instance.deckManager.cardRedied || Manager.Instance.busy) return;
        if (target && !scaled)
        {
            Scale();
        }
        else if (!target && scaled)
        {
            Unscale();
        }
    }

    void Scale()
    {
        Manager.Instance.deckManager.AlignCardsAsSiblings();
        scaled = true;
        transform.localScale = scale * Vector3.one;
        transform.localPosition += new Vector3(0,offset,0);
        transform.SetParent(Manager.Instance.deckManager.handHoldingCardTransform);
    }

    void Unscale()
    {
        transform.SetParent(Manager.Instance.deckManager.handTransform);
        scaled = false;
        transform.localScale = Vector3.one;
        transform.localPosition += new Vector3(0, -offset, 0);
        Manager.Instance.deckManager.AlignCardsAsSiblings();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        target = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        target = false;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (display) return;

        if (Manager.Instance.busy) return;

        if (!target) { return; }

        if (card == null) { return; }

        Manager.Instance.deckManager.cardRedied = true;

        Manager.Instance.boardManager.BeginCardTargeting(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        Manager.Instance.boardManager.heldCard = card;
        Manager.Instance.deckManager.physicalCardHeld = this;

        clicked = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (clicked) Manager.Instance.boardManager.clickingCard = true;
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (display) return;

        clicked = false;

        Manager.Instance.boardManager.draggingCard = true;

        //if (!target) { return; }

        //if (card == null) { return; }

        //Manager.Instance.boardManager.BeginCardTargeting(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        //Manager.Instance.boardManager.heldCard = card;
        //Manager.Instance.deckManager.physicalCardHeld = this;
    }

    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    if (eventData.hovered.Contains(Manager.Instance.boardManager.discard))
    //    {
    //        if (Manager.Instance.boardManager.heldCard == null) { return; }
    //        Manager.Instance.boardManager.EndCardTargeting();
    //        Manager.Instance.deckManager.cardRedied = false;
    //        Manager.Instance.deckManager.DiscardOrUseCard(Manager.Instance.boardManager.heldCard, true);
    //    }
    //}
}
