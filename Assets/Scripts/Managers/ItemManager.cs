using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    public List<ItemList> playerItems = new List<ItemList>();

    private void Start()
    {
        playerItems.Clear();

        Item item = ItemCreator.GetRandom();
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
    public ItemResponse TriggerOnTimeTick()
    {
        ItemResponse response = new ItemResponse();
        foreach (var item in playerItems)
        {
            ItemResponse r = item.item.OnTimeTick(item.stacks);
            response.integer += r.integer;
            foreach (var statusEffect in r.statusEffects)
                response.statusEffects.Add(statusEffect);
        }
        return response;
    }
    public ItemResponse TriggerOnEnemyAct(EnemyUnit target)
    {
        ItemResponse response = new ItemResponse();
        foreach (var item in playerItems)
        {
            ItemResponse r = item.item.OnEnemyAct(item.stacks, target);
            response.integer += r.integer;
            foreach (var statusEffect in r.statusEffects)
                response.statusEffects.Add(statusEffect);
        }
        return response;
    }
}


