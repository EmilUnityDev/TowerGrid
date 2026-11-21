using Lofelt.NiceVibrations;
using UnityEngine;

namespace Game.Scripts.GameRoot.Services.Feedback
{
    public interface IFeedbackService
    {
        void PlayMusic(string key, bool loop = true);
        void PlayMusic(AudioClip clip, bool loop = true);
        void StopMusic();
        void PlayFeedback(AudioClip clip, HapticPatterns.PresetType presetType);
        void PlayFeedback(string key, HapticPatterns.PresetType presetType);
        void PlaySfx(string key);
        void PlaySfx(AudioClip clip, float pitch = 1f);
        void PlayLoopingSfx(string key, float pitch = 1f);
        void StopAllLoopingSfx();
        void PlayHaptic(HapticPatterns.PresetType presetType);
    }
}