using System;
using Game.Scripts.GameRoot.Services.DataStorage.Interfaces;

namespace Game.Scripts.GameRoot.Services.DataStorage.Implementations
{
    public class GameSettingsStorage : IGameSettingsStorage
    {
        private const string SoundKey = "GameSettings_Sound";
        private const string MusicKey = "GameSettings_Music";
        private const string HapticKey = "GameSettings_Haptic";
        private readonly IDataStorage _dataStorage;

        public GameSettingsStorage(IDataStorage dataStorage)
        {
            _dataStorage = dataStorage;
            IsMusicOn = _dataStorage.Get(MusicKey, true);
            IsSoundOn = _dataStorage.Get(SoundKey, true);
            IsHapticOn = _dataStorage.Get(HapticKey, true);
        }

        public bool IsSoundOn { get; private set; }
        public bool IsMusicOn { get; private set; }
        public bool IsHapticOn { get; private set; }

        public event Action<bool> OnSoundStateChanged;
        public event Action<bool> OnMusicStateChanged;
        public event Action<bool> OnHapticStateChanged;

        public void SetSoundState(bool isOn)
        {
            if (IsSoundOn == isOn) return;
            IsSoundOn = isOn;
            _dataStorage.Set(SoundKey, isOn);
            OnSoundStateChanged?.Invoke(isOn);
        }

        public void SetMusicState(bool isOn)
        {
            if (IsMusicOn == isOn) return;
            IsMusicOn = isOn;
            _dataStorage.Set(MusicKey, isOn);
            OnMusicStateChanged?.Invoke(isOn);
        }

        public void SetHapticState(bool isOn)
        {
            if (IsHapticOn == isOn) return;
            IsHapticOn = isOn;
            _dataStorage.Set(HapticKey, isOn);
            OnHapticStateChanged?.Invoke(isOn);
        }
    }
}
