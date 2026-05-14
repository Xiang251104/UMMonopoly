using System.Collections.Generic;
using UnityEngine;

namespace UMMonopoly.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "UMMonopoly/Game Config")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Money")]
        public int startingMoney = 1500;
        public int salaryOnGo = 200;

        [Header("Jail")]
        public int jailFine = 50;
        public int maxJailTurns = 3;
        public int jailTilePosition = 10;
        public int goToJailTilePosition = 30;

        [Header("Win Condition")]
        public int targetWinScore = 0;            // 0 = play until last solvent player
        public int boardSize = 40;

        [Header("Dice")]
        public int diceCount = 2;
        public int diceSides = 6;
        public int maxDoublesBeforeJail = 3;

        [Header("Tile + Card Data")]
        public List<TileDataSO> tiles;
        public List<CardDataSO> akademikDeck;
        public List<CardDataSO> kampusDeck;
    }
}
