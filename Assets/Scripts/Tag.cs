using UnityEngine;
using System.Collections.Generic;

public abstract class Tag
{
    public abstract string GiveName();
    public abstract CardTag GiveTag();
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
    public int costChange = 0;
    public int omniboost = 0;
    public int cardDraw = 0;
    public int pushNorth = 0;
    public int classResource = 0;
    public bool targetAnywhere = false;
    public int cardDrawWhenDiscarded = 0;
    public int bonusToPierceOrDamage = 0;
    public int additionalRepeats = 0;
}
public enum CardTag
{
    None,
    Damage,         //Damage taken by player
    Flame,          //Usually applies or manipulates flame status effect
    Thunder,        //Usually applies or manipulates thunder status effect
    Hacking,        //Usually applies or manipulates hacking status effect
    Projectile,     //Is something that launches projectiles
    Precision,      //Is something that targets one single tile
    Area,           //Is something that targest a lot of tiles
    Cryo,           //Usually applies or manipulates cryo status effect
    Deployment,     //Is something or manipulates stuff on the tactical grid
    Explosive,      //Is something explosive, usually coupled with Area
    Trap,           //Is something placed on the tactical grid that triggers
    Cleave,         //Probably not used, but large melee
    Cards,          //Usually relates to drawing, discarding, and making cards
    Repair,         //Usually relates to healing self or deployment
    TimeWarp,       //Usually relates to changing time in either direction
    Defence,        //Usually relates to blocking damage
    Swift,
    Flexible,
    Power,
}
public class Burn : Tag
{
    public override string GiveName()
    {
        return "Burn";
    }
    public override CardTag GiveTag()
    {
        return CardTag.Flame;
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
public class Hacking : Tag
{
    public override string GiveName()
    {
        return "Hacking";
    }
    public override CardTag GiveTag()
    {
        return CardTag.Hacking;
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
public class Cards : Tag
{
    public override string GiveName()
    {
        return "Cards";
    }
    public override CardTag GiveTag()
    {
        return CardTag.Cards;
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
    public override CardTag GiveTag()
    {
        return CardTag.Explosive;
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
    public override CardTag GiveTag()
    {
        return CardTag.Swift;
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
    public override CardTag GiveTag()
    {
        return CardTag.Flexible;
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
    public override CardTag GiveTag()
    {
        return CardTag.Power;
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
//public class Name : Tag
//{
//    public override string GiveName()
//    {
//        return "Name";
//    }
//    public override CardTag GiveTag()
//    {
//        return CardTag.Tag;
//    }
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