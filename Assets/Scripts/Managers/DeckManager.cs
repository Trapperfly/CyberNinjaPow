using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using UnityEngine.Rendering;
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
            int selected = Random.Range(0, deck.cards.Count);
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
        int i = Random.Range(0, deck.possibleCards.Count);
        deck.cards.Add(deck.possibleCards[i]);
        Debug.Log("Added " + deck.possibleCards[i].name + " to deck.");
    }
    public Card GetRandomCard()
    {
        int i = Random.Range(0, deck.possibleCards.Count);
        Card card = deck.possibleCards[i];
        return card;
    }


    public void AddCardTo(WhereDoesTheCardGo where, Card card)
    {
        switch (where)
        {
            case WhereDoesTheCardGo.Nowhere:
                break;
            case WhereDoesTheCardGo.Hand:
                //Discard one card first if not enough space in hand.
                hand.Add(card);
                GameObject cardGO = CreateCard(card);
                cardGO.transform.SetParent(handTransform);
                cardGO.transform.localScale = Vector3.one;
                handCards.Add(cardGO.transform);

                AlignCards();
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
        if (where == WhereDoesTheCardGo.Hand) AlignCards();
    }

    public GameObject CreateCard(Card card)
    {
        Card oldCard = card;
        card = AddTagEffectsToCard(Instantiate(card));
        card.original = oldCard;
        GameObject cardGO = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, canvas.transform);
        CardObject cardObject = cardGO.GetComponent<CardObject>();
        cardObject.cardName.text = card.cardName;
        cardObject.cardDescription.text = card.description;
        cardObject.art.sprite = card.artwork;
        if (card.cost < 0) { cardObject.time.GetComponent<Image>().sprite = timeSprites[timeSprites.Count - 1]; }
        else
            cardObject.time.GetComponent<Image>().sprite = timeSprites[card.cost];
        cardObject.range.GetComponent<Image>().sprite = rangeSprites[(int)card.range];
        cardObject.tags.GetComponent<Image>().sprite = tagHolderSprites[card.extraTagSlots + card.cardTags.Count];
        cardObject.card = card;

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
        Debug.Log(card.cardName);
        response.Print();
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

        for (int i = 0; i < count; i++)
        {
            RectTransform card = handTransform.GetChild(i) as RectTransform;

            if (card == null) continue;

            float x = count > 1 ? (spread / (count - 1) * i) - spread / 2f : 0f;
            card.localPosition = new Vector3(x, 0, 0);
        }
    }

    public void AlignCardsAsSiblings()
    {
        if (hand.Count == 0) return;

        for (int i = 0; i < handCards.Count; i++)
        {
            handCards[i].SetSiblingIndex(i);
        }

        //CardObject[] cardObjects = handTransform.GetComponentsInChildren<CardObject>();

        //foreach (CardObject co in cardObjects)
        //{
        //    co.transform.SetSiblingIndex(co.handIndex);
        //}
    }

    public void DrawPile(int amount = 1, int timeProgress = 1, bool cardDraw = false)
    {
        StartCoroutine(IDrawPile(amount, timeProgress, cardDraw));
    }

    public IEnumerator IDrawPile(int amount = 1, int timeProgress = 1, bool cardDraw = false)
    {
        Manager.Instance.busy = true;
        amount = cardDraw ? amount + 1 : amount;
        yield return null;
        for (int i = 0; i < amount; i++)
        {
            if (hand.Count >= handSize) { break; }

            DrawCard();
            Debug.Log("Drawing Cards");

            yield return new WaitForSeconds(drawAnimTime);
        }

        Manager.Instance.gameManager.ProgressTime(timeProgress);
        AlignCards();
        AlignCardsAsSiblings();
        Manager.Instance.busy = false;
        yield return null;
    }
    public Card DrawCard()
    {
        if (draw.Count <= 0) ShuffleDiscardIntoDraw();

        Card drawnCard = draw[Random.Range(0, draw.Count)];
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
            int selected = Random.Range(0, discard.Count);
            draw.Add(discard[selected]);
            discard.RemoveAt(selected);
        }
    }
    public void DiscardOrUseCard(Card card, int cost, bool discardTheCard = false)
    {
        discard.Add(card.original);

        handCards.Remove(physicalCardHeld.transform);
        hand.Remove(card.original);

        Destroy(physicalCardHeld.gameObject);

        AlignCards();

        if (!discardTheCard) Manager.Instance.gameManager.ProgressTime(cost);
    }

    //public void DiscardRandomHandCard(int amount = 1)
    //{
    //    for (int i = 0; i < amount; i++)
    //    {
    //        if (hand.Count <= 0) { return; }

    //        Card drawnCard = hand[Random.Range(0, hand.Count)];
    //        hand.Remove(drawnCard);
    //        discard.Add(drawnCard);
    //    }
    //}
    public void DiscardNextDraw()
    {
        Card drawnCard = draw[Random.Range(0, draw.Count)];
        draw.Remove(drawnCard);
        discard.Add(drawnCard);
    }
}
