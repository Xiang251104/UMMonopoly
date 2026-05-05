using UnityEngine;

namespace UMMonopoly.Data
{
    [CreateAssetMenu(fileName = "Card", menuName = "UMMonopoly/Card")]
    public class CardDataSO : ScriptableObject
    {
        public CardDeckType deck;

        [TextArea(2, 4)]
        public string description;

        public CardEffectType effect;

        [Tooltip("Money amount, tile index, or step count depending on effect type.")]
        public int amount;
    }
}
