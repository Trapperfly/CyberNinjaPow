using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int playerResource;
    public int maxPlayerResource;

    public Transform classResourceAmmo;
    public GameObject ammoPrefab;

    public int block;

    public int playerMove = 1;
    public int playerMoveCost = 1;
    public bool UseResource(int value)
    {
        if (playerResource < value) return false;
        
        playerResource -= value;

        DisplayResources();
        return true;
    }
    public void ChangeResource(int value)
    {
        playerResource += value;
        if (playerResource > maxPlayerResource) playerResource = maxPlayerResource;
        if (playerResource < 0) playerResource = 0;
        DisplayResources();
    }

    public int UseAllResource()
    {
        int value = playerResource;
        playerResource = 0;
        DisplayResources();
        return value;
    }

    public void DisplayResources()
    {
        foreach (Transform child in classResourceAmmo)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < playerResource; i++)
        {
            Transform ammo = Instantiate(ammoPrefab, classResourceAmmo).transform;
            ammo.localPosition = new(39 * i, 0, 0);
        }
    }
    public void TakeDamage(Card damageCard)
    {
        if (block > 0)
        {
            block--;
            return;
        }
        Manager.Instance.deckManager.AddCardTo(WhereDoesTheCardGo.Hand, damageCard);
        Manager.Instance.UIManager.Grade(-20);
    }
    public void ResetBlock()
    {
        block = 0;
    }

    public void PlayerMove()
    {
        Manager.Instance.enemyManager.MoveAllEnemiesToSimulatePlayerMovement(playerMove, playerMoveCost);
    }
}
