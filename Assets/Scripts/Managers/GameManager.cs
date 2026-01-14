using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float hehe;

    private void Start()
    {
        Manager.Instance.deckManager.handSize = 2000000;
    }
}
