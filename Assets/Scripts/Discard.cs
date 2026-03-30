using UnityEngine;
using UnityEngine.EventSystems;


public class Discard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Manager.Instance.boardManager.hoveringDiscard = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Manager.Instance.boardManager.hoveringDiscard = false;
    }
}
