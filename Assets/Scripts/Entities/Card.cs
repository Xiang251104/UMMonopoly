using UMMonopoly.Data;

namespace UMMonopoly.Entities
{
    public class Card
    {
        public CardDataSO Data { get; }
        public string Description => Data.description;
        public CardEffectType Effect => Data.effect;
        public int Amount => Data.amount;

        public Card(CardDataSO data)
        {
            Data = data;
        }
    }
}
