using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
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
            deck.cards.Add(card);
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
            Debug.Log("Added " + deck.cards[selected].cardName + " back to the deck.");
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
        GameObject cardGO = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, canvas.transform);
        CardObject cardObject = cardGO.GetComponent<CardObject>();
        cardObject.cardName.text = card.cardName;
        cardObject.cardDescription.text = card.description;
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
                if (setting.tag == tag)
                {
                    variant = setting.variant;
                    break;
                }
            }
            Transform tagGO = cardObject.tags.GetChild(i);
            tagGO.gameObject.SetActive(true);
            tagGO.GetChild(0).GetComponent<Image>().sprite = tagBackgroundSprites[(int)tag];

            RectTransform tagIconRect = tagGO.GetChild(1).GetComponent<RectTransform>();
            Image image = tagIconRect.GetComponent<Image>();

            image.sprite = tagSprites[new Vector2Int(0 + variant, (int)tag)];
            //tagIconRect.sizeDelta = new Vector2(
            //    image.sprite.rect.width,
            //    image.sprite.rect.height
            //) * 32;
            i++;
        }
        return cardGO;
    }

    public void AlignCards(int offset = 0)
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

    public void DrawPile(int amount = 1, int timeProgress = 1)
    {
        StartCoroutine(IDrawPile(amount, timeProgress));
    }

    public IEnumerator IDrawPile(int amount = 1, int timeProgress = 1)
    {
        Manager.Instance.busy = true;
        yield return null;
        for (int i = 0; i < amount; i++)
        {
            if (hand.Count >= handSize) { break; }

            DrawCard();

            yield return new WaitForSeconds(drawAnimTime);
        }

        Manager.Instance.gameManager.ProgressTime(timeProgress);
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
    public void DiscardOrUseCard(Card card, bool discardTheCard = false)
    {
        discard.Add(card);

        handCards.Remove(physicalCardHeld.transform);
        hand.Remove(card);

        Destroy(physicalCardHeld.gameObject);

        AlignCards();

        if (!discardTheCard) Manager.Instance.gameManager.ProgressTime(card.cost);
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
