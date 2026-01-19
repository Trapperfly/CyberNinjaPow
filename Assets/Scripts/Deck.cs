using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Deck", menuName = "Card/Deck")]
public class Deck : ScriptableObject
{
    public string deckName;
    public List<Card> cards = new();
}