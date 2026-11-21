using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Game.SlideshowGamePopup;
using Game.Scripts.GameRoot;
using Game.Scripts.GameRoot.Services.Analytics;
using Game.Scripts.GameRoot.Services.Feedback;
using Game.Scripts.GameRoot.Services.Overlay;
using Game.Scripts.GameRoot.Services.Popups;
using Game.Scripts.Services.Popups.SettingsPopup;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VContainer;

namespace Game.Scripts.MainMenu.Root.View
{
    public class UIMainMenuRootBinder : MonoBehaviour
    {
        [Header("Main Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _slideshowButton;
        [SerializeField] private Button _battlePassButton;
        [SerializeField] private Button _dailyTaskButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _homeButton;

        [Header("Navigation Panel")]
        [SerializeField] private GameObject _buttonsPanel;

        [Header("Shop Appearance")]
        [SerializeField] private Image _playerBackgroundImage;
        [SerializeField] private Image _playerFrameImage;

        [Header("Child Image Animation Settings")]
        [SerializeField] private float _animationDuration = 0.2f;
        [SerializeField] private float _activePosY = 10f;
        [SerializeField] private float _inactivePosY = 0f;
        [SerializeField] private float _activeHeight = 120f;
        [SerializeField] private float _inactiveHeight = 100f;

        private IFeedbackService _audioService;
        private IPopupService _popupService;
        private IUIOverlayService _overlayService;
        private UIRootView _uiRoot;
        private Coroutines _coroutines;
        private IAnalyticEventService _analyticEventService;
        private ShopAppearanceService _shopAppearanceService;

        private Transform _buttonsPanelOriginalParent;
        private int _buttonsPanelOriginalSiblingIndex;

        private CanvasGroup _homeCG;
        private CanvasGroup _settingsCG;
        private CanvasGroup _slideshowCG;
        private CanvasGroup _shopCG;

        private RectTransform _homeChildRT;
        private RectTransform _settingsChildRT;
        private RectTransform _slideshowChildRT;
        private RectTransform _shopChildRT;

        private readonly Dictionary<CanvasGroup, Coroutine>
            _runningAnimations = new();

        private enum ActivePopup
        {
            None,
            Settings,
            Slideshow,
            Shop
        }

        private ActivePopup _currentActive =
            ActivePopup.None;

        [Inject]
        public void Construct(
            IFeedbackService audioService,
            IPopupService popupService,
            IUIOverlayService overlayService,
            UIRootView uiRoot,
            Coroutines coroutines,
            IAnalyticEventService analyticEventService,
            ShopAppearanceService shopAppearanceService)
        {
            _audioService = audioService;
            _popupService = popupService;
            _overlayService = overlayService;
            _uiRoot = uiRoot;
            _coroutines = coroutines;
            _analyticEventService = analyticEventService;
            _shopAppearanceService = shopAppearanceService;
        }

        private void Start()
        {
            InitializeButtonsPanel();
            InitializeNavCanvasGroups();
            InitializeButtons();
            ApplyShopAppearance();

            ApplyNavButtonsState(
                ActivePopup.None,
                true);
        }

        private void InitializeButtonsPanel()
        {
            if (_buttonsPanel == null)
                return;

            _buttonsPanelOriginalParent =
                _buttonsPanel.transform.parent;

            _buttonsPanelOriginalSiblingIndex =
                _buttonsPanel.transform.GetSiblingIndex();
        }

        private void InitializeNavCanvasGroups()
        {
            if (_homeButton != null)
            {
                _homeCG =
                    _homeButton.GetComponentInChildren<
                        CanvasGroup>(true);

                if (_homeCG != null)
                {
                    _homeChildRT =
                        _homeCG.GetComponent<
                            RectTransform>();
                }
            }

            if (_settingsButton != null)
            {
                _settingsCG =
                    _settingsButton.GetComponentInChildren<
                        CanvasGroup>(true);

                if (_settingsCG != null)
                {
                    _settingsChildRT =
                        _settingsCG.GetComponent<
                            RectTransform>();
                }
            }

            if (_slideshowButton != null)
            {
                _slideshowCG =
                    _slideshowButton.GetComponentInChildren<
                        CanvasGroup>(true);

                if (_slideshowCG != null)
                {
                    _slideshowChildRT =
                        _slideshowCG.GetComponent<
                            RectTransform>();
                }
            }

            if (_shopButton != null)
            {
                _shopCG =
                    _shopButton.GetComponentInChildren<
                        CanvasGroup>(true);

                if (_shopCG != null)
                {
                    _shopChildRT =
                        _shopCG.GetComponent<
                            RectTransform>();
                }
            }
        }

        private void InitializeButtons()
        {
            _playButton?.onClick.AddListener(
                StartGame);

            _homeButton?.onClick.AddListener(
                GoHome);

            _settingsButton?.onClick.AddListener(
                OpenSettings);

            _slideshowButton?.onClick.AddListener(
                OpenSlideshow);

            _shopButton?.onClick.AddListener(
                OpenShop);

            _battlePassButton?.onClick.AddListener(
                OpenBattlePass);

            _dailyTaskButton?.onClick.AddListener(
                OpenDailyTask);
        }

        private void ApplyShopAppearance()
        {
            if (_shopAppearanceService == null)
            {
                Debug.LogWarning(
                    "[UIMainMenuRootBinder] ShopAppearanceService не внедрён.",
                    this);

                return;
            }

            ApplyImageSprite(
                _playerBackgroundImage,
                _shopAppearanceService.GetSelectedPlayerBackgroundSprite());
            _shopAppearanceService.GetSelectedBackgroundItemId();
            _playerBackgroundImage.SetNativeSize();

            ApplyImageSprite(
                _playerFrameImage,
                _shopAppearanceService.GetSelectedPlayerFrameSprite());
            _playerFrameImage.SetNativeSize();
        }

        private void ApplyImageSprite(
            Image image,
            Sprite sprite)
        {
            if (image == null ||
                sprite == null)
            {
                return;
            }

            image.sprite = sprite;
            image.enabled = true;
        }

        private void RefreshNavButtons(
            ActivePopup active)
        {
            ApplyNavButtonsState(
                active,
                false);
        }

        private void ApplyNavButtonsState(
            ActivePopup active,
            bool instant)
        {
            _currentActive = active;

            bool isHome =
                active == ActivePopup.None;

            SetChildButtonState(
                _homeCG,
                _homeChildRT,
                isHome,
                instant);

            SetChildButtonState(
                _settingsCG,
                _settingsChildRT,
                active == ActivePopup.Settings,
                instant);

            SetChildButtonState(
                _slideshowCG,
                _slideshowChildRT,
                active == ActivePopup.Slideshow,
                instant);

            SetChildButtonState(
                _shopCG,
                _shopChildRT,
                active == ActivePopup.Shop,
                instant);

            if (_homeButton != null)
            {
                _homeButton.interactable =
                    !isHome;
            }

            if (_settingsButton != null)
            {
                _settingsButton.interactable =
                    active != ActivePopup.Settings;
            }

            if (_slideshowButton != null)
            {
                _slideshowButton.interactable =
                    active != ActivePopup.Slideshow;
            }

            if (_shopButton != null)
            {
                _shopButton.interactable =
                    active != ActivePopup.Shop;
            }
        }

        private void SetChildButtonState(
            CanvasGroup canvasGroup,
            RectTransform childRectTransform,
            bool isActive,
            bool instant)
        {
            if (canvasGroup == null ||
                childRectTransform == null)
            {
                return;
            }

            if (_runningAnimations.TryGetValue(
                    canvasGroup,
                    out Coroutine runningCoroutine) &&
                runningCoroutine != null)
            {
                StopCoroutine(
                    runningCoroutine);

                _runningAnimations.Remove(
                    canvasGroup);
            }

            float targetAlpha =
                isActive ? 1f : 0f;

            float targetPosY =
                isActive
                    ? _activePosY
                    : _inactivePosY;

            float targetHeight =
                isActive
                    ? _activeHeight
                    : _inactiveHeight;

            canvasGroup.blocksRaycasts =
                isActive;

            canvasGroup.interactable =
                isActive;

            if (instant)
            {
                canvasGroup.alpha =
                    targetAlpha;

                Vector2 position =
                    childRectTransform
                        .anchoredPosition;

                position.y =
                    targetPosY;

                childRectTransform
                    .anchoredPosition =
                    position;

                Vector2 size =
                    childRectTransform
                        .sizeDelta;

                size.y =
                    targetHeight;

                childRectTransform.sizeDelta =
                    size;

                return;
            }

            Coroutine coroutine =
                StartCoroutine(
                    AnimateChildRoutine(
                        canvasGroup,
                        childRectTransform,
                        targetAlpha,
                        targetPosY,
                        targetHeight));

            _runningAnimations[canvasGroup] =
                coroutine;
        }

        private IEnumerator AnimateChildRoutine(
            CanvasGroup canvasGroup,
            RectTransform childRectTransform,
            float targetAlpha,
            float targetPosY,
            float targetHeight)
        {
            float startAlpha =
                canvasGroup.alpha;

            float startPosY =
                childRectTransform
                    .anchoredPosition.y;

            float startHeight =
                childRectTransform
                    .sizeDelta.y;

            float elapsed = 0f;

            while (elapsed < _animationDuration)
            {
                elapsed += Time.deltaTime;

                float t =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        elapsed /
                        _animationDuration);

                canvasGroup.alpha =
                    Mathf.Lerp(
                        startAlpha,
                        targetAlpha,
                        t);

                Vector2 position =
                    childRectTransform
                        .anchoredPosition;

                position.y =
                    Mathf.Lerp(
                        startPosY,
                        targetPosY,
                        t);

                childRectTransform
                    .anchoredPosition =
                    position;

                Vector2 size =
                    childRectTransform
                        .sizeDelta;

                size.y =
                    Mathf.Lerp(
                        startHeight,
                        targetHeight,
                        t);

                childRectTransform.sizeDelta =
                    size;

                yield return null;
            }

            canvasGroup.alpha =
                targetAlpha;

            Vector2 finalPosition =
                childRectTransform
                    .anchoredPosition;

            finalPosition.y =
                targetPosY;

            childRectTransform
                .anchoredPosition =
                finalPosition;

            Vector2 finalSize =
                childRectTransform
                    .sizeDelta;

            finalSize.y =
                targetHeight;

            childRectTransform.sizeDelta =
                finalSize;

            _runningAnimations.Remove(
                canvasGroup);
        }

        private void GoHome()
        {
            PlayClickSound();

            _currentActive =
                ActivePopup.None;

            _popupService?.CloseCurrentPopup();

            ReturnPanelHome();
            ApplyShopAppearance();

            RefreshNavButtons(
                ActivePopup.None);
        }

        private void MovePanelToPopupContainer()
        {
            if (_buttonsPanel == null ||
                _uiRoot == null ||
                _uiRoot.NavPanelContainer == null)
            {
                return;
            }

            _buttonsPanel.transform.SetParent(
                _uiRoot.NavPanelContainer,
                false);
        }

        private void ReturnPanelHome()
        {
            if (_buttonsPanel == null ||
                _buttonsPanelOriginalParent == null)
            {
                return;
            }

            _buttonsPanel.transform.SetParent(
                _buttonsPanelOriginalParent,
                false);

            _buttonsPanel.transform.SetSiblingIndex(
                _buttonsPanelOriginalSiblingIndex);
        }

        private void OpenSettings()
        {
            PlayClickSound();

            if (_currentActive !=
                ActivePopup.None)
            {
                _popupService?.CloseCurrentPopup();
            }

            RefreshNavButtons(
                ActivePopup.Settings);

            _popupService?.ShowPopup<
                SettingsPopupView>(
                onPopupClosed: () =>
                {
                    if (_currentActive !=
                        ActivePopup.Settings)
                    {
                        return;
                    }

                    ReturnPanelHome();

                    RefreshNavButtons(
                        ActivePopup.None);
                });

            MovePanelToPopupContainer();
        }

        private void OpenSlideshow()
        {
            PlayClickSound();

            if (_currentActive !=
                ActivePopup.None)
            {
                _popupService?.CloseCurrentPopup();
            }

            RefreshNavButtons(
                ActivePopup.Slideshow);

            _popupService?.ShowPopup<
                SlideshowGamePopupView>(
                onPopupClosed: () =>
                {
                    if (_currentActive !=
                        ActivePopup.Slideshow)
                    {
                        return;
                    }

                    ReturnPanelHome();

                    RefreshNavButtons(
                        ActivePopup.None);
                });

            MovePanelToPopupContainer();
        }

        private void OpenShop()
        {
            PlayClickSound();

            if (_currentActive !=
                ActivePopup.None)
            {
                _popupService?.CloseCurrentPopup();
            }

            RefreshNavButtons(
                ActivePopup.Shop);

            _popupService?.ShowPopup<
                ShopPopupView>(
                onPopupClosed: () =>
                {
                    ApplyShopAppearance();

                    if (_currentActive !=
                        ActivePopup.Shop)
                    {
                        return;
                    }

                    ReturnPanelHome();

                    RefreshNavButtons(
                        ActivePopup.None);
                });

            MovePanelToPopupContainer();
        }

        private void OpenBattlePass()
        {
            PlayClickSound();

            _popupService?.ShowPopup<
                BattlePassPopupView>();
        }

        private void OpenDailyTask()
        {
            PlayClickSound();

            _popupService?.ShowPopup<
                DailyTaskPopupView>();
        }

        public void StartGame()
        {
            if (_playButton != null)
            {
                _playButton.interactable =
                    false;
            }

            if (_settingsButton != null)
            {
                _settingsButton.interactable =
                    false;
            }

            PlayClickSound();

            _overlayService?.ShowLoading();

            if (_coroutines == null)
            {
                Debug.LogError(
                    "[UIMainMenuRootBinder] " +
                    "Coroutines не внедрён.",
                    this);

                return;
            }

            _coroutines.StartCoroutine(
                StartLocalGame());
        }

        private IEnumerator StartLocalGame()
        {
            yield return new WaitForSeconds(
                0.3f);

            _uiRoot?.ClearSceneUI();

            SceneManager.LoadScene(Scenes.GAMEPLAY);
        }

        private void PlayClickSound()
        {
            _audioService?.PlaySfx(
                "ClickSound");
        }

        private void OnDestroy()
        {
            foreach (Coroutine coroutine in
                     _runningAnimations.Values)
            {
                if (coroutine != null)
                {
                    StopCoroutine(
                        coroutine);
                }
            }

            _runningAnimations.Clear();

            _playButton?.onClick.RemoveListener(
                StartGame);

            _homeButton?.onClick.RemoveListener(
                GoHome);

            _settingsButton?.onClick.RemoveListener(
                OpenSettings);

            _slideshowButton?.onClick.RemoveListener(
                OpenSlideshow);

            _shopButton?.onClick.RemoveListener(
                OpenShop);

            _battlePassButton?.onClick.RemoveListener(
                OpenBattlePass);

            _dailyTaskButton?.onClick.RemoveListener(
                OpenDailyTask);
        }
    }
}