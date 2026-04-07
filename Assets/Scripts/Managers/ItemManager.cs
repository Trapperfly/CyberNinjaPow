using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    public List<ItemList> playerItems = new List<ItemList>();

    private void Start()
    {
        playerItems.Clear();

        Item item = new RocketFists();
        ItemList startingItem = new ItemList(item, item.GiveName(), 1);
        playerItems.Add(startingItem);
    }
}


