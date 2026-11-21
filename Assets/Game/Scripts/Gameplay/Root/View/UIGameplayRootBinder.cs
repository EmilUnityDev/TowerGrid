using Game.Scripts.Gameplay.Building;
using Game.Scripts.Gameplay.Building.Data;
using Game.Scripts.Gameplay.Building.Grid;
using Game.Scripts.Gameplay.Core.Services;
using Game.Scripts.Gameplay.Skills;
using Game.Scripts.Gameplay.UI;
using Game.Scripts.GameRoot.Services.Feedback;
using Game.Scripts.GameRoot.Services.Popups;
using Game.Scripts.Services.Popups.SettingsPopup;
using Game.Scripts.Services.Popups.SkillsPopup;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using DG.Tweening;

namespace Game.Scripts.Gameplay.Root.View
{
    public class UIGameplayRootBinder : MonoBehaviour
    {
        private IFeedbackService _audioService;

        [Header("Buttons")]
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _skipTutorialButton;
        [SerializeField] private Button _dailyButton;

        [Space]
        [SerializeField] private TimerComponent _timer;

        [Header("Building Slots")]
        [SerializeField] private BuildingSlotUI _buildingSlot1;
        [SerializeField] private BuildingSlotUI _buildingSlot2;
        [SerializeField] private BuildingSlotUI _buildingSlot3;

        [Header("Skills")]
        [SerializeField] private SkillButtonUI _draftRefreshButton;
        [SerializeField] private SkillButtonUI _rezoneButton;
        [SerializeField] private SkillButtonUI _demolitionButton;

        [Header("Charge Bar (8 segments)")]
        [SerializeField] private GameObject[] _chargeSegments =
            new GameObject[8];

        [Header("Camera Input")]
        [SerializeField] private CameraInputView _cameraInputView;

        [Header("Score Bonuses")]
        [SerializeField] private ScoreBonusSpawner _scoreBonusSpawner;

        [Header("UI Frames")]
        [SerializeField] private RectTransform _timerFrame;
        [SerializeField] private RectTransform _totalScoreFrame;

        [Header("Tutorial FTUE Elements")]
        [SerializeField] private GameObject _ftueRoot;
        [SerializeField] private GameObject _hintFieldImage;
        [SerializeField] private GameObject _handFirstImage;
        [SerializeField] private GameObject _topHintImage;
        [SerializeField] private Button _ftueInventoryButton;
        [SerializeField] private GameObject _handImage2;
        [SerializeField] private GameObject _handImage3;
        [SerializeField] private GameObject _handImage4;
        [SerializeField] private GameObject _hand5Image;
        [SerializeField] private GameObject _hand6Image;
        [SerializeField] private GameObject _hand7Image;
        [SerializeField] private GameObject _handImage8;
        [SerializeField] private GameObject _hand9Image;
        [SerializeField] private GameObject _hand10Image;
        [SerializeField] private GameObject _hand11Image;
        [SerializeField] private GameObject _slot3FirstHint;
        [SerializeField] private GameObject _slot3SecondHint;
        [SerializeField] private GameObject _slot2FirstHint;

        private IPopupService _popupService;
        private IBuildingSpawnService _spawnService;
        private BuildingCatalog _catalog;
        private Camera _uiCamera;
        private BuildingSlotUI[] _slots;
        private ICameraControlService _cameraControlService;
        private SkillService _skillService;
        private IBuildingGrid _grid;
        private GridConfig _gridConfig;
        private DemolishBlockRoot _demolishBlock;
        private GameOverCheckService _gameOverCheckService;
        private TutorialGameService _tutorialGameService;
        private DailyQuestManager _dailyQuestManager;
        private IObjectResolver _resolver;

        private bool _isDraftRefreshActive;
        private bool _isRezoneActive;
        private bool _isDemolitionActive;
        private bool _draftRefreshLockedByTutorial;

        private Action _ftueInventoryCallback;

        private readonly Func<BuildingData>[] _slotRefillOverrides =
            new Func<BuildingData>[3];

        [Inject]
        public void Construct(
            IFeedbackService audioService,
            IPopupService popupService,
            IBuildingSpawnService spawnService,
            BuildingCatalog catalog,
            Camera uiCamera,
            ICameraControlService cameraControlService,
            SkillService skillService,
            IBuildingGrid grid,
            GridConfig gridConfig,
            DemolishBlockRoot demolishBlock,
            GameOverCheckService gameOverCheckService,
            TutorialGameService tutorialGameService,
            DailyQuestManager dailyQuestManager,
            IObjectResolver resolver)
        {
            _audioService = audioService;
            _popupService = popupService;
            _spawnService = spawnService;
            _catalog = catalog;
            _uiCamera = uiCamera;
            _cameraControlService = cameraControlService;
            _skillService = skillService;
            _grid = grid;
            _gridConfig = gridConfig;
            _demolishBlock = demolishBlock;
            _gameOverCheckService = gameOverCheckService;
            _tutorialGameService = tutorialGameService;
            _dailyQuestManager = dailyQuestManager;
            _resolver = resolver;
        }

        public void InitializeBinder()
        {
            if (_tutorialGameService.IsTutorialGame)
            {
                _ftueRoot.SetActive(true);
                _demolitionButton.gameObject.SetActive(false);
                _rezoneButton.gameObject.SetActive(false);
                _draftRefreshButton.gameObject.SetActive(true);
                _draftRefreshButton.Button.interactable = false;
                _draftRefreshLockedByTutorial = true;

                if (_timerFrame != null)
                    _timerFrame.gameObject.SetActive(false);

                if (_exitButton != null)
                    _exitButton.gameObject.SetActive(false);

                if (_settingsButton != null)
                    _settingsButton.gameObject.SetActive(false);

                if (_skipTutorialButton != null)
                {
                    _skipTutorialButton.gameObject.SetActive(
                        _tutorialGameService.IsTutorialGame);

                    if (_tutorialGameService.IsTutorialGame)
                    {
                        _skipTutorialButton.onClick.AddListener(
                            OnSkipTutorialPressed);
                    }
                }

                if (_dailyButton != null)
                    _dailyButton.gameObject.SetActive(false);

                if (_totalScoreFrame != null)
                {
                    Vector2 position =
                        _totalScoreFrame.anchoredPosition;

                    position.x = 0f;

                    _totalScoreFrame.anchoredPosition =
                        position;
                }
            }
            else
            {
                _ftueRoot.SetActive(false);

                if (_skipTutorialButton != null)
                {
                    _skipTutorialButton.gameObject.SetActive(
                        false);
                }
            }

            if (_hintFieldImage != null)
                _hintFieldImage.SetActive(false);

            if (_handFirstImage != null)
                _handFirstImage.SetActive(false);

            if (_topHintImage != null)
                _topHintImage.SetActive(false);

            if (_ftueInventoryButton != null)
                _ftueInventoryButton.gameObject.SetActive(false);

            if (_handImage2 != null)
                _handImage2.SetActive(false);

            if (_handImage3 != null)
                _handImage3.SetActive(false);

            if (_handImage4 != null)
                _handImage4.SetActive(false);

            if (_hand5Image != null)
                _hand5Image.SetActive(false);

            if (_hand6Image != null)
                _hand6Image.SetActive(false);

            if (_hand7Image != null)
                _hand7Image.SetActive(false);

            if (_handImage8 != null)
                _handImage8.SetActive(false);

            if (_hand9Image != null)
                _hand9Image.SetActive(false);

            if (_hand10Image != null)
                _hand10Image.SetActive(false);

            if (_hand11Image != null)
                _hand11Image.SetActive(false);

            if (_slot3FirstHint != null)
                _slot3FirstHint.SetActive(false);

            if (_slot3SecondHint != null)
                _slot3SecondHint.SetActive(false);

            if (_slot2FirstHint != null)
                _slot2FirstHint.SetActive(false);

            _exitButton.onClick.AddListener(
                OpenAbortPopup);

            _settingsButton.onClick.AddListener(
                OpenSettingsPopup);

            _slots = new[]
            {
                _buildingSlot1,
                _buildingSlot2,
                _buildingSlot3
            };

            _gameOverCheckService.SetSlotWidthsProvider(
                () =>
                    _slots
                        .Select(
                            slot =>
                                slot.CurrentData != null
                                    ? slot.CurrentData.SlotWidth
                                    : 0)
                        .ToArray());

            foreach (BuildingSlotUI slot in _slots)
            {
                slot.Initialize(
                    _spawnService,
                    () => RefillSlot(slot),
                    _uiCamera);

                slot.SetBuilding(
                    _catalog.GetRandom());

                slot.SetInterceptHandler(
                    OnSlotIntercepted);
            }

            if (_cameraInputView != null)
            {
                _cameraInputView.Initialize(
                    _cameraControlService);

                if (_tutorialGameService.IsTutorialGame)
                {
                    _cameraInputView.SetEnabled(
                        false);
                }
            }

            if (_demolishBlock != null)
            {
                _demolishBlock.Initialize(
                    _grid,
                    _gridConfig,
                    OnDemolishComplete);

                _demolishBlock.Hide();
            }

            _draftRefreshButton.Button.onClick.AddListener(
                OnDraftRefreshPressed);

            _rezoneButton.Button.onClick.AddListener(
                OnRezonePressed);

            _demolitionButton.Button.onClick.AddListener(
                OnDemolitionPressed);

            _skillService.OnChargeChanged +=
                UpdateChargeBar;

            _skillService.DraftRefresh.OnStateChanged +=
                UpdateSkillButtons;

            _skillService.DraftRefresh.OnUsed +=
                RefreshAllSlots;

            _skillService.Rezone.OnStateChanged +=
                UpdateSkillButtons;

            _skillService.Demolition.OnStateChanged +=
                UpdateSkillButtons;

            UpdateChargeBar();
            UpdateSkillButtons();
        }

        public void SetHintFieldActive(
            bool active)
        {
            if (_hintFieldImage != null)
                _hintFieldImage.SetActive(active);
        }

        public void SetHandFirstActive(
            bool active)
        {
            if (_handFirstImage != null)
                _handFirstImage.SetActive(active);
        }

        public void SetTopHintActive(
            bool active)
        {
            if (_topHintImage != null)
                _topHintImage.SetActive(active);
        }

        public void SetFtueInventoryButton(
            bool active,
            Action onClick)
        {
            if (_ftueInventoryButton == null)
                return;

            if (_ftueInventoryCallback != null)
            {
                _ftueInventoryButton.onClick.RemoveListener(
                    OnFtueInventoryClicked);

                _ftueInventoryCallback = null;
            }

            _ftueInventoryButton.gameObject.SetActive(
                active);

            if (active && onClick != null)
            {
                _ftueInventoryCallback = onClick;

                _ftueInventoryButton.onClick.AddListener(
                    OnFtueInventoryClicked);
            }
        }

        private void OnFtueInventoryClicked()
        {
            Action callback =
                _ftueInventoryCallback;

            SetFtueInventoryButton(
                false,
                null);

            callback?.Invoke();
        }

        public void SetHandImage2Active(
            bool active)
        {
            if (_handImage2 != null)
                _handImage2.SetActive(active);
        }

        public void SetHandImage3Active(
            bool active)
        {
            if (_handImage3 != null)
                _handImage3.SetActive(active);
        }

        public void SetHandImage4Active(
            bool active)
        {
            if (_handImage4 != null)
                _handImage4.SetActive(active);
        }

        public void SetHand5ImageActive(
            bool active)
        {
            if (_hand5Image != null)
                _hand5Image.SetActive(active);
        }

        public void SetHand6ImageActive(
            bool active)
        {
            if (_hand6Image != null)
                _hand6Image.SetActive(active);
        }

        public void SetHand7ImageActive(
            bool active)
        {
            if (_hand7Image != null)
                _hand7Image.SetActive(active);
        }

        public void SetHandImage8Active(
            bool active)
        {
            if (_handImage8 != null)
                _handImage8.SetActive(active);
        }

        public void SetHand9ImageActive(
            bool active)
        {
            if (_hand9Image != null)
                _hand9Image.SetActive(active);
        }

        public void SetHand10ImageActive(
            bool active)
        {
            if (_hand10Image != null)
                _hand10Image.SetActive(active);
        }

        public void SetHand11ImageActive(
            bool active)
        {
            if (_hand11Image != null)
                _hand11Image.SetActive(active);
        }

        public void SetSlot3FirstHintActive(
            bool active)
        {
            if (_slot3FirstHint != null)
                _slot3FirstHint.SetActive(active);
        }

        public void SetSlot3SecondHintActive(
            bool active)
        {
            if (_slot3SecondHint != null)
                _slot3SecondHint.SetActive(active);
        }

        public void SetSlot2FirstHintActive(
            bool active)
        {
            if (_slot2FirstHint != null)
                _slot2FirstHint.SetActive(active);
        }

        public void SetDraftRefreshInteractable(
            bool interactable)
        {
            _draftRefreshLockedByTutorial =
                !interactable;

            _draftRefreshButton.Button.interactable =
                interactable;
        }

        public void SetFtueInventoryButtonInteractable(
            bool interactable)
        {
            if (_ftueInventoryButton == null)
                return;

            _ftueInventoryButton.interactable =
                interactable;

            _ftueInventoryButton.image.raycastTarget =
                interactable;
        }

        public void SetCameraInputEnabled(
            bool enabled)
        {
            if (_cameraInputView != null)
            {
                _cameraInputView.SetEnabled(
                    enabled);
            }
        }

        public void AnimateCameraToY(
            float targetY,
            float duration)
        {
            Camera camera =
                Camera.main;

            if (camera == null)
                return;

            camera.transform
                .DOMoveY(
                    targetY,
                    duration)
                .SetEase(
                    Ease.InOutCubic);
        }

        public void SetSlotsInteractable(
            bool interactable)
        {
            if (_slots == null)
                return;

            foreach (BuildingSlotUI slot in _slots)
            {
                slot.SetInteractable(
                    interactable);
            }
        }

        public void SetSlotInteractable(
            int slotIndex,
            bool interactable)
        {
            if (_slots == null ||
                slotIndex < 0 ||
                slotIndex >= _slots.Length)
            {
                return;
            }

            _slots[slotIndex].SetInteractable(
                interactable);
        }

        public void SetSlotBuilding(
            int slotIndex,
            BuildingData data)
        {
            if (_slots == null ||
                slotIndex < 0 ||
                slotIndex >= _slots.Length)
            {
                return;
            }

            _slots[slotIndex].SetBuilding(
                data);
        }

        public void SubscribeSlotPlaced(
            int slotIndex,
            Action onPlaced)
        {
            if (_slots == null ||
                slotIndex < 0 ||
                slotIndex >= _slots.Length)
            {
                return;
            }

            _slots[slotIndex].OnBuildingPlaced +=
                onPlaced;
        }

        public void UnsubscribeSlotPlaced(
            int slotIndex,
            Action onPlaced)
        {
            if (_slots == null ||
                slotIndex < 0 ||
                slotIndex >= _slots.Length)
            {
                return;
            }

            _slots[slotIndex].OnBuildingPlaced -=
                onPlaced;
        }

        private void RefillSlot(
            BuildingSlotUI slot)
        {
            int index =
                Array.IndexOf(
                    _slots,
                    slot);

            if (index >= 0 &&
                index < _slotRefillOverrides.Length &&
                _slotRefillOverrides[index] != null)
            {
                BuildingData data =
                    _slotRefillOverrides[index]();

                slot.SetBuilding(
                    data);
            }
            else
            {
                slot.SetBuilding(
                    _catalog.GetRandom());
            }

            _gameOverCheckService.Check();
        }

        private void OnDraftRefreshPressed()
        {
            if (!_skillService.DraftRefresh.CanUse)
                return;

            _isDraftRefreshActive = true;
            UpdateSkillButtons();

            _skillService.DraftRefresh.Use();

            _dailyQuestManager?.RegisterSkillUsed(
                DailySkillType.DraftRefresh);

            _isDraftRefreshActive = false;
            UpdateSkillButtons();
        }

        private void OnRezonePressed()
        {
            var rezone =
                _skillService.Rezone;

            if (rezone.IsActive)
            {
                rezone.Cancel();
                _isRezoneActive = false;
            }
            else if (rezone.CanUse)
            {
                rezone.Use();
                _isRezoneActive = true;
            }

            UpdateSkillButtons();
        }

        private void OnDemolitionPressed()
        {
            var demolition =
                _skillService.Demolition;

            if (demolition.IsActive)
            {
                demolition.Cancel();
                _isDemolitionActive = false;

                _demolishBlock?.Hide();

                if (_cameraInputView != null)
                {
                    _cameraInputView.SetEnabled(
                        true);
                }
            }
            else if (demolition.CanUse)
            {
                demolition.Use();
                _isDemolitionActive = true;

                _demolishBlock?.Show();

                if (_cameraInputView != null)
                {
                    _cameraInputView.SetEnabled(
                        false);
                }
            }

            UpdateSkillButtons();
        }

        private void OnDemolishComplete()
        {
            _skillService.Demolition.ConfirmUse();

            _dailyQuestManager?.RegisterSkillUsed(
                DailySkillType.Demolition);

            _isDemolitionActive = false;

            _demolishBlock?.Hide();

            if (_cameraInputView != null)
            {
                _cameraInputView.SetEnabled(
                    true);
            }

            UpdateSkillButtons();
            _gameOverCheckService.Check();
        }

        private void UpdateChargeBar()
        {
            int current =
                _skillService.CurrentCharge;

            for (int i = 0;
                 i < _chargeSegments.Length;
                 i++)
            {
                if (_chargeSegments[i] != null)
                {
                    _chargeSegments[i].SetActive(
                        i < current);
                }
            }
        }

        private void UpdateSkillButtons()
        {
            _draftRefreshButton.UpdateState(
                _skillService.DraftRefresh,
                _isDraftRefreshActive);

            if (_draftRefreshLockedByTutorial)
            {
                _draftRefreshButton.Button.interactable =
                    false;
            }

            _rezoneButton.UpdateState(
                _skillService.Rezone,
                _isRezoneActive ||
                _skillService.Rezone.IsActive);

            _demolitionButton.UpdateState(
                _skillService.Demolition,
                _isDemolitionActive ||
                _skillService.Demolition.IsActive);
        }

        private void OpenAbortPopup()
        {
            _popupService.ShowPopup<
                AbortPopupView>();
        }

        private void OpenSettingsPopup()
        {
            _popupService.ShowPopup<
                SettingsPopupView>();
        }

        private void OnDestroy()
        {
            _exitButton.onClick.RemoveListener(
                OpenAbortPopup);

            _settingsButton.onClick.RemoveListener(
                OpenSettingsPopup);

            _draftRefreshButton.Button.onClick.RemoveListener(
                OnDraftRefreshPressed);

            _rezoneButton.Button.onClick.RemoveListener(
                OnRezonePressed);

            _demolitionButton.Button.onClick.RemoveListener(
                OnDemolitionPressed);

            if (_skillService != null)
            {
                _skillService.OnChargeChanged -=
                    UpdateChargeBar;

                _skillService.DraftRefresh.OnStateChanged -=
                    UpdateSkillButtons;

                _skillService.DraftRefresh.OnUsed -=
                    RefreshAllSlots;

                _skillService.Rezone.OnStateChanged -=
                    UpdateSkillButtons;

                _skillService.Demolition.OnStateChanged -=
                    UpdateSkillButtons;
            }

            if (_skipTutorialButton != null)
            {
                _skipTutorialButton.onClick.RemoveListener(
                    OnSkipTutorialPressed);
            }
        }

        private bool OnSlotIntercepted(
            BuildingSlotUI slot)
        {
            if (_skillService.Demolition.IsActive)
                return true;

            if (_skillService.Rezone.IsActive &&
                slot.CurrentData != null)
            {
                BuildingData currentData =
                    slot.CurrentData;

                BuildingData newData =
                    _catalog.GetRandomWithSizeExcludingType(
                        currentData.Size,
                        currentData.Type);

                slot.SetBuilding(
                    newData);

                _skillService.Rezone.ConfirmUse();

                _dailyQuestManager?.RegisterSkillUsed(
                    DailySkillType.Rezone);

                _isRezoneActive = false;

                UpdateSkillButtons();

                return true;
            }

            return false;
        }

        public void RefreshAllSlots()
        {
            foreach (BuildingSlotUI slot in _slots)
            {
                slot.SetBuilding(
                    _catalog.GetRandom());
            }

            _gameOverCheckService.Check();
        }

        public void SetSlotRefillOverride(
            int slotIndex,
            Func<BuildingData> overrideProvider)
        {
            if (slotIndex < 0 ||
                slotIndex >= _slotRefillOverrides.Length)
            {
                return;
            }

            _slotRefillOverrides[slotIndex] =
                overrideProvider;
        }

        private void OnSkipTutorialPressed()
        {
            _resolver
                ?.Resolve<TutorialScenarioService>()
                ?.SkipTutorial();
        }
    }
}