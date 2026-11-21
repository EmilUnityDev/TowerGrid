namespace Game.Scripts.GameRoot.Services.DataStorage
{
    public interface IDataStorage
    {
        void Set<T>(string key, T value);
        T Get<T>(string key, T defaultValue = default);
        bool HasKey(string key);
        void DeleteKey(string key);
    }
}