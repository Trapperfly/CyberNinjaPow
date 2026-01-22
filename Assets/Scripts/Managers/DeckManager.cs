using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DeckManager : MonoBehaviour
{
    public int handSize = 5;

    public GameObject cardPrefab;

    public RectTransform handTransform;

    public CardObject physicalCardHeld;

    public List<Card> draw = new List<Card>();
    public List<Card> discard = new List<Card>();
    public List<Card> hand = new List<Card>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) DrawCard();
        if (Input.GetKeyDown(KeyCode.D)) DiscardRandomHandCard();
    }

    void CreateCard(Card card)
    {
        GameObject cardGO = Instantiate(cardPrefab, Vector2.zero, Quaternion.identity, handTransform);
        cardGO.GetComponentInChildren<TMP_Text>().text = card.name;
        cardGO.GetComponent<CardObject>().card = card;
        AlignCards();
    }

    void AlignCards(int offset = 0)
    {
        int i = 0;
        foreach (RectTransform card in handTransform)
        {
            card.localPosition = new((160f * i) - (160f * (handTransform.childCount - 1 + offset) / 2), 0, 0);
            i++;
        }
    }

    public void DrawCard(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            if (hand.Count >= handSize) { return; }

            if (draw.Count <= 0) ShuffleDiscardIntoDraw();

            Card drawnCard = draw[Random.Range(0, draw.Count)];
            draw.Remove(drawnCard);
            hand.Add(drawnCard);

            CreateCard(drawnCard);
        }
    }
    public void ShuffleDiscardIntoDraw()
    {
        foreach (Card card in discard)
        {
            draw.Add(card);
        }
        discard.Clear();
    }
    public void DiscardOrUseCard(Card card)
    {
        discard.Add(card);
        Destroy(physicalCardHeld.gameObject);
        hand.Remove(card);
        AlignCards(-1);
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
