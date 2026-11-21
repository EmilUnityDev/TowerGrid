using System;
using DG.Tweening;
using Game.Scripts.Game.SlideshowGamePopup;
using Game.Scripts.GameRoot;
using Game.Scripts.GameRoot.Services.DataStorage.Interfaces;
using Game.Scripts.GameRoot.Services.Feedback;
using Game.Scripts.GameRoot.Services.Popups;
using Game.Scripts.GameRoot.Services.Scenes;
using Lofelt.NiceVibrations;
using TutorScripts;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Scripts.Services.Popups.SettingsPopup
{
    public class SettingsPopupView : MonoBehaviour, IPopup
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _howToPlayButton;

        [Header("Toggles")] [SerializeField] private Button _musicToggleButton;
        [SerializeField] private Button _soundToggleButton;
        [SerializeField] private Button _hapticToggleButton;

        [Header("Handles")] [SerializeField] private RectTransform _musicHandle;
        [SerializeField] private RectTransform _soundHandle;
        [SerializeField] private RectTransform _hapticHandle;

        [Header("Toggle Sprites")] [SerializeField]
        private Image _musicToggleImage;

        [SerializeField] private Image _soundToggleImage;
        [SerializeField] private Image _hapticToggleImage;
        [SerializeField] private Sprite _toggleOnSprite;
        [SerializeField] private Sprite _toggleOffSprite;

        [Header("Anim Settings")] [SerializeField]
        private float _toggleMoveDistance = 50f;

        [SerializeField] private float _toggleMoveDuration = 0.25f;
        [SerializeField] private Ease _toggleEase = Ease.OutBack;

        [Header("Popup Anim")] [SerializeField]
        private float _fadeDuration = 0.4f;

        [SerializeField] private float _scaleDuration = 0.45f;

        [SerializeField] private SettingsPopupLogic _settingsPopupLogic;

        [Header("Background")] [SerializeField]
        private Image _bgImage;
        [SerializeField] private Image _bgGameplayImage;

        public event Action OnClose;

        private bool _isMusicOn = true;
        private bool _isSoundOn = true;
        private bool _isHapticOn = true;

        private Vector2 _musicStartPos;
        private Vector2 _soundStartPos;
        private Vector2 _hapticStartPos;
        private IHapticsService _hapticsServiceOptional;

        private IFeedbackService _audioService;
        private IGameSettingsStorage _gameStateProvider;
        private IPopupService _popupService;
        private ISceneService _sceneService;

        [Inject]
        public void Construct(
            IFeedbackService audioService,
            IGameSettingsStorage gameStateProvider,
            IPopupService popupService,
            ISceneService sceneService)
        {
            _audioService = audioService;
            _gameStateProvider = gameStateProvider;
            _popupService = popupService;
            _sceneService = sceneService;

            if (_audioService != null)
            {
                Debug.Log("Audio Service initialized");
            }
        }

        private void Awake()
        {
            if (_musicHandle != null) _musicStartPos = _musicHandle.anchoredPosition;
            if (_soundHandle != null) _soundStartPos = _soundHandle.anchoredPosition;
            if (_hapticHandle != null) _hapticStartPos = _hapticHandle.anchoredPosition;
            _audioService?.PlaySfx("popupAppear");
        }

        private void Start()
        {
            bool isGameplay =
                _sceneService != null &&
                _sceneService.IsCurrentScene(
                    Scenes.GAMEPLAY);

            bool isGameplayOrTutorial =
                _sceneService != null &&
                (_sceneService.IsCurrentScene(
                     Scenes.GAMEPLAY) ||
                 _sceneService.IsCurrentScene(
                     Scenes.TUTORIAL));

            if (_backButton != null)
            {
                _backButton.gameObject.SetActive(
                    isGameplay);

                if (isGameplay)
                {
                    _backButton.onClick.AddListener(() =>
                    {
                        _audioService?.PlaySfx(
                            "ClickSound");

                        ClosePopup();
                    });
                }
            }

            if (_bgImage != null)
            {
                _bgImage.gameObject.SetActive(
                    !isGameplayOrTutorial);
            }

            if (_bgGameplayImage != null)
            {
                _bgGameplayImage.gameObject.SetActive(
                    isGameplayOrTutorial);
            }

            if (_gameStateProvider == null)
                return;

            _isMusicOn =
                _gameStateProvider.IsMusicOn;

            _isSoundOn =
                _gameStateProvider.IsSoundOn;

            _isHapticOn =
                _gameStateProvider.IsHapticOn;

            HapticController.hapticsEnabled =
                _isHapticOn;

            if (_settingsPopupLogic != null)
            {
                _settingsPopupLogic.MusicEnabled =
                    _isMusicOn;

                _settingsPopupLogic.SfxEnabled =
                    _isSoundOn;
            }

            if (_hapticsServiceOptional != null)
            {
                _hapticsServiceOptional.SetEnabled(
                    _isHapticOn);
            }

            SetHandleInstant(
                _musicHandle,
                _musicStartPos,
                _isMusicOn);

            SetHandleInstant(
                _soundHandle,
                _soundStartPos,
                _isSoundOn);

            SetHandleInstant(
                _hapticHandle,
                _hapticStartPos,
                _isHapticOn);

            SetToggleSprite(
                _musicToggleImage,
                _isMusicOn);

            SetToggleSprite(
                _soundToggleImage,
                _isSoundOn);

            SetToggleSprite(
                _hapticToggleImage,
                _isHapticOn);

            if (_howToPlayButton != null)
            {
                _howToPlayButton.gameObject.SetActive(
                    !isGameplayOrTutorial);

                if (!isGameplayOrTutorial)
                {
                    _howToPlayButton.onClick.AddListener(() =>
                    {
                        _audioService?.PlaySfx(
                            "ClickSound");

                        ShowSlideshow();
                    });
                }
            }

            if (_musicToggleButton != null)
            {
                _musicToggleButton.onClick.AddListener(
                    ToggleMusic);
            }

            if (_soundToggleButton != null)
            {
                _soundToggleButton.onClick.AddListener(
                    ToggleSound);
            }

            if (_hapticToggleButton != null)
            {
                _hapticToggleButton.onClick.AddListener(
                    ToggleHaptic);
            }
        }

        public void Bind(IPopupData data = null)
        {
            PopupAnimationHandler.PlaySpawnAnimation(gameObject);
        }

        public void ClosePopup()
        {
            PopupAnimationHandler.PlayCloseAnimation(gameObject, () =>
            {
                OnClose?.Invoke();
                Destroy(gameObject);
            });
        }

        public void ShowSlideshow()
        {
            PopupAnimationHandler.PlayCloseAnimation(gameObject, () =>
            {
                OnClose?.Invoke();
                _popupService.ShowPopup<SlideshowGamePopupView>();
                Destroy(gameObject);
            });
        }

        private void ToggleMusic()
        {
            _audioService?.PlaySfx("ClickSound");
            _isMusicOn = !_isMusicOn;

            AnimateHandle(_musicHandle, _musicStartPos, _isMusicOn);
            SetToggleSprite(_musicToggleImage, _isMusicOn);

            if (_isMusicOn)
            {
                _settingsPopupLogic?.EnableMusic();
                _gameStateProvider.SetMusicState(true);
            }
            else
            {
                _settingsPopupLogic?.DisableMusic();
                _gameStateProvider.SetMusicState(false);
            }

            if (_settingsPopupLogic != null)
                _settingsPopupLogic.MusicEnabled = _isMusicOn;
        }

        private void ToggleSound()
        {
            _audioService?.PlaySfx("ClickSound");
            _isSoundOn = !_isSoundOn;

            AnimateHandle(_soundHandle, _soundStartPos, _isSoundOn);
            SetToggleSprite(_soundToggleImage, _isSoundOn);

            if (_isSoundOn)
            {
                _settingsPopupLogic?.EnableSound();
                _gameStateProvider.SetSoundState(true);
            }
            else
            {
                _settingsPopupLogic?.DisableSound();
                _gameStateProvider.SetSoundState(false);
            }

            if (_settingsPopupLogic != null)
                _settingsPopupLogic.SfxEnabled = _isSoundOn;
        }

        private void ToggleHaptic()
        {
            _audioService?.PlaySfx("ClickSound");
            _isHapticOn = !_isHapticOn;

            AnimateHandle(_hapticHandle, _hapticStartPos, _isHapticOn);
            SetToggleSprite(_hapticToggleImage, _isHapticOn);

            _gameStateProvider.SetHapticState(_isHapticOn);
            HapticController.hapticsEnabled = _isHapticOn;

            if (_hapticsServiceOptional != null)
                _hapticsServiceOptional.SetEnabled(_isHapticOn);
        }

        private void SetHandleInstant(RectTransform handle, Vector2 startPos, bool isOn)
        {
            if (handle == null) return;
            var x = isOn ? startPos.x + _toggleMoveDistance : startPos.x;
            handle.anchoredPosition = new Vector2(x, startPos.y);
        }

        private void AnimateHandle(RectTransform handle, Vector2 startPos, bool isOn)
        {
            if (handle == null) return;
            var targetX = isOn ? startPos.x + _toggleMoveDistance : startPos.x;
            handle.DOAnchorPosX(targetX, _toggleMoveDuration).SetEase(_toggleEase);
        }

        private void SetToggleSprite(Image image, bool isOn)
        {
            if (image == null) return;
            if (_toggleOnSprite == null || _toggleOffSprite == null) return;
            image.sprite = isOn ? _toggleOnSprite : _toggleOffSprite;
        }
    }

    public interface IHapticsService
    {
        void SetEnabled(bool enabled);
        bool IsEnabled { get; }
    }
}