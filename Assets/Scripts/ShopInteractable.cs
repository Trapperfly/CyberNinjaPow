using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ShopInteractable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ShopInteractableType type;
    public List<Card> cardSelection = new();
    public Card card;
    public List<Item> itemSelection = new();
    public Item item;

    public int cost = 0;

    Canvas canvas;

    private void Start()
    {
        canvas = Manager.Instance.shopManager.canvas;
    }

    public void Generate()
    {
        cardSelection.Clear();
        itemSelection.Clear();
        card = null;
        item = null;
        cost = 0;

        switch (type)
        {
            case ShopInteractableType.CardSelection:
                for (int i = 0; i < 3; i++)
                {
                    cardSelection.Add(Manager.Instance.deckManager.GetRandomCard());
                }
                break;
            case ShopInteractableType.SpecificCard:
                card = Manager.Instance.deckManager.GetRandomCard();
                cost = (int)card.rarity;

                GameObject cardGO = Manager.Instance.deckManager.CreateCard(card);
                cardGO.transform.SetParent(transform.GetChild(0));
                cardGO.transform.localPosition = Vector3.zero;
                cardGO.transform.localScale = Vector3.one;

                cardGO.GetComponent<CardObject>().display = true;
                cardGO.GetComponent<CardObject>().offset = 0;
                break;
            case ShopInteractableType.ItemSelection:
                for (int i = 0; i < 3; i++)
                {
                    itemSelection.Add(ItemCreator.GetRandom());
                }
                break;
            case ShopInteractableType.SpecificItem:
                item = ItemCreator.GetRandom();
                cost = (int)item.GiveRarity() * 3;
                break;
            default:
                break;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        //Do action
        switch (type)
        {
            case ShopInteractableType.ExitShop:         //Remove the shop interface.
                canvas.gameObject.SetActive(false);
                Manager.Instance.gameManager.StartWave();
                break;
            case ShopInteractableType.Refresh:          //Refreshes the shop with new instances of interactables.
                if (Manager.Instance.gameManager.money >= cost)
                {
                    Manager.Instance.gameManager.AlterMoney(-cost);
                    Manager.Instance.shopManager.GenerateShop();
                }
                break;
            case ShopInteractableType.RemoveCard:       //Show the menu for removing cards from the deck.
                break;
            case ShopInteractableType.CardSelection:    //Show the menu for selecting cards.
                break;
            case ShopInteractableType.SpecificCard:     //Buy the specific card if enough money.
                if (Manager.Instance.gameManager.money >= cost)
                {
                    Manager.Instance.gameManager.AlterMoney(-cost);
                    Manager.Instance.deckManager.AddCardTo(WhereDoesTheCardGo.Deck, card);
                    Destroy(gameObject);
                }
                break;
            case ShopInteractableType.ItemSelection:    //Show the menu for selecting items.
                break;
            case ShopInteractableType.SpecificItem:     //Buy the specific item if enough money.
                if (Manager.Instance.gameManager.money >= cost)
                {
                    Manager.Instance.gameManager.AlterMoney(-cost);
                    Manager.Instance.itemManager.playerItems.Add(new ItemList(item, item.GiveName(), 1));
                    Destroy(gameObject);
                }
                break;
            default:
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //If use, display an explanation or show a larger variant of Interactable
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Reset all possible OnPointerEnter events.
    }
}

public enum ShopInteractableType
{
    ExitShop,
    Refresh,
    RemoveCard,
    CardSelection,
    SpecificCard,
    ItemSelection,
    SpecificItem
}