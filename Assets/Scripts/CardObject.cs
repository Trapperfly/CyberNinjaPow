using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler
{
    public RectTransform hand;
    public Card card = null;

    public float scale = 1.25f;
    public float offset = 1f;

    public int handIndex = 0;
    //int savedHandIndex = 0;

    public bool target;
    public bool scaled = false;

    public TMP_Text cardName;
    public TMP_Text cardDescription;

    public Image art;

    public Transform time;
    public Transform range;
    public Transform tags;

    public bool clicked = false;
    public bool display = false;

    private static CardObject currentlyScaled;
    //private void Update()
    //{
    //    if (Manager.Instance.deckManager.cardRedied || Manager.Instance.busy) return;
    //    if (target && !scaled && currentlyScaled == null)
    //    {
    //        currentlyScaled = this;
    //        Scale();
    //    }
    //    else if (!target && scaled)
    //    {
    //        currentlyScaled = null;
    //        Unscale();
    //    }
    //}

    void Scale()
    {
        scaled = true;
        transform.localScale = scale * Vector3.one;
        transform.localPosition += new Vector3(0, offset, 0);
        if (!display)
        {
            Manager.Instance.deckManager.AlignCardsAsSiblings();
            transform.SetAsLastSibling(); // always on top, after alignment
        }
    }

    void Unscale()
    {
        scaled = false;
        transform.localScale = Vector3.one;
        transform.localPosition += new Vector3(0, -offset, 0);
        if (!display) Manager.Instance.deckManager.AlignCardsAsSiblings(); // restores correct order
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Manager.Instance.deckManager.cardRedied || Manager.Instance.busy) return;
        if (!scaled) Scale();
        target = true;

        Manager.Instance.tutorialManager.ShowTutorial(Tutorials.WhenCardIsHovered);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Manager.Instance.deckManager.cardRedied || Manager.Instance.busy) return;
        if (scaled) Unscale();
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
