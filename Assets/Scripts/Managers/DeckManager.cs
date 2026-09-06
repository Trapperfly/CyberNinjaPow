using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;
public enum WhereDoesTheCardGo
{
    Nowhere,
    Hand,
    Draw,
    Discard,
    Deck
}
public class DeckManager : MonoBehaviour
{
    public int handSize = 5;
    public float cardSpread = 600f;
    public bool cardRedied = false;
    public int drawCost = 1;
    public float drawAnimTime = 0.1f;

    public Canvas canvas;

    public GameObject cardPrefab;
    public Deck deck;
    public List<Card> accumulatedDamageCards = new List<Card>();

    public RectTransform handTransform;
    public RectTransform handHoldingCardTransform;

    public CardObject physicalCardHeld;

    public List<Card> draw = new List<Card>();
    public List<Card> discard = new List<Card>();
    public List<Card> hand = new List<Card>();

    public List<Transform> handCards = new List<Transform>();

    Dictionary<Vector2Int, Sprite> tagSprites = new Dictionary<Vector2Int, Sprite>();
    public List<TagVariant> tagVisualSettings = new List<TagVariant>();
    public List<Sprite> timeSprites = new List<Sprite>();
    public List<Sprite> rangeSprites = new List<Sprite>();
    public List<Sprite> tagHolderSprites = new List<Sprite>();
    public List<Sprite> tagBackgroundSprites = new List<Sprite>();

    public int handIndexCounter = 0;

    public Color damageCardHueShift = Color.white;

    public Color damageColor = new();
    public Color blockColor = new();
    public Color drawColor = new();
    public Color discardColor = new();
    public Color addColor = new();
    public Color pushColor = new();
    public Color timeColor = new();
    public Color classColor = new();
    public Color burnColor = new();
    public Color acidColor = new();
    public Color hackColor = new();

    public Card lightDamageForTesting;

    [System.Serializable]
    public class TagVariant
    {
        public CardTag tag;
        public int variant;
    }

    private void Start()
    {
        var loaded = Resources.LoadAll<Sprite>("Sprites/UI/Cards/Icons/Tags/icon_sheet_v2");
        foreach (Sprite sprite in loaded)
        {
            tagSprites.Add(new Vector2Int(Mathf.RoundToInt(sprite.rect.x / 7), Mathf.RoundToInt((loaded[0].texture.height - sprite.rect.y - 7) / 7) + 1), sprite);
        }
        deck = Instantiate(deck);
    }
    public void SaveDeck()
    {
        deck.cards.Clear();
        foreach (Card card in draw)
        {
            deck.cards.Add(card);
        }
        Debug.Log(deck.cards.Count);
        draw.Clear();
        foreach (Card card in discard)
        {
            deck.cards.Add(card);
        }
        Debug.Log(deck.cards.Count);
        discard.Clear();
        foreach (Card card in hand)
        {
            deck.cards.Add(card.original);
        }
        Debug.Log(deck.cards.Count);
        hand.Clear();
        ClearHand();
    }

    public void LoadDeck()
    {
        if (deck.cards.Count == 0) Debug.Log("Deck contained no cards.");
        while (deck.cards.Count > 0)
        {
            int selected = Manager.Instance.gameManager.gameSeed.Next(0, deck.cards.Count);
            //Debug.Log("Added " + deck.cards[selected].cardName + " back to the deck.");
            draw.Add(deck.cards[selected]);
            deck.cards.RemoveAt(selected);
        }
    }
    public void ClearHand()
    {
        foreach (Transform card in handTransform)
        {
            Destroy(card.gameObject);
        }
        handCards.Clear();
    }

    public void AddRandomCardToDeck()
    {
        int i = Manager.Instance.gameManager.gameSeed.Next(0, deck.possibleCards.Count);
        deck.cards.Add(deck.possibleCards[i]);
        Debug.Log("Added " + deck.possibleCards[i].name + " to deck.");
    }
    public Card GetRandomCard()
    {
        int i = Manager.Instance.gameManager.gameSeed.Next(0, deck.possibleCards.Count);
        Card card = deck.possibleCards[i];
        return card;
    }
    public void TakeLightDamage()
    {
        AddCardTo(WhereDoesTheCardGo.Hand, lightDamageForTesting);
    }

    public void AddCardTo(WhereDoesTheCardGo where, Card card)
    {
        switch (where)
        {
            case WhereDoesTheCardGo.Nowhere:
                break;
            case WhereDoesTheCardGo.Hand:
                //Discard one card first if not enough space in hand.
                //Debug.Log("Hand size is " + handSize + " and hand count is " + hand.Count);
                if (hand.Count >= handSize) DiscardRandomNonDamageHandCard();
                //Add card
                hand.Add(card);
                GameObject cardGO = CreateCard(card);
                cardGO.transform.SetParent(handTransform);
                cardGO.transform.localScale = Vector3.one;
                handCards.Add(cardGO.transform);
                break;
            case WhereDoesTheCardGo.Draw:
                draw.Add(card);
                break;
            case WhereDoesTheCardGo.Discard:
                discard.Add(card);
                break;
            case WhereDoesTheCardGo.Deck:
                deck.cards.Add(card);
                break;
            default:
                break;
        }
        AlignCards();
        AlignCardsAsSiblings();
    }

    public GameObject CreateCard(Card card)
    {
        Card oldCard = card;
        card = AddTagEffectsToCard(Instantiate(card));
        card.original = oldCard;
        GameObject cardGO = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, handTransform);
        cardGO.transform.localPosition = Vector3.zero;
        CardObject cardObject = cardGO.GetComponent<CardObject>();
        cardObject.cardName.text = card.cardName;
        cardObject.cardDescription.text = CreateRichTextDescription(card);
        cardObject.art.sprite = card.artwork;
        if (card.cost < 0) { cardObject.time.GetComponent<Image>().sprite = timeSprites[timeSprites.Count - 1]; }
        else
            cardObject.time.GetComponent<Image>().sprite = timeSprites[card.cost];
        cardObject.range.GetComponent<Image>().sprite = rangeSprites[(int)card.range];
        cardObject.tags.GetComponent<Image>().sprite = tagHolderSprites[card.extraTagSlots + card.cardTags.Count];
        cardObject.card = card;

        foreach(CardTag tag in card.cardTags)
        {
            if (tag.tag == TagEnum.Damage)
            {
                cardObject.time.GetComponent<Image>().color = damageCardHueShift;
                cardObject.range.GetComponent<Image>().color = damageCardHueShift;
                cardObject.tags.GetComponent<Image>().color = damageCardHueShift;
                break;
            }
        }

        int i = 0;

        foreach (CardTag tag in card.cardTags)
        {
            int variant = 0;
            foreach (TagVariant setting in tagVisualSettings)
            {
                if (setting.tag.tag == tag.tag)
                {
                    variant = setting.variant;
                    break;
                }
            }
            Transform tagGO = cardObject.tags.GetChild(i);
            tagGO.gameObject.SetActive(true);
            tagGO.GetChild(0).GetComponent<Image>().sprite = tagBackgroundSprites[(int)tag.tag];

            RectTransform tagIconRect = tagGO.GetChild(1).GetComponent<RectTransform>();
            Image image = tagIconRect.GetComponent<Image>();

            image.sprite = tagSprites[new Vector2Int(0 + variant, (int)tag.tag)];

            i++;
        }
        return cardGO;
    }

    public string CreateRichTextDescription(Card card)
    {
        string result = card.description + "\n\n";
        if (card.description == "") result = "";

        if (card.tileEffects.Count == 1)
        {
            if (card.tileEffects[0].projectiles.Count == 1)
            {
                result += "Shoot a " + Fancy(card.tileEffects[0].projectiles[0].projDamage, damageColor) 
                    + " damage projectile " + card.tileEffects[0].projectiles[0].direction.ToString();
            }
            else
            {
                result += "[" + Fancy(card.tileEffects[0].damage, damageColor) + "]";
            }
        }
        //else
        //{
        //    foreach (TileEffect effect in card.tileEffects)
        //    {
        //        if (effect.projectiles.Count == 0)
        //        {
        //            result += "[" + Fancy(effect.damage, damageColor) + "]";
        //        }
                
        //    }
        //}

            bool targetCard = false;
        if (card.tileEffects.Count > 0) targetCard = true;
        if (card.targetAll.doThis) targetCard = true;

        foreach (CardTag tag in card.cardTags)
        {
            if (tag.nonFunctional) continue;
            switch (tag.tag)
            {
                case TagEnum.None:
                    break;
                case TagEnum.Damage:
                    result += "Is removed instead of discarded when used.";
                    break;
                case TagEnum.Basic:
                    break;
                case TagEnum.Flame:
                    if (targetCard) result += "Inflict " + Fancy("Burning", burnColor) + ".";
                    //else result += "Deal " + Fancy(effect.amount * effect.doXTimes, burnColor) + " damage to all " + Fancy("Burning", burnColor) + " enemies.";
                    break;
                case TagEnum.Acid:
                    if (targetCard) result += "Inflict " + Fancy("Acid", acidColor) + ".";
                    //else result += ;
                    break;
                case TagEnum.Hacking:
                    if (targetCard) result += "Inflict " + Fancy("Hack", hackColor) + ".";
                    //else result += ;
                    break;
                case TagEnum.Explosive:
                    if (targetCard) result += Fancy("Push ", pushColor) + Fancy(1, pushColor) + " North.";
                    //else result += ;
                    break;
                //case TagEnum.Cards:
                //    if (targetCard) result += ;
                //    else result += ;
                //    break;
                //case TagEnum.Swift:
                //    if (targetCard) result += ;
                //    else result += ;
                //    break;
                //case TagEnum.Flexible:
                //    if (targetCard) result += ;
                //    else result += ;
                //    break;
                //case TagEnum.Power:
                //    if (targetCard) result += ;
                //    else result += ;
                //    break;
                default:
                    break;
            }
        }

        foreach (AdditionalCardEffect effect in card.additionalCardEffects)
        {
            switch (effect.otherEffect)
            {
                case OtherCardEffects.None:
                    break;
                case OtherCardEffects.Block:
                    result += "Block " + Fancy(effect.amount, blockColor) + " attacks for " + Fancy(card.cost, timeColor) + " time.";
                    break;
                case OtherCardEffects.Parry:
                    result += "Block " + Fancy(effect.amount, blockColor) + " attacks for " + Fancy(card.cost, timeColor) + " time.";
                    break;
                case OtherCardEffects.DrawCards:
                    result += "Draw " + Fancy(effect.amount * effect.doXTimes, drawColor) + " cards.";
                    break;
                case OtherCardEffects.DiscardCards:
                    result += "Discard " + Fancy(effect.amount * effect.doXTimes, discardColor) + " cards.";
                    break;
                case OtherCardEffects.AddClassResource:
                    result += "Gain " + Fancy(effect.amount * effect.doXTimes, classColor) + " class resource.";
                    break;
                case OtherCardEffects.AddCardToHand:
                    result += "Add " + Fancy(effect.amount * effect.doXTimes, addColor) + " " + Fancy(effect.card.cardName, drawColor) + " to your hand.";
                    break;
                case OtherCardEffects.AddCardToDiscard:
                    result += "Add " + Fancy(effect.amount * effect.doXTimes, addColor) + " " + Fancy(effect.card.cardName, drawColor) + " to your discard.";
                    break;
                case OtherCardEffects.AddCardToDraw:
                    result += "Add " + Fancy(effect.amount * effect.doXTimes, addColor) + " " + Fancy(effect.card.cardName, drawColor) + " to your draw.";
                    break;
                case OtherCardEffects.ActivateBurn:
                    result += "Deal " + Fancy(effect.amount * effect.doXTimes, burnColor) + " damage to all " + Fancy("Burning", burnColor) + " enemies.";
                    break;
                default:
                    break;
            }
        }

        return result;
    }
    public string Fancy(int text, Color color, bool bold = true)
    {
        string result = Fancy(text.ToString(), color.ToHexString(), bold);

        return result;
    }
    public string Fancy(string text, Color color, bool bold = true)
    {
        string result = Fancy(text, color.ToHexString(), bold);

        return result;
    }

    public string Fancy(string text, string hex = "ffffff", bool bold = true)
    {
        string result = "<color=#" + hex + ">" + text + "</color>";

        if (bold) result = "<b>" + result + "</b>";

        return result;
    }

    public Tag CreateTagFromEnum(TagEnum tag)
    {
        switch (tag)
        {
            case TagEnum.None:
                return null;
            case TagEnum.Damage:
                return null;
            case TagEnum.Flame:
                return new Burn();
            case TagEnum.Hacking:
                return new Hack();
            case TagEnum.Explosive:
                return new Explosive();
            case TagEnum.Cards:
                return new Cards();
            case TagEnum.Swift:
                return new Swift();
            case TagEnum.Flexible:
                return new Flexible();
            case TagEnum.Power:
                return new Power();
            default:
                return null;
        }
    }

    public Card AddTagEffectsToCard(Card card)
    {
        foreach (CardTag tag in card.cardTags)
        {
            if (!tag.nonFunctional) card.tags.Add(CreateTagFromEnum(tag.tag));
        }
        if (card.tags.Count == 0) return card;
        TagResponse response = new TagResponse();
        foreach (Tag tag in card.tags)
        {
            if (tag == null) continue;

            if (card.tileEffects.Count > 0 || card.targetAll.doThis)
            {
                response = tag.OnTarget(response);
            }
            else
            {
                response = tag.OnNonTarget(response);
            }
        }
        //Debug.Log(card.cardName);
        //response.Print();
        //Cost
        card.cost += response.costChange;
        if (card.cost < 0) card.cost = 0;
        //Omni
        foreach (AdditionalCardEffect ace in card.additionalCardEffects)
        {
            ace.amount += response.omniboost;
        }
        //Draw card
        if(response.cardDraw > 0)
        {
            AdditionalCardEffect additionalDraw = new AdditionalCardEffect();
            additionalDraw.otherEffect = OtherCardEffects.DrawCards;
            additionalDraw.amount += response.cardDraw;
            card.additionalCardEffects.Add(additionalDraw);
        }
        //Activate burn
        if (response.activateBurn)
        {
            AdditionalCardEffect addBurningTrigger = new AdditionalCardEffect();
            addBurningTrigger.otherEffect = OtherCardEffects.ActivateBurn;
            card.additionalCardEffects.Add(addBurningTrigger);
        }
        //Class resource
        if (response.classResource != 0)
        {
            AdditionalCardEffect addClassResource = new AdditionalCardEffect();
            addClassResource.otherEffect = OtherCardEffects.AddClassResource;
            addClassResource.amount += response.classResource;
            card.additionalCardEffects.Add(addClassResource);
        }
        //Add repeats
        card.repeats += response.additionalRepeats;
        //Card draw when discarded
        //For later

        //Add status effect
        foreach (TileEffect effect in card.tileEffects)
        {
            foreach (StatusEffect status in response.statusEffects)
            {
                StatusEffectEntry statusEntry = new StatusEffectEntry();
                statusEntry.effect = status;
                statusEntry.stacks = 1;
                statusEntry.duration = 1;

                effect.statusEffects.Add(statusEntry);
            }
        }
        //Damage or pierce
        foreach (TileEffect effect in card.tileEffects)
        {
            if (effect.projectiles.Count > 0)
                foreach (ProjectileData projectile in effect.projectiles)
                {
                    projectile.pierce += response.bonusToPierceOrDamage;
                }
            else
                effect.damage += response.bonusToPierceOrDamage;
        }
        //Target anywhere
        if (response.targetAnywhere) card.range = Range.Anywhere;
        //Push north, can probably be direction of attack or something later.
        if (response.pushNorth != 0)
        foreach (TileEffect effect in card.tileEffects)
        {
            effect.pushDirection = Direction.North;
            effect.pushDistance = response.pushNorth;
        }
        return card;
    }

    public void AlignCards()
    {
        if (hand.Count == 0) return;

        int count = hand.Count;
        float spread = Mathf.Min(cardSpread, count * 150f);

        //Debug.Log("Hand count is " + count);

        for (int i = 0; i < count; i++)
        {
            RectTransform card = handCards[i] as RectTransform;

            //Debug.Log("Aligning " + card.GetComponent<CardObject>().card.cardName);

            if (card == null) continue;

            float x = count > 1 ? (spread / (count - 1) * i) - spread / 2f : 0f;

            if (card.GetComponent<CardObject>().target) card.localPosition = new Vector3(x, card.localPosition.y, 0);
            else card.localPosition = new Vector3(x, 0, 0);
        }
    }

    public void AlignCardsAsSiblings()
    {
        if (hand.Count == 0) return;

        for (int i = 0; i < handCards.Count; i++)
        {
            if (handCards[i].GetComponent<CardObject>().target) handCards[i].SetSiblingIndex(10);
            else handCards[i].SetSiblingIndex(i);
        }

        //CardObject[] cardObjects = handTransform.GetComponentsInChildren<CardObject>();

        //foreach (CardObject co in cardObjects)
        //{
        //    co.transform.SetSiblingIndex(co.handIndex);
        //}
    }
    public void DrawCard(int amount = 1)
    {
        StartCoroutine(IDrawCard(amount));
    }
    public IEnumerator IDrawCard(int amount, int time = 0)
    {
        Manager.Instance.busy = true;
        for (int i = 0; i < amount; i++)
        {
            if (hand.Count >= handSize) { break; }

            DrawCardToHand();

            yield return new WaitForSeconds(drawAnimTime);
        }
        if (time > 0)
            Manager.Instance.gameManager.ProgressTime(time);
    }

    public void DrawPile()
    {
        StartCoroutine(IDrawCard(handSize, drawCost));
    }

    public Card DrawCardToHand()
    {
        if (draw.Count <= 0) ShuffleDiscardIntoDraw();

        Card drawnCard = draw[0];
        draw.Remove(drawnCard);
        hand.Add(drawnCard);

        GameObject cardGO = CreateCard(drawnCard);
        cardGO.transform.SetParent(handTransform);
        cardGO.transform.localScale = Vector3.one;
        handCards.Add(cardGO.transform);

        AlignCards();
        AlignCardsAsSiblings();

        return drawnCard;
    }

    public void ShuffleDiscardIntoDraw()
    {
        while (discard.Count > 0)
        {
            int selected = Manager.Instance.gameManager.gameSeed.Next(0, discard.Count);
            draw.Add(discard[selected]);
            discard.RemoveAt(selected);
        }
    }
    public void DealWithUsedCard(Card editedCard, int cost, bool isDiscarded = false, WhereDoesTheCardGo whereDoesTheCardGo = WhereDoesTheCardGo.Discard)
    {
        handCards.Remove(physicalCardHeld.transform);
        //Debug.Log(editedCard.original);
        hand.Remove(editedCard.original);

        physicalCardHeld.gameObject.SetActive(false);
        Destroy(physicalCardHeld.gameObject);

        switch (whereDoesTheCardGo)
        {
            case WhereDoesTheCardGo.Nowhere:
                break;
            case WhereDoesTheCardGo.Hand:
                AddCardTo(WhereDoesTheCardGo.Hand, editedCard.original);
                break;
            case WhereDoesTheCardGo.Draw:
                draw.Add(editedCard.original);
                break;
            case WhereDoesTheCardGo.Discard:
                discard.Add(editedCard.original);
                break;
            default:
                discard.Add(editedCard.original);
                break;
        }

        AlignCards();
        AlignCardsAsSiblings();

        if (!isDiscarded) Manager.Instance.gameManager.ProgressTime(cost);
    }

    public void DiscardRandomHandCard(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            if (hand.Count <= 0) { return; }

            int cardIndex = Manager.Instance.gameManager.gameSeed.Next(0, hand.Count);
            
            discard.Add(hand[cardIndex]);
            hand.Remove(hand[cardIndex]);

            Transform handCard = handCards[cardIndex];
            handCards.Remove(handCard);
            handCard.SetParent(null);
            handCard.gameObject.SetActive(false);
            Destroy(handCard.gameObject);

            AlignCards();
            AlignCardsAsSiblings();
        }
    }

    public void DiscardRandomNonDamageHandCard(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            if (hand.Count <= 0) { return; }

            List<int> indexOfNonDamageCards = new();

            int index = 0;
            foreach (Card card in hand)
            {
                bool damage = false;
                foreach (CardTag tag in card.cardTags)
                {
                    if (tag.tag == TagEnum.Damage) damage = true;
                    break;
                }
                if (!damage) indexOfNonDamageCards.Add(index);
                index++;
            }

            if (indexOfNonDamageCards.Count <= 0)
            {
                Debug.LogWarning("There was no non damage card to remove");
                return;
            }
            int cardIndex = indexOfNonDamageCards[Manager.Instance.gameManager.gameSeed.Next(0, indexOfNonDamageCards.Count)];

            discard.Add(hand[cardIndex]);
            hand.Remove(hand[cardIndex]);

            Transform handCard = handCards[cardIndex];
            handCards.Remove(handCard);
            handCard.SetParent(null);
            handCard.gameObject.SetActive(false);
            Destroy(handCard.gameObject);

            AlignCards();
            AlignCardsAsSiblings();
        }
    }
    public void DiscardNextDraw()
    {
        Card drawnCard = draw[0];
        draw.Remove(drawnCard);
        discard.Add(drawnCard);
    }
    public List<Card> GetDamageCardsInHand()
    {
        List<Card> cards = new();

        foreach (Card card in hand)
        {
            foreach (CardTag tag in card.cardTags)
                if (tag.tag == TagEnum.Damage) cards.Add(card);
        }

        return cards;
    }
}
