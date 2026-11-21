using UnityEngine;
using UnityEngine.Events;

namespace TutorScripts
{

    public class SettingsPopupLogic : MonoBehaviour
    {
        public bool MusicEnabled = true;
        public bool SfxEnabled = true;

        public UnityEvent OnEnableMusic;
        public UnityEvent OnDisableMusic;
        public UnityEvent OnEnableSound;
        public UnityEvent OnDisableSound;

        public void EnableMusic()
        {
            MusicEnabled = true;
            OnEnableMusic?.Invoke();
        }

        public void EnableSound()
        {
            SfxEnabled = true;
            OnEnableSound?.Invoke();
        }

        public void DisableMusic()
        {
            MusicEnabled = false;
            OnDisableMusic?.Invoke();
        }

        public void DisableSound()
        {
            SfxEnabled = false;
            OnDisableSound?.Invoke();
        }

        public void ResetProgress()
        {
        }
    }
}