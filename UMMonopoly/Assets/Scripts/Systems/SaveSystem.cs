using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UMMonopoly.Entities;

namespace UMMonopoly.Systems
{
    [System.Serializable]
    public class PlayerSaveData
    {
        public int id;
        public string name;
        public int money;
        public int position;
        public bool inJail;
        public int jailTurnsRemaining;
        public bool isBankrupt;
        public int getOutOfJailCards;
        public List<int> ownedPropertyPositions = new List<int>();
        public List<int> propertyUpgradeLevels = new List<int>();
    }

    [System.Serializable]
    public class GameSaveData
    {
        public string version = "1.0";
        public int currentPlayerIndex;
        public int turnNumber;
        public List<PlayerSaveData> players = new List<PlayerSaveData>();
    }

    public static class SaveSystem
    {
        private const string FileName = "savegame.json";

        public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public static void Save(GameSaveData data)
        {
            var json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
        }

        public static GameSaveData Load()
        {
            if (!File.Exists(SavePath)) return null;
            var json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<GameSaveData>(json);
        }

        public static bool HasSave() => File.Exists(SavePath);

        public static void DeleteSave()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }

        public static GameSaveData Snapshot(List<Player> players, int currentIdx, int turn, Board board)
        {
            var data = new GameSaveData
            {
                currentPlayerIndex = currentIdx,
                turnNumber = turn
            };
            foreach (var p in players)
            {
                var ps = new PlayerSaveData
                {
                    id = p.Id,
                    name = p.Name,
                    money = p.Money,
                    position = p.BoardPosition,
                    inJail = p.InJail,
                    jailTurnsRemaining = p.JailTurnsRemaining,
                    isBankrupt = p.IsBankrupt,
                    getOutOfJailCards = p.GetOutOfJailCards
                };
                foreach (var prop in p.OwnedProperties)
                {
                    ps.ownedPropertyPositions.Add(prop.Position);
                    ps.propertyUpgradeLevels.Add(prop.UpgradeLevel);
                }
                data.players.Add(ps);
            }
            return data;
        }
    }
}
