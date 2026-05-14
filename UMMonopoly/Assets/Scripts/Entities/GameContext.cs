using UMMonopoly.Data;
using UMMonopoly.Systems;

namespace UMMonopoly.Entities
{
    public class GameContext
    {
        public Board Board { get; }
        public Bank Bank { get; }
        public CardDeck AkademikDeck { get; }
        public CardDeck KampusDeck { get; }
        public GameConfigSO Config { get; }
        public System.Collections.Generic.List<Player> Players { get; }

        public GameContext(Board board, Bank bank, CardDeck akademik, CardDeck kampus,
            GameConfigSO config, System.Collections.Generic.List<Player> players)
        {
            Board = board;
            Bank = bank;
            AkademikDeck = akademik;
            KampusDeck = kampus;
            Config = config;
            Players = players;
        }
    }
}
