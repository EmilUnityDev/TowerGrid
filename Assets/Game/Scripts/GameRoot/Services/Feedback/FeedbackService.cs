using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.GameRoot.Services.DataStorage.Interfaces;
using Lofelt.NiceVibrations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Scripts.GameRoot.Services.Feedback
{
    public class FeedbackService : IFeedbackService, IDisposable
    {
        private const float DEFAULT_MAX_VOLUME = .5f;
        private const float DISABLED_VOLUME = 0;
        private const int DEFAULT_POOL_SIZE = 8;
        private const float MIN_SAME_CLIP_INTERVAL = 0.06f;

        private readonly AudioSource _musicSrc;
        private readonly IReadOnlyList<AudioSource> _sfxPool;
        private readonly Dictionary<string, float> _lastPlayed = new();
        private readonly IGameSettingsStorage _gameSettings;
        private readonly List<AudioSource> _loopingSfxSources = new();
        
        private GameObject _gameObject;

        public FeedbackService(IGameSettingsStorage gameSettings)
        {
            _gameSettings = gameSettings;
            _gameObject = new GameObject("FeedbackService");
            Object.DontDestroyOnLoad(_gameObject);
            _musicSrc = _gameObject.AddComponent<AudioSource>();
            _musicSrc.loop = true;
            _musicSrc.playOnAwake = false;
            _musicSrc.volume = _gameSettings.IsMusicOn ? DEFAULT_MAX_VOLUME : DISABLED_VOLUME;
            var sfxVolume = _gameSettings.IsSoundOn ? DEFAULT_MAX_VOLUME : DISABLED_VOLUME;
            var sfxPool = new List<AudioSource>(DEFAULT_POOL_SIZE);
            for (var i = 0; i < DEFAULT_POOL_SIZE; i++)
            {
                var audioSource = _gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.volume = sfxVolume;
                sfxPool.Add(audioSource);
            }

            _sfxPool = sfxPool;
            _gameSettings.OnMusicStateChanged += OnMusicStateChanged;
            _gameSettings.OnSoundStateChanged += OnSoundStateChanged;
        }
        
        public void PlayMusic(string key, bool loop = true)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var clip = Resources.Load<AudioClip>($"Music/{key}");
            
            if (!clip)
            {
                Debug.LogWarning($"[FeedbackService] Music clip not found for key: '{key}'");
                return;
            }

            PlayMusic(clip, loop);
        }
        
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (!clip)
            {
                Debug.LogWarning("[FeedbackService] Music clip is null.");
                return;
            }

            _musicSrc.clip = clip;
            _musicSrc.loop = loop;
            _musicSrc.Play();
        }
        
        public void StopMusic()
        {
            _musicSrc.Stop();
        }

        public void PlayFeedback(string key, HapticPatterns.PresetType presetType)
        {
            PlaySfx(key);
            PlayHaptic(presetType);
        }
        
        public void PlayFeedback(AudioClip clip, HapticPatterns.PresetType presetType)
        {
            PlaySfx(clip);
            PlayHaptic(presetType);
        }

        public void PlaySfx(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var clip = Resources.Load<AudioClip>($"SFX/{key}");
            if (!clip)
            {
                Debug.LogWarning($"[FeedbackService] AudioClip not found for key: '{key}'");
                return;
            }

            PlaySfx(clip);
        }

        public void PlaySfx(AudioClip clip, float pitch = 1)
        {
            if (!_gameSettings.IsSoundOn)
            {
                return;
            }

            if (!clip)
            {
                Debug.LogWarning("[FeedbackService] AudioClip is null.");
                return;
            }

            var now = Time.realtimeSinceStartup;
            if (_lastPlayed.TryGetValue(clip.name, out var last) && now - last < MIN_SAME_CLIP_INTERVAL)
            {
                return;
            }

            _lastPlayed[clip.name] = now;
            
            var freeSource = _sfxPool.FirstOrDefault(s => !s.isPlaying);

            if (!freeSource)
            {
                freeSource = _sfxPool[0];
                freeSource.Stop();
            }

            freeSource.clip = clip;
            freeSource.pitch = pitch;
            freeSource.loop = false;
            freeSource.Play();
        }

        public void PlayLoopingSfx(string key, float pitch = 1)
        {
            if (!_gameSettings.IsSoundOn || string.IsNullOrEmpty(key)) return;

            var clip = Resources.Load<AudioClip>($"SFX/{key}");
            if (!clip) return;
            
            var freeSource = _sfxPool.FirstOrDefault(s => !s.isPlaying);
            
            if (!freeSource)
            {
                freeSource = _sfxPool[0];
                freeSource.Stop();
            }

            freeSource.clip = clip;
            freeSource.loop = true;
            freeSource.pitch = pitch;
            freeSource.Play();

            _loopingSfxSources.Add(freeSource);
        }

        public void StopAllLoopingSfx()
        {
            foreach (var audioSource in _loopingSfxSources)
            {
                if (!audioSource.loop)
                {
                    continue;
                }

                audioSource.loop = false;
                audioSource.Stop();
            }
            
            _loopingSfxSources.Clear();
        }

        public void PlayHaptic(HapticPatterns.PresetType presetType)
        {
            if (_gameSettings.IsHapticOn)
            {
                HapticPatterns.PlayPreset(presetType);
            }
        }

        private void OnSoundStateChanged(bool state)
        {
            var volume = state ? DEFAULT_MAX_VOLUME : DISABLED_VOLUME;
            foreach (var audioSource in _sfxPool)
            {
                audioSource.volume = volume;
            }
        }

        private void OnMusicStateChanged(bool state)
        {
            _musicSrc.volume = state ? DEFAULT_MAX_VOLUME : DISABLED_VOLUME;
        }

        public void Dispose()
        {
            _gameSettings.OnMusicStateChanged -= OnMusicStateChanged;
            _gameSettings.OnSoundStateChanged -= OnSoundStateChanged;

            if (_gameObject)
            {
                Object.Destroy(_gameObject);
                _gameObject = null;
            }
        }
    }
}