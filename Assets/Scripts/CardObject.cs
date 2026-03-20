using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CardObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
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
        scaled = true;
        transform.localScale = scale * Vector3.one;
        transform.localPosition += new Vector3(0,offset,0);
        transform.SetAsLastSibling();
    }

    void Unscale()
    {
        scaled = false;
        transform.localScale = Vector3.one;
        transform.localPosition += new Vector3(0, -offset, 0);
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
        clicked = false;

        Manager.Instance.boardManager.draggingCard = true;

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
