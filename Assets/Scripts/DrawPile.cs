using UnityEngine;
using UnityEngine.EventSystems;

public class DrawPile : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Manager.Instance.busy) return;
        Manager.Instance.deckManager.DrawCard(Manager.Instance.deckManager.handSize);
    }
}
