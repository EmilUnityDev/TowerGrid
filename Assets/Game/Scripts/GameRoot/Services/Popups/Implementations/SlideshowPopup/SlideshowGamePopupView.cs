using System;
using Game.Scripts.GameRoot;
using Game.Scripts.GameRoot.Services.Overlay;
using Game.Scripts.GameRoot.Services.Popups;
using Game.Scripts.GameRoot.Services.Scenes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Scripts.Game.SlideshowGamePopup
{
    public class SlideshowGamePopupView : MonoBehaviour, IPopup
    {

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private TextMeshProUGUI _skipButtonText;
        [SerializeField] private RectTransform _navigationContainer;
        [SerializeField] private UiSlideSwitcher _slideSwitcher;

        public event Action OnClose;

        private ISceneService _sceneService;
        private IUIOverlayService _overlayService;
        private Coroutines _coroutines;

        private void EnsureServices()
        {
           
        }

        [Inject]
        public void Construct(
            IUIOverlayService overlayService,
            ISceneService sceneService,
            Coroutines coroutines)
        {
            _overlayService = overlayService;
            _sceneService = sceneService;
            _coroutines = coroutines;
        }

        public void Bind(IPopupData data)
        {
            EnsureServices();


            PopupAnimationHandler.PlaySpawnAnimation(gameObject);

            bool isMainMenu =
                _sceneService != null &&
                _sceneService.IsCurrentScene(Scenes.MAIN_MENU);

            if (isMainMenu)
            {
                SetupMainMenuView();
            }
            else
            {
                SetupGameplayView();
            }

            _skipButton.onClick.RemoveAllListeners();
            _playButton.onClick.RemoveAllListeners();

            _skipButton.onClick.AddListener(OnSkipButtonClicked);
            _playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private void SetupMainMenuView()
        {
            _playButton.gameObject.SetActive(false);

            _skipButton.gameObject.SetActive(false);
            _skipButton.image.color = Color.white;

            RectTransform skipButtonRect =
                _skipButton.GetComponent<RectTransform>();

            if (skipButtonRect != null)
            {
                skipButtonRect.anchorMin = new Vector2(0f, 1f);
                skipButtonRect.anchorMax = new Vector2(0f, 1f);
                skipButtonRect.pivot = new Vector2(0.5f, 0.5f);
                skipButtonRect.anchoredPosition = new Vector2(193f, -175f);
            }

            if (_skipButtonText != null)
                _skipButtonText.text = string.Empty;

            if (_navigationContainer != null)
            {
                Vector2 pos = _navigationContainer.anchoredPosition;
                pos.y = -785f;
                _navigationContainer.anchoredPosition = pos;
            }
        }

        private void SetupGameplayView()
        {
            if (_navigationContainer != null)
            {
                Vector2 pos = _navigationContainer.anchoredPosition;
                pos.y = -991f;
                _navigationContainer.anchoredPosition = pos;
            }

            if (_slideSwitcher != null)
            {
                _slideSwitcher.OnSlideChanged += OnSlideChanged;
            }
        }

        private void OnSlideChanged(int currentIndex, int totalCount)
        {
            bool isMainMenu = _sceneService != null && _sceneService.IsCurrentScene(Scenes.MAIN_MENU);
            if (!isMainMenu && currentIndex >= totalCount - 1)
            {
                if (_navigationContainer != null)
                    _navigationContainer.gameObject.SetActive(false);
            }
        }

        private void OnSkipButtonClicked()
        {

            PlayClickSound();
            ClosePopup();
        }

        private void OnPlayButtonClicked()
        {
            PlayClickSound();
            ClosePopup();
        }

        private void PlayClickSound()
        {
        }

        private void ClaimPrize()
        {
        }

        public void ClosePopup()
        {
            PopupAnimationHandler.PlayCloseAnimation(gameObject, () =>
            {
                OnClose?.Invoke();

                EnsureServices();

                bool isMainMenu =
                    _sceneService != null &&
                    _sceneService.IsCurrentScene(Scenes.MAIN_MENU);

                // if (!isMainMenu)
                // {
                //     _overlayService?.ShowLoadingWithTransition();
                //     
                // }

                Destroy(gameObject);
            });
        }
    }
}