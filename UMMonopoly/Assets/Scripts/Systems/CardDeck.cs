using System.Collections.Generic;
using UMMonopoly.Data;
using UMMonopoly.Entities;

namespace UMMonopoly.Systems
{
    public class CardDeck
    {
        private readonly Queue<Card> _drawPile = new Queue<Card>();
        private readonly List<Card> _allCards = new List<Card>();
        private readonly System.Random _rng;
        public CardDeckType Type { get; }

        public CardDeck(CardDeckType type, IEnumerable<CardDataSO> cards, int seed = 0)
        {
            Type = type;
            _rng = seed == 0 ? new System.Random() : new System.Random(seed);
            foreach (var c in cards)
            {
                _allCards.Add(new Card(c));
            }
            Shuffle();
        }

        public Card Draw()
        {
            if (_drawPile.Count == 0) Shuffle();
            return _drawPile.Dequeue();
        }

        public void Shuffle()
        {
            _drawPile.Clear();
            var copy = new List<Card>(_allCards);
            for (int i = copy.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (copy[i], copy[j]) = (copy[j], copy[i]);
            }
            foreach (var c in copy) _drawPile.Enqueue(c);
        }
    }
}
