using UnityEngine;
using System.Collections.Generic;

public abstract class Tag
{
    public abstract string GiveName();
    public abstract TagEnum GiveTag();
    public virtual TagResponse OnTarget(TagResponse response)
    {
        return null;
    }
    public virtual TagResponse OnNonTarget(TagResponse response)
    {
        return null;
    }
}

public class TagResponse
{
    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    public bool activateBurn = false;
    public int costChange = 0;
    public int omniboost = 0;
    public int cardDraw = 0;
    public int pushNorth = 0;
    public int classResource = 0;
    public bool targetAnywhere = false;
    public int cardDrawWhenDiscarded = 0;
    public int bonusToPierceOrDamage = 0;
    public int additionalRepeats = 0;

    public void Print()
    {
        foreach (StatusEffect status in statusEffects) 
            Debug.Log("Status: " + status.ToString());
        Debug.Log("Activates burn: " + (activateBurn ? "Yes" : "No"));
        Debug.Log("Cost change: " + costChange.ToString());
        Debug.Log("Omniboost: " + omniboost.ToString());
        Debug.Log("Card Draw: " + cardDraw.ToString());
        Debug.Log("Push North: " + pushNorth.ToString());
        Debug.Log("Class Resource: " + classResource.ToString());
        Debug.Log("Target anywhere: " + (targetAnywhere ? "Yes" : "No"));
        Debug.Log("Card when discard: " + cardDrawWhenDiscarded.ToString());
        Debug.Log("Power: " + bonusToPierceOrDamage.ToString());
        Debug.Log("Play the card again: " + additionalRepeats.ToString());
    }
}
public enum TagEnum
{
    None,
    Damage,         //Damage taken by player
    Basic,          //Is on the starting cards, does nothing.
    Flame,          //Usually applies or manipulates flame status effect
    Acid,           //Usually applies or manipulates acid status effect
    Hacking,        //Usually applies or manipulates hacking status effect
    Explosive,      //Is something explosive, usually coupled with Area
    Cards,          //Usually relates to drawing, discarding, and making cards
    Swift,          //Is on cheap cards, makes cards cost less.
    Flexible,       //Is on cards that areeasy to use, add to make easy to use or flexible.
    Power,          //Is on cards that are strong, add to make stronger.

}
//Thunder,        //Usually applies or manipulates thunder status effect
//Projectile,     //Is something that launches projectiles
//Precision,      //Is something that targets one single tile
//Area,           //Is something that targest a lot of tiles
//Cryo,           //Usually applies or manipulates cryo status effect
//Deployment,     //Is something or manipulates stuff on the tactical grid
//Trap,           //Is something placed on the tactical grid that triggers
//Cleave,         //Probably not used, but large melee
//Repair,         //Usually relates to healing self or deployment
//TimeWarp,       //Usually relates to changing time in either direction
//Defence,        //Usually relates to blocking damage
[System.Serializable]
public class CardTag
{
    public bool nonFunctional = false;
    public TagEnum tag;
}
public class Burn : Tag
{
    public override string GiveName()
    {
        return "Burn";
    }
    public override TagEnum GiveTag()
    {
        return TagEnum.Flame;
    }
    public override TagResponse OnTarget(TagResponse response)
    {
        response.statusEffects.Add(StatusEffect.Burning);
        return response;
    }
    public override TagResponse OnNonTarget(TagResponse response)
    {
        //Manager.Instance.enemyManager.ActivateBurn();
        return response;
    }
}
public class Hack : Tag
{
    public override string GiveName()
    {
        return "Hacking";
    }
    public override TagEnum GiveTag()
    {
        return TagEnum.Hacking;
    }
    public override TagResponse OnTarget(TagResponse response)
    {
        response.statusEffects.Add(StatusEffect.Hacked);
        return response;
    }
    public override TagResponse OnNonTarget(TagResponse response)
    {
        response.omniboost++;
        return response;
    }
}
public class Acid : Tag
{
    public override string GiveName()
    {
        return "Acid";
    }
    public override TagEnum GiveTag()
    {
        return TagEnum.Acid;
    }
    public override TagResponse OnTarget(TagResponse response)
    {
        response.statusEffects.Add(StatusEffect.Acid);
        return response;
    }
    public override TagResponse OnNonTarget(TagResponse response)
    {
        response.omniboost++;
        return response;
    }
}
public class Cards : Tag
{
    public override string GiveName()
    {
        return "Cards";
    }
    public override TagEnum GiveTag()
    {
        return TagEnum.Cards;
    }
    public override TagResponse OnTarget(TagResponse response)
    {
        response.cardDraw++;
        return response;
    }
    public override TagResponse OnNonTarget(TagResponse response)
    {
        response.cardDraw++;
        return response;
    }
}
public class Explosive : Tag
{
    public override string GiveName()
    {
        return "Explosive";
    }
    public override TagEnum GiveTag()
    {
        return TagEnum.Explosive;
    }
    public override TagResponse OnTarget(TagResponse response)
    {
        response.pushNorth++;
        return response;
    }
    public override TagResponse OnNonTarget(TagResponse response)
    {
        response.classResource++;
        return response;
    }
}
public class Swift : Tag
{
    public override string GiveName()
    {
        return "Swift";
    }
    public override TagEnum GiveTag()
    {
        return TagEnum.Swift;
    }
    public override TagResponse OnTarget(TagResponse response)
    {
        response.costChange--;
        return response;
    }
    public override TagResponse OnNonTarget(TagResponse response)
    {
        response.costChange--;
        return response;
    }
}
public class Flexible : Tag
{
    public override string GiveName()
    {
        return "Flexible";
    }
    public override TagEnum GiveTag()
    {
        return TagEnum.Flexible;
    }
    public override TagResponse OnTarget(TagResponse response)
    {
        response.targetAnywhere = true;
        return response;
    }
    public override TagResponse OnNonTarget(TagResponse response)
    {
        response.cardDrawWhenDiscarded++;
        return response;
    }
}
public class Power : Tag
{
    public override string GiveName()
    {
        return "Power";
    }
    public override TagEnum GiveTag()
    {
        return TagEnum.Power;
    }
    public override TagResponse OnTarget(TagResponse response)
    {
        response.bonusToPierceOrDamage++;
        return response;
    }
    public override TagResponse OnNonTarget(TagResponse response)
    {
        response.additionalRepeats++;
        return response;
    }
}
public class Basic : Tag
{
    public override string GiveName()
    {
        return "Basic";
    }
    public override TagEnum GiveTag()
    {
        return TagEnum.Basic;
    }
    public override TagResponse OnTarget(TagResponse response)
    {
        //Effect
        return response;
    }
    public override TagResponse OnNonTarget(TagResponse response)
    {
        //Effect
        return response;
    }
}
//public class Name : Tag
//{
//    public override string GiveName()
//    {
//        return "Name";
//    }
//public override TagEnum GiveTag()
//{
//    return TagEnum.Tag;
//}
//    public override TagResponse OnTarget(TagResponse response)
//    {
//        //Effect
//        return response;
//    }
//    public override TagResponse OnNonTarget(TagResponse response)
//    {
//        //Effect
//        return response;
//    }
//}