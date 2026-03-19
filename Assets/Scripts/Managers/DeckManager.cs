using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public int handSize = 5;
    public float cardSpread = 160f;
    public bool cardRedied = false;

    public GameObject cardPrefab;
    public Deck deck;

    public RectTransform handTransform;

    public CardObject physicalCardHeld;

    public List<Card> draw = new List<Card>();
    public List<Card> discard = new List<Card>();
    public List<Card> hand = new List<Card>();

    Dictionary<Vector2Int, Sprite> tagSprites = new Dictionary<Vector2Int, Sprite>();

    [System.Serializable]
    public class TagVariant
    {
        public CardTag tag;
        public int variant;
    }

    private void Start()
    {
        var loaded = Resources.LoadAll<Sprite>("Sprites/UI/Icons/Tags/icon_sheet_v2");
        foreach (Sprite sprite in loaded)
        {
            tagSprites.Add(new Vector2Int(Mathf.RoundToInt(sprite.rect.x / 7), Mathf.RoundToInt((loaded[0].texture.height - sprite.rect.y - 7) / 7) + 1), sprite);
        }
        foreach (Card card in deck.cards)
        {
            draw.Add(card);
        }
        DrawCard(handSize);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !Manager.Instance.busy) DrawCard(handSize);
        if (Input.GetKeyDown(KeyCode.D)) DiscardRandomHandCard();
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
                CreateCard(card);
                break;
            case WhereDoesTheCardGo.Draw:
                draw.Add(card);
                break;
            case WhereDoesTheCardGo.Discard:
                discard.Add(card);
                break;
            default:
                break;
        }
    }

    public enum WhereDoesTheCardGo
    {
        Nowhere,
        Hand,
        Draw,
        Discard
    }

    void CreateCard(Card card)
    {
        GameObject cardGO = Instantiate(cardPrefab, Vector2.zero, Quaternion.identity, handTransform);
        CardObject cardObject = cardGO.GetComponent<CardObject>();
        cardObject.cardName.text = card.name;
        cardObject.cost.text = card.cost.ToString();
        cardObject.card = card;
        int i = 0;
        foreach (CardTag tag in card.cardTags)
        {
            cardObject.tags.GetChild(i).gameObject.SetActive(true);
            cardObject.tags.GetChild(i).GetComponent<Image>().sprite = tagSprites[new Vector2Int(0,(int)tag)];
            i++;
        }
        AlignCards();
    }

    void AlignCards(int offset = 0)
    {
        int i = 0;
        foreach (RectTransform card in handTransform)
        {
            card.localPosition = new((cardSpread * i) - (cardSpread * (hand.Count - 1) / 2), 0, 0);
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

            CreateCard(drawnCard);
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
        hand.Remove(card);
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
