using UnityEngine;
using UnityEngine.EventSystems;

public class CardObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public RectTransform hand;
    public Card card = null;

    public float scale = 1.25f;
    bool target;
    public void OnPointerEnter(PointerEventData eventData)
    {
        target = true;
        transform.localScale *= scale;
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        target = false;
        transform.localScale *= 1 / scale;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!target) { return; }

        if (card == null) { return; }

        Manager.Instance.boardManager.heldCard = card;
        Manager.Instance.deckManager.physicalCardHeld = this;
    }
}
