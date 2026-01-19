using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public int handSize = 5;

    public List<Card> draw = new List<Card>();
    public List<Card> discard = new List<Card>();
    public List<Card> hand = new List<Card>();
}
