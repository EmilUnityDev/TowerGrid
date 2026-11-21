using Game.Scripts.GameRoot.Services.DataStorage.Json;

namespace Game.Scripts.GameRoot.Services.DataStorage
{
    public sealed class GameDataRepository
    {
        private readonly IJsonDataService _jsonDataService;
        private GameData _data;

        public GameDataRepository(IJsonDataService jsonDataService)
        {
            _jsonDataService = jsonDataService;
            _data = _jsonDataService.Load();
        }

        public GameData Data => _data;

        public void Save()
        {
            _jsonDataService.Save(_data);
        }

        public void Reset()
        {
            _jsonDataService.Delete();
            _data = new GameData();
        }
    }
}