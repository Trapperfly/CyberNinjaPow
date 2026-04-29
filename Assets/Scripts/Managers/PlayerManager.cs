using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int playerResource;
    public int maxPlayerResource;

    public bool UseResource(int value)
    {
        if (playerResource < value) return false;
        
        playerResource -= value;

        DisplayResources();
        return true;
    }
    public void AddResource(int value)
    {
        playerResource += value;
        if (playerResource > maxPlayerResource) playerResource = maxPlayerResource;
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

    }
}
