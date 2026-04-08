using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
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

    public Canvas canvas;

    public GameObject cardPrefab;
    public Deck deck;
    public List<Card> accumulatedDamageCards = new List<Card>();

    public RectTransform handTransform;

    public CardObject physicalCardHeld;

    public List<Card> draw = new List<Card>();
    public List<Card> discard = new List<Card>();
    public List<Card> hand = new List<Card>();

    Dictionary<Vector2Int, Sprite> tagSprites = new Dictionary<Vector2Int, Sprite>();
    public List<TagVariant> tagVisualSettings = new List<TagVariant>();
    public List<Sprite> timeSprites = new List<Sprite>();
    public List<Sprite> rangeSprites = new List<Sprite>();
    public List<Sprite> tagHolderSprites = new List<Sprite>();
    public List<Sprite> tagBackgroundSprites = new List<Sprite>();

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
        foreach (Card card in deck.cards)
        {
            draw.Add(card);
        }
        DrawCard(handSize);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !Manager.Instance.busy) DrawCard(handSize);
        //if (Input.GetKeyDown(KeyCode.D)) AddRandomCardToDeck();
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

    void AlignCards(int offset = 0)
    {
        int i = 0;
        foreach (RectTransform card in handTransform)
        {
            card.localPosition = new((cardSpread / hand.Count * i) - (cardSpread / hand.Count * (hand.Count - 1) / 2), 0, 0);
            i++;
        }
    }

    public void DrawCard(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            if (hand.Count >= handSize) { break; }

            if (draw.Count <= 0) ShuffleDiscardIntoDraw();

            Card drawnCard = draw[Random.Range(0, draw.Count)];
            draw.Remove(drawnCard);
            hand.Add(drawnCard);

            GameObject cardGO = CreateCard(drawnCard);
            cardGO.transform.SetParent(handTransform);
            cardGO.transform.localScale = Vector3.one;

            AlignCards();
        }

        Manager.Instance.gameManager.ProgressTime(1);
    }
    public void ShuffleDiscardIntoDraw()
    {
        foreach (Card card in discard)
        {
            draw.Add(card);
        }
        discard.Clear();
    }
    public void DiscardOrUseCard(Card card, bool discardTheCard = false)
    {
        discard.Add(card);
        Destroy(physicalCardHeld.gameObject);
        Debug.Log("Discarding/using card");
        if (hand.Remove(card)) Debug.Log("Removed card from hand");
        AlignCards(-1);

        if (!discardTheCard) Manager.Instance.gameManager.ProgressTime(card.cost);
    }
    public void DiscardRandomHandCard(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            if (hand.Count <= 0) { return; }

            Card drawnCard = hand[Random.Range(0, hand.Count)];
            hand.Remove(drawnCard);
            discard.Add(drawnCard);
        }
    }
    public void DiscardNextDraw()
    {
        Card drawnCard = draw[Random.Range(0, draw.Count)];
        draw.Remove(drawnCard);
        discard.Add(drawnCard);
    }
}
