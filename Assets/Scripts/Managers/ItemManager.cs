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

    public ItemResponse TriggerOnHit(EnemyUnit target)
    {
        ItemResponse response = new ItemResponse();
        foreach (var item in playerItems)
        {
            ItemResponse r = item.item.OnHit(item.stacks, target, target.position);
            response.integer += r.integer;
            foreach (var statusEffect in r.statusEffects)
                response.statusEffects.Add(statusEffect);
        }
        return response;
    }
}


