using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public int handSize = 5;

    public List<Card> cards = new List<Card>();
    public List<Card> hand = new List<Card>();

    public Dictionary<Card, string> availableCards = new Dictionary<Card, string>();
}
