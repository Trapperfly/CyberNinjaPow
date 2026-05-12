using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int playerResource;
    public int maxPlayerResource;

    public Transform classResourceAmmo;
    public GameObject ammoPrefab;

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
}
