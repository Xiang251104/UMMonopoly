using UnityEngine;

namespace UMMonopoly.Data
{
    [CreateAssetMenu(fileName = "TileData", menuName = "UMMonopoly/Tile Data")]
    public class TileDataSO : ScriptableObject
    {
        [Header("Identity")]
        public int position;
        public string tileName;
        public TileType type;

        [Header("Property / Station / Utility")]
        public int purchasePrice;
        public ColorGroup colorGroup = ColorGroup.None;

        [Tooltip("Rent at upgrade levels: [base, +1, +2, +3, +postgrad]. Length 5 for properties, 4 for stations, ignored for utilities.")]
        public int[] rentTable;
        public int upgradeCost;

        [Header("Tax")]
        public int taxAmount;

        [Header("Card")]
        public CardDeckType deckType;

        [Header("Visual")]
        public Sprite icon;
        public Color tileColor = Color.white;
    }
}
