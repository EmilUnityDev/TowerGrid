using System;

namespace Game.Scripts.GameRoot.Services.DataStorage.Interfaces
{
    public interface IGameSettingsStorage
    {
        bool IsSoundOn { get; }
        bool IsMusicOn { get; }
        bool IsHapticOn { get; }

        event Action<bool> OnSoundStateChanged;
        event Action<bool> OnMusicStateChanged;
        event Action<bool> OnHapticStateChanged;

        void SetSoundState(bool isOn);
        void SetMusicState(bool isOn);
        void SetHapticState(bool isOn);
    }
}