using System;
using System.Collections.Generic;
using System.Linq;

public static class ItemCreator
{
    private static List<Type> _itemTypes;

    static ItemCreator()
    {
        _itemTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(Item)) && !t.IsAbstract)
            .ToList();
    }

    public static Item GetRandom()
    {
        Type randomType = _itemTypes[UnityEngine.Random.Range(0, _itemTypes.Count)];
        return (Item)Activator.CreateInstance(randomType);
    }

    public static Item GetRandomOfRarity(Rarity rarity)
    {
        var filtered = _itemTypes
            .Select(t => (Item)Activator.CreateInstance(t))
            .Where(i => i.GiveRarity() == rarity)
            .ToList();

        return filtered[UnityEngine.Random.Range(0, filtered.Count)];
    }
}