using UnityEngine;
using UnityEngine.EventSystems;

public class DrawPile : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Manager.Instance.deckManager.DrawCard(Manager.Instance.deckManager.handSize);
    }
}
