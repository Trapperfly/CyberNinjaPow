using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public List<Vector2Int> shopCardAmount = new List<Vector2Int>();
    public List<Vector2Int> shopItemAmount = new List<Vector2Int>();

    public Transform cardShop;
    public Transform itemShop;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateShop();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            GenerateShop(ShopQuality.Elite);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            GenerateShop(ShopQuality.Boss);
        }
    }
    public void GenerateShop(ShopQuality quality = ShopQuality.Normal)
    {
        ResetShop();

        int randomCardAmount = Random.Range(shopCardAmount[(int)quality-1].x, shopCardAmount[(int)quality-1].y + 1);
        int randomItemAmount = Random.Range(shopItemAmount[(int)quality-1].x, shopItemAmount[(int)quality-1].y + 1);

        Transform cardSlots = cardShop.GetChild(0);
        Transform cardCost = cardShop.GetChild(1);

        for (int i = 0; i < randomCardAmount; i++)
        {
            cardSlots.GetChild(i).gameObject.SetActive(true);

            cardCost.GetChild(i).gameObject.SetActive(true);
            cardCost.GetChild(i).GetComponent<TMP_Text>().text = i.ToString() + "$";
        }

        Transform itemSlots = itemShop.GetChild(0);
        Transform itemCost = itemShop.GetChild(1);

        for (int i = 0; i < randomItemAmount; i++)
        {
            itemSlots.GetChild(i).gameObject.SetActive(true);

            itemCost.GetChild(i).gameObject.SetActive(true);
            itemCost.GetChild(i).GetComponent<TMP_Text>().text = i.ToString() + "$";
        }
    }

    public void ResetShop()
    {
        foreach (Transform slot in cardShop.GetChild(0))
        {
            slot.gameObject.SetActive(false);
        }
        foreach (Transform slot in cardShop.GetChild(1))
        {
            slot.gameObject.SetActive(false);
        }
        foreach (Transform slot in itemShop.GetChild(0))
        {
            slot.gameObject.SetActive(false);
        }
        foreach (Transform slot in itemShop.GetChild(1))
        {
            slot.gameObject.SetActive(false);
        }
    }

    public Card GetRandomCardFromAvailableCards(CardRarity rarity = CardRarity.none)
    {
        List<Card> availableCards = new List<Card>();

        foreach (Card card in Manager.Instance.deckManager.deck.possibleCards)
        {
            switch (rarity)
            {
                case CardRarity.none:
                    break;
                case CardRarity.Common:
                case CardRarity.Uncommon:
                case CardRarity.Rare:
                case CardRarity.Legendary:
                    if (card.rarity != rarity) continue;
                    break;
                default:
                    break;
            }
            availableCards.Add(card);
        }

        if (availableCards.Count <= 0) return null;

        Card selectedCard = availableCards[Random.Range(0, availableCards.Count)];

        return selectedCard;
    }
}

public enum ShopQuality
{
    none, 
    Normal,
    Elite,
    Boss
}

public enum CardRarity
{
    none,
    Common,
    Uncommon,
    Rare,
    Legendary
}