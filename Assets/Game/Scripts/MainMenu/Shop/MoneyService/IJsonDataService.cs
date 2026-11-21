namespace Game.Scripts.GameRoot.Services.DataStorage.Json
{
    public interface IJsonDataService
    {
        GameData Load();
        void Save(GameData data);
        void Delete();
        string FilePath { get; }
    }
}