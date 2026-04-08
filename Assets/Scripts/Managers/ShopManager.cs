using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject cardInteractablePrefab;
    public GameObject itemInteractablePrefab;

    public List<Vector2Int> shopCardAmount = new List<Vector2Int>();
    public List<Vector2Int> shopItemAmount = new List<Vector2Int>();

    public ShopInteractable cardSelection;
    public ShopInteractable itemSelection;

    public Transform cardShop;
    public Transform itemShop;

    public TMP_Text moneyText;

    public Canvas canvas;
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

        cardSelection.Generate();
        itemSelection.Generate();

        int randomCardAmount = Random.Range(shopCardAmount[(int)quality-1].x, shopCardAmount[(int)quality-1].y + 1);
        int randomItemAmount = Random.Range(shopItemAmount[(int)quality-1].x, shopItemAmount[(int)quality-1].y + 1);

        for (int i = 0; i < randomCardAmount; i++)
        {
            GameObject cardInteractable = Instantiate(cardInteractablePrefab, cardShop);
            cardInteractable.transform.localPosition = Vector3.zero + new Vector3(200 * i, 0, 0);

            ShopInteractable specificCard = cardInteractable.GetComponent<ShopInteractable>();
            specificCard.Generate();

            cardInteractable.transform.GetChild(1).GetComponent<TMP_Text>().text = specificCard.cost + "$";
        }

        for (int i = 0; i < randomItemAmount; i++)
        {
            GameObject itemInteractable = Instantiate(itemInteractablePrefab, itemShop);
            itemInteractable.transform.localPosition = Vector3.zero + new Vector3(200 * i, 0, 0);

            ShopInteractable specificItem = itemInteractable.GetComponent<ShopInteractable>();
            specificItem.Generate();

            itemInteractable.transform.GetChild(1).GetComponent<TMP_Text>().text = specificItem.cost + "$";
        }
    }

    public void ResetShop()
    {
        foreach (Transform slot in cardShop)
        {
            Destroy(slot.gameObject);
        }
        foreach (Transform slot in itemShop)
        {
            Destroy(slot.gameObject);
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