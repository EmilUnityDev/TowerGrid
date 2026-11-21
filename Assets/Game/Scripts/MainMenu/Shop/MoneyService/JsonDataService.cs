using System.IO;
using UnityEngine;

namespace Game.Scripts.GameRoot.Services.DataStorage.Json
{
    public sealed class JsonDataService : IJsonDataService
    {
        private const string FileName = "game_data.json";

        public string FilePath { get; }

        public JsonDataService()
        {
            FilePath = Path.Combine(Application.persistentDataPath, FileName);
            Debug.Log($"[JsonDataService] Save file path: {FilePath}");
        }

        public GameData Load()
        {
            if (!File.Exists(FilePath))
            {
                Debug.Log($"[JsonDataService] No save file found. Creating default at: {FilePath}");
                var defaultData = new GameData();
                Save(defaultData);
                return defaultData;
            }

            var json = File.ReadAllText(FilePath);
            var data = JsonUtility.FromJson<GameData>(json) ?? new GameData();
            Debug.Log($"[JsonDataService] Save file loaded from: {FilePath}\n{json}");
            return data;
        }

        public void Save(GameData data)
        {
            var json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(FilePath, json);
        }

        public void Delete()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }
}