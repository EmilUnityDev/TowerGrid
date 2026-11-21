using System.Collections;
using Game.Scripts.Gameplay.Building;
using Game.Scripts.Gameplay.Building.Data;
using Game.Scripts.Gameplay.Building.Grid;
using Game.Scripts.Gameplay.Root.View;
using Game.Scripts.GameRoot;
using Game.Scripts.GameRoot.Services.Overlay;
using Game.Scripts.GameRoot.Services.Popups;
using Game.Scripts.GameRoot.Services.Popups.Implementations.TutorialHintsPopup;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Scripts.Gameplay.Skills;

namespace Game.Scripts.Gameplay.Core.Services
{
    public class TutorialScenarioService
    {
        private readonly IPopupService _popupService;
        private readonly IBuildingGrid _grid;
        private readonly GridConfig _gridConfig;
        private readonly IBuildingFactory _buildingFactory;
        private readonly BuildingsParent _buildingsParent;
        private readonly BuildingCatalog _catalog;
        private readonly UIGameplayRootBinder _uiBinder;
        private readonly Coroutines _coroutines;
        private readonly GridBoundsProvider _boundsProvider;
        private readonly TutorialSceneRefs _sceneRefs;
        private readonly SkillService _skillService;
        private readonly IUIOverlayService _overlayService;
        private readonly UIRootView _uiRoot;

        private int _tutorialAllowedSlotIndex = -1;
        private int _tutorialAllowedLevel = -1;
        private BuildingData _tutorialAllowedData;

        private int _firstPlacedSlotIndex;
        private int _firstPlacedSlotWidth;

        private int _secondPlacedSlotIndex;

        private int _mediumLevelOneSlotIndex;
        private int _smallLevel1SlotIndex;
        private int _thirdPlacedSlotIndex;
        private int _fourthPlacedSlotIndex;
        private int _fifthPlacedSlotIndex;
        private int _sixthPlacedSlotIndex;
        private int _seventhPlacedSlotIndex;
        private int _eighthPlacedSlotIndex;

        public bool IsGameOver { get; private set; }

        public TutorialScenarioService(
            IPopupService popupService,
            IBuildingGrid grid,
            GridConfig gridConfig,
            IBuildingFactory buildingFactory,
            BuildingsParent buildingsParent,
            BuildingCatalog catalog,
            UIGameplayRootBinder uiBinder,
            Coroutines coroutines,
            GridBoundsProvider boundsProvider,
            TutorialSceneRefs sceneRefs,
            SkillService skillService,
            IUIOverlayService overlayService,
            UIRootView uiRoot)
        {
            _popupService = popupService;
            _grid = grid;
            _gridConfig = gridConfig;
            _buildingFactory = buildingFactory;
            _buildingsParent = buildingsParent;
            _catalog = catalog;
            _uiBinder = uiBinder;
            _coroutines = coroutines;
            _boundsProvider = boundsProvider;
            _sceneRefs = sceneRefs;
            _skillService = skillService;
            _overlayService = overlayService;
            _uiRoot = uiRoot;
        }

        public void StartScenario()
        {
            _coroutines.StartCoroutine(StartScenarioDelayed());
        }

        private IEnumerator StartScenarioDelayed()
        {
            yield return null;
            _uiBinder.SetSlotsInteractable(false);
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(0), OnHint0Closed);
        }

        private void OnHint0Closed()
        {
            _coroutines.StartCoroutine(AfterHint0Sequence());
        }

        private IEnumerator AfterHint0Sequence()
        {
            SpawnTutorialBuildings();
            _uiBinder.SetHintFieldActive(true);
            yield return new WaitForSeconds(1.4f);
            _uiBinder.SetHintFieldActive(false);
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(1), OnHint2Closed);
        }

        private void OnHint2Closed()
        {
            _uiBinder.SetHandFirstActive(true);
            _uiBinder.SetTopHintActive(true);
            _uiBinder.SetFtueInventoryButton(true, OnFtueInventoryPressed);
        }

        private void OnFtueInventoryPressed()
        {
            _uiBinder.SetFtueInventoryButton(false, null);
            _uiBinder.SetTopHintActive(false);
            _uiBinder.SetHandFirstActive(false);
            SetupTutorialSlots();
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(3), OnHint4Closed);
        }

        private void SetupTutorialSlots()
        {
            var largeService = FindBuildingInCatalog(BuildingType.Service, BuildingSize.Large);
            var smallLiving = FindBuildingInCatalog(BuildingType.Living, BuildingSize.Small);
            var mediumCommercial = FindBuildingInCatalog(BuildingType.Commercial, BuildingSize.Medium);
            _uiBinder.SetSlotBuilding(0, largeService);
            _uiBinder.SetSlotBuilding(1, smallLiving);
            _uiBinder.SetSlotBuilding(2, mediumCommercial);
        }

        private void OnHint4Closed()
        {
            _uiBinder.SetHandImage2Active(true);
            if (_sceneRefs != null && _sceneRefs.FieldHintBottom != null)
                _sceneRefs.FieldHintBottom.SetActive(true);
            _uiBinder.SetSlot3FirstHintActive(true);

            var mediumCommercial = FindBuildingInCatalog(BuildingType.Commercial, BuildingSize.Medium);
            _tutorialAllowedData = mediumCommercial;

            int totalSlots = _grid.TotalSlots;
            var largeLiving = FindBuildingInCatalog(BuildingType.Living, BuildingSize.Large);
            var smallLiving = FindBuildingInCatalog(BuildingType.Living, BuildingSize.Small);
            int largeSlot = totalSlots - largeLiving.SlotWidth;
            int smallSlot = largeSlot - smallLiving.SlotWidth;
            _tutorialAllowedSlotIndex = smallSlot - mediumCommercial.SlotWidth;
            _tutorialAllowedLevel = 0;

            _firstPlacedSlotIndex = _tutorialAllowedSlotIndex;
            _firstPlacedSlotWidth = mediumCommercial.SlotWidth;

            EnableTutorialPlacementRestriction();
            _uiBinder.SetSlotRefillOverride(2, () => FindBuildingInCatalog(BuildingType.Commercial, BuildingSize.Large));
            _uiBinder.SetSlotInteractable(2, true);
            _uiBinder.SubscribeSlotPlaced(2, OnFirstBuildingPlaced);
        }

        private void OnFirstBuildingPlaced()
        {
            _uiBinder.UnsubscribeSlotPlaced(2, OnFirstBuildingPlaced);
            _uiBinder.SetSlotInteractable(2, false);
            _uiBinder.SetHandImage2Active(false);
            if (_sceneRefs != null && _sceneRefs.FieldHintBottom != null)
                _sceneRefs.FieldHintBottom.SetActive(false);
            _uiBinder.SetSlot3FirstHintActive(false);
            DisableTutorialPlacementRestriction();
            AddTutorialCharge(1);
            _coroutines.StartCoroutine(AfterFirstBuildingPlacedSequence());
        }

        private IEnumerator AfterFirstBuildingPlacedSequence()
        {
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(5), OnHint6Closed);
            yield break;
        }

        private void OnHint6Closed()
        {
            var largeCommercial = FindBuildingInCatalog(BuildingType.Commercial, BuildingSize.Large);
            _tutorialAllowedData = largeCommercial;
            _tutorialAllowedSlotIndex = _firstPlacedSlotIndex - largeCommercial.SlotWidth;
            _tutorialAllowedLevel = 0;
            _secondPlacedSlotIndex = _tutorialAllowedSlotIndex;

            EnableTutorialPlacementRestriction();

            _uiBinder.SetSlot3SecondHintActive(true);
            _uiBinder.SetHandImage3Active(true);
            if (_sceneRefs != null && _sceneRefs.FieldSecondHintBottom != null)
                _sceneRefs.FieldSecondHintBottom.SetActive(true);

            _uiBinder.SetSlotRefillOverride(2, () => FindBuildingInCatalog(BuildingType.Living, BuildingSize.Large));
            _uiBinder.SetSlotInteractable(2, true);
            _uiBinder.SubscribeSlotPlaced(2, OnSecondBuildingPlaced);
        }

        private void OnSecondBuildingPlaced()
        {
            _uiBinder.UnsubscribeSlotPlaced(2, OnSecondBuildingPlaced);
            _uiBinder.SetSlotInteractable(2, false);
            _uiBinder.SetSlot3SecondHintActive(false);
            _uiBinder.SetHandImage3Active(false);
            if (_sceneRefs != null && _sceneRefs.FieldSecondHintBottom != null)
                _sceneRefs.FieldSecondHintBottom.SetActive(false);
            DisableTutorialPlacementRestriction();
            _uiBinder.SetSlotRefillOverride(2, null);
            AddTutorialCharge(1);
            _coroutines.StartCoroutine(AfterSecondBuildingPlacedSequence());
        }

        private IEnumerator AfterSecondBuildingPlacedSequence()
        {
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(7), OnHint7Closed);
            yield break;
        }

        private void OnHint7Closed()
        {
            if (_sceneRefs != null && _sceneRefs.Floor1Completed != null)
                _sceneRefs.Floor1Completed.SetActive(true);
            _coroutines.StartCoroutine(AfterFloor1CompletedSequence());
        }

        private IEnumerator AfterFloor1CompletedSequence()
        {
            yield return new WaitForSeconds(1.5f);
            if (_sceneRefs != null && _sceneRefs.Floor1Completed != null)
                _sceneRefs.Floor1Completed.SetActive(false);
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(8), OnHint9Closed);
        }

        private void OnHint9Closed()
        {
            if (_sceneRefs != null && _sceneRefs.RedHint != null)
                _sceneRefs.RedHint.SetActive(true);
            _coroutines.StartCoroutine(AfterRedHintSequence());
        }

        private IEnumerator AfterRedHintSequence()
        {
            yield return new WaitForSeconds(1.5f);
            if (_sceneRefs != null && _sceneRefs.RedHint != null)
                _sceneRefs.RedHint.SetActive(false);
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(10), OnHint11Closed);
        }

        private void OnHint11Closed()
        {
            var largeLiving = FindBuildingInCatalog(BuildingType.Living, BuildingSize.Large);
            _tutorialAllowedData = largeLiving;
            _tutorialAllowedSlotIndex = _smallLevel1SlotIndex - largeLiving.SlotWidth;
            _tutorialAllowedLevel = 1;
            _thirdPlacedSlotIndex = _tutorialAllowedSlotIndex;

            EnableTutorialPlacementRestriction();

            _uiBinder.SetHandImage4Active(true);
            _uiBinder.SetSlot3SecondHintActive(true);

            _uiBinder.SetSlotInteractable(2, true);
            _uiBinder.SubscribeSlotPlaced(2, OnThirdBuildingPlaced);
        }

        private void OnThirdBuildingPlaced()
        {
            _uiBinder.UnsubscribeSlotPlaced(2, OnThirdBuildingPlaced);
            _uiBinder.SetSlotInteractable(2, false);
            _uiBinder.SetHandImage4Active(false);
            _uiBinder.SetSlot3SecondHintActive(false);
            DisableTutorialPlacementRestriction();
            AddTutorialCharge(1);
            _coroutines.StartCoroutine(AfterThirdBuildingPlacedSequence());
        }

        private IEnumerator AfterThirdBuildingPlacedSequence()
        {
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(12), OnHint12Closed);
            yield break;
        }

        private void OnHint12Closed()
        {
            var smallLiving = FindBuildingInCatalog(BuildingType.Living, BuildingSize.Small);
            _tutorialAllowedData = smallLiving;
            _tutorialAllowedSlotIndex = _thirdPlacedSlotIndex - smallLiving.SlotWidth;
            _tutorialAllowedLevel = 1;
            _fourthPlacedSlotIndex = _tutorialAllowedSlotIndex;

            EnableTutorialPlacementRestriction();

            _uiBinder.SetHand5ImageActive(true);
            _uiBinder.SetSlotRefillOverride(1, () => FindBuildingInCatalog(BuildingType.Service, BuildingSize.Medium));
            _uiBinder.SetSlotInteractable(1, true);
            _uiBinder.SubscribeSlotPlaced(1, OnFourthBuildingPlaced);
        }

        private void OnFourthBuildingPlaced()
        {
            _uiBinder.UnsubscribeSlotPlaced(1, OnFourthBuildingPlaced);
            _uiBinder.SetSlotInteractable(1, false);
            _uiBinder.SetHand5ImageActive(false);
            DisableTutorialPlacementRestriction();
            AddTutorialCharge(1);
            _coroutines.StartCoroutine(AfterFourthBuildingPlacedSequence());
        }

        private IEnumerator AfterFourthBuildingPlacedSequence()
        {
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(13), OnHint14Closed);
            yield break;
        }

        private void OnHint14Closed()
        {
            var mediumService = FindBuildingInCatalog(BuildingType.Service, BuildingSize.Medium);
            _tutorialAllowedData = mediumService;
            _tutorialAllowedSlotIndex = _fourthPlacedSlotIndex - mediumService.SlotWidth;
            _tutorialAllowedLevel = 1;
            _fifthPlacedSlotIndex = _tutorialAllowedSlotIndex;

            EnableTutorialPlacementRestriction();

            _uiBinder.SetSlot2FirstHintActive(true);
            _uiBinder.SetHand6ImageActive(true);
            if (_sceneRefs != null && _sceneRefs.FieldThirdHintBottom != null)
                _sceneRefs.FieldThirdHintBottom.SetActive(true);

            _uiBinder.SetSlotRefillOverride(1, () => FindBuildingInCatalog(BuildingType.Service, BuildingSize.Small));
            _uiBinder.SetSlotInteractable(1, true);
            _uiBinder.SubscribeSlotPlaced(1, OnFifthBuildingPlaced);
        }

        private void OnFifthBuildingPlaced()
        {
            _uiBinder.UnsubscribeSlotPlaced(1, OnFifthBuildingPlaced);
            _uiBinder.SetSlotInteractable(1, false);
            _uiBinder.SetSlot2FirstHintActive(false);
            _uiBinder.SetHand6ImageActive(false);
            if (_sceneRefs != null && _sceneRefs.FieldThirdHintBottom != null)
                _sceneRefs.FieldThirdHintBottom.SetActive(false);
            DisableTutorialPlacementRestriction();
            AddTutorialCharge(1);
            _coroutines.StartCoroutine(AfterFifthBuildingPlacedSequence());
        }

        private IEnumerator AfterFifthBuildingPlacedSequence()
        {
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(15), OnHint15Closed);
            yield break;
        }

        private void OnHint15Closed()
        {
            var smallService = FindBuildingInCatalog(BuildingType.Service, BuildingSize.Small);
            _tutorialAllowedData = smallService;
            _tutorialAllowedSlotIndex = _fifthPlacedSlotIndex - smallService.SlotWidth;
            _tutorialAllowedLevel = 1;
            _sixthPlacedSlotIndex = _tutorialAllowedSlotIndex;

            EnableTutorialPlacementRestriction();

            _uiBinder.SetHand7ImageActive(true);
            _uiBinder.SetSlotRefillOverride(1, null);
            _uiBinder.SetSlotInteractable(1, true);
            _uiBinder.SubscribeSlotPlaced(1, OnSixthBuildingPlaced);
        }

        private void OnSixthBuildingPlaced()
        {
            _uiBinder.UnsubscribeSlotPlaced(1, OnSixthBuildingPlaced);
            _uiBinder.SetSlotInteractable(1, false);
            _uiBinder.SetHand7ImageActive(false);
            DisableTutorialPlacementRestriction();
            AddTutorialCharge(3);
            _coroutines.StartCoroutine(AfterSixthBuildingPlacedSequence());
        }

        private IEnumerator AfterSixthBuildingPlacedSequence()
        {
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(16), OnHint18Closed);
            yield break;
        }

        private void OnHint18Closed()
        {
            _uiBinder.SetSlotsInteractable(false);
            _uiBinder.SetDraftRefreshInteractable(true);
            _uiBinder.SetHandImage8Active(true);

            _uiBinder.SetFtueInventoryButton(true, null);
            _uiBinder.SetFtueInventoryButtonInteractable(false);

            _skillService.DraftRefresh.OnUsed += OnTutorialDraftRefreshUsed;
        }

        private void OnTutorialDraftRefreshUsed()
        {
            _skillService.DraftRefresh.OnUsed -= OnTutorialDraftRefreshUsed;
            _uiBinder.SetHandImage8Active(false);
            _uiBinder.SetDraftRefreshInteractable(false);
            _uiBinder.SetFtueInventoryButton(false, null);

            var largePremium = FindBuildingInCatalog(BuildingType.Premium, BuildingSize.Large);
            var mediumPremium = FindBuildingInCatalog(BuildingType.Premium, BuildingSize.Medium);
            _uiBinder.SetSlotBuilding(0, largePremium);
            _uiBinder.SetSlotBuilding(1, largePremium);
            _uiBinder.SetSlotBuilding(2, mediumPremium);

            _coroutines.StartCoroutine(AfterDraftRefreshSequence());
        }

        private IEnumerator AfterDraftRefreshSequence()
        {
            yield return new WaitForSeconds(1f);
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(19), OnHint20Closed);
        }

        private void OnHint20Closed()
        {
            _uiBinder.AnimateCameraToY(6f, 1.5f);
            _coroutines.StartCoroutine(AfterCameraMovedSequence());
        }

        private IEnumerator AfterCameraMovedSequence()
        {
            yield return new WaitForSeconds(0.8f);

            var largePremium = FindBuildingInCatalog(BuildingType.Premium, BuildingSize.Large);
            _tutorialAllowedData = largePremium;
            _tutorialAllowedSlotIndex = 0;
            _tutorialAllowedLevel = 2;
            _seventhPlacedSlotIndex = 0;

            EnableTutorialPlacementRestriction();

            _uiBinder.SetHand9ImageActive(true);
            _uiBinder.SetSlotInteractable(0, true);
            _uiBinder.SubscribeSlotPlaced(0, OnSeventhBuildingPlaced);
        }

        private void OnSeventhBuildingPlaced()
        {
            _uiBinder.UnsubscribeSlotPlaced(0, OnSeventhBuildingPlaced);
            _uiBinder.SetSlotInteractable(0, false);
            _uiBinder.SetHand9ImageActive(false);
            DisableTutorialPlacementRestriction();

            _coroutines.StartCoroutine(AfterSeventhBuildingPlacedSequence());
        }

        private IEnumerator AfterSeventhBuildingPlacedSequence()
        {
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(21), OnHint21Closed);
            yield break;
        }

        private void OnHint21Closed()
        {
            var largePremium = FindBuildingInCatalog(BuildingType.Premium, BuildingSize.Large);
            _tutorialAllowedData = largePremium;
            _tutorialAllowedSlotIndex = _seventhPlacedSlotIndex + largePremium.SlotWidth;
            _tutorialAllowedLevel = 2;
            _eighthPlacedSlotIndex = _tutorialAllowedSlotIndex;

            EnableTutorialPlacementRestriction();

            _uiBinder.SetHand10ImageActive(true);
            if (_sceneRefs != null && _sceneRefs.FieldFourthHintBottom != null)
                _sceneRefs.FieldFourthHintBottom.SetActive(true);
            _uiBinder.SetSlotInteractable(1, true);
            _uiBinder.SubscribeSlotPlaced(1, OnEighthBuildingPlaced);
        }

        private void OnEighthBuildingPlaced()
        {
            _uiBinder.UnsubscribeSlotPlaced(1, OnEighthBuildingPlaced);
            _uiBinder.SetSlotInteractable(1, false);
            _uiBinder.SetHand10ImageActive(false);
            if (_sceneRefs != null && _sceneRefs.FieldFourthHintBottom != null)
                _sceneRefs.FieldFourthHintBottom.SetActive(false);
            DisableTutorialPlacementRestriction();

            var mediumPremium = FindBuildingInCatalog(BuildingType.Premium, BuildingSize.Medium);
            _tutorialAllowedData = mediumPremium;
            _tutorialAllowedSlotIndex = _eighthPlacedSlotIndex + FindBuildingInCatalog(BuildingType.Premium, BuildingSize.Large).SlotWidth;
            _tutorialAllowedLevel = 2;

            EnableTutorialPlacementRestriction();

            _uiBinder.SetHand11ImageActive(true);
            _uiBinder.SetSlotInteractable(2, true);
            _uiBinder.SubscribeSlotPlaced(2, OnNinthBuildingPlaced);
        }

        private void OnNinthBuildingPlaced()
        {
            _uiBinder.UnsubscribeSlotPlaced(2, OnNinthBuildingPlaced);
            _uiBinder.SetSlotInteractable(2, false);
            _uiBinder.SetHand11ImageActive(false);
            DisableTutorialPlacementRestriction();

            _coroutines.StartCoroutine(AfterNinthBuildingPlacedSequence());
        }

        private IEnumerator AfterNinthBuildingPlacedSequence()
        {
            _popupService.ShowPopup<TutorialHintsPopupView, TutorialHintsPopupData>(
                new TutorialHintsPopupData(22));
            yield break;
        }

        private void EnableTutorialPlacementRestriction()
        {
            _grid.SetPlacementRestriction(_tutorialAllowedSlotIndex, _tutorialAllowedLevel, _tutorialAllowedData);
        }

        private void DisableTutorialPlacementRestriction()
        {
            _grid.ClearPlacementRestriction();
        }

        private void SpawnTutorialBuildings()
        {
            var largeLiving = FindBuildingInCatalog(BuildingType.Living, BuildingSize.Large);
            var smallLiving = FindBuildingInCatalog(BuildingType.Living, BuildingSize.Small);
            var mediumLiving = FindBuildingInCatalog(BuildingType.Living, BuildingSize.Medium);

            int totalSlots = _grid.TotalSlots;
            float gridMinX = _boundsProvider.MinX;
            float gridMaxX = _boundsProvider.MaxX;

            int largeSlot = totalSlots - largeLiving.SlotWidth;
            PlaceTutorialBuilding(largeLiving, largeSlot, 0, gridMinX, gridMaxX);

            int smallSlot = largeSlot - smallLiving.SlotWidth;
            PlaceTutorialBuilding(smallLiving, smallSlot, 0, gridMinX, gridMaxX);

            int mediumSlot = totalSlots - mediumLiving.SlotWidth;
            PlaceTutorialBuilding(mediumLiving, mediumSlot, 1, gridMinX, gridMaxX);
            _mediumLevelOneSlotIndex = mediumSlot;

            int smallLevel1Slot = mediumSlot - smallLiving.SlotWidth;
            PlaceTutorialBuilding(smallLiving, smallLevel1Slot, 1, gridMinX, gridMaxX);
            _smallLevel1SlotIndex = smallLevel1Slot;
        }

        private void PlaceTutorialBuilding(BuildingData data, int slotIndex, int level, float gridMinX, float gridMaxX)
        {
            if (!_grid.TryPlace(slotIndex, level, data))
            {
                Debug.LogWarning($"[Tutorial] Failed to place {data.name} at slot={slotIndex} level={level}");
                return;
            }

            var go = _buildingFactory.Create(data, _buildingsParent.transform);
            var building = go.GetComponent<BuildingInstance>();
            building.MarkPlaced();

            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = 2;

            var collider = go.GetComponent<BoxCollider2D>();
            float colliderWidth = collider != null
                ? collider.size.x * go.transform.lossyScale.x
                : data.SlotWidth * _gridConfig.SlotWidth;

            Vector2 snapPos = _grid.GetTightSnapPosition(
                slotIndex, level, data, colliderWidth, gridMinX, gridMaxX);

            go.transform.position = new Vector3(snapPos.x, snapPos.y, 0f);

            var lp = go.transform.localPosition;
            lp.y = _gridConfig.LevelYPositions[level];
            go.transform.localPosition = lp;
        }

        private BuildingData FindBuildingInCatalog(BuildingType type, BuildingSize size)
        {
            foreach (var b in _catalog.Buildings)
            {
                if (b.Type == type && b.Size == size)
                    return b;
            }
            foreach (var b in _catalog.Buildings)
            {
                if (b.Size == size)
                    return b;
            }
            return _catalog.GetRandom();
        }

        private void AddTutorialCharge(int segments)
        {
            _skillService.AddChargeForTutorial(segments);
        }

        public void EndTutorial()
        {
            IsGameOver = true;
            _overlayService.ShowLoadingWithTransition(null, OpenMainMenu);
            _popupService.CloseCurrentPopup();
        }

        public void SkipTutorial()
        {
            IsGameOver = true;
            _popupService.CloseCurrentPopup();
            _overlayService.ShowLoadingWithTransition(null, OpenMainMenu);
        }

        private void OpenMainMenu()
        {
            _uiRoot.ClearSceneUI();
            _popupService.CloseCurrentPopup();
            _overlayService.HideAllLoading();
            SceneManager.LoadScene(Scenes.MAIN_MENU);
        }
    }
}
