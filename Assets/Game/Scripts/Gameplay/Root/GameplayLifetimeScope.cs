using Game.Scripts.Gameplay.Building;
using Game.Scripts.Gameplay.Building.Data;
using Game.Scripts.Gameplay.Building.Grid;
using Game.Scripts.Gameplay.Core.Services;
using Game.Scripts.Gameplay.Core.StateMachine;
using Game.Scripts.Gameplay.Root.View;
using Game.Scripts.Gameplay.Skills;
using Game.Scripts.Gameplay.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.Gameplay.Root
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private UIGameplayRootBinder _sceneUIRootPrefab;
        [SerializeField] private Camera _uiCamera;

        [Header("Building System")]
        [SerializeField] private GridConfig _gridConfig;

        [Header("Building Skins")]
        [SerializeField] private ShopBuildingCatalogData[] _buildingCatalogs;

        [SerializeField] private BuildingsParent _buildingsParent;
        [SerializeField] private BuildingPrefabsConfig _buildingPrefabsConfig;
        [SerializeField] private GridBoundsProvider _gridBoundsProvider;

        [Header("Demolish")]
        [SerializeField] private DemolishBlockRoot _demolishBlock;

        [Header("Tutorial Scene Elements")]
        [SerializeField] private TutorialSceneRefs _tutorialSceneRefs;

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[GameplayLifetimeScope] Configure called!");
            
            if (_sceneUIRootPrefab != null)
            {
                UIGameplayRootBinder uiSceneInstance =
                    Instantiate(_sceneUIRootPrefab);

                builder.RegisterComponent(uiSceneInstance);

                TimerComponent timer =
                    uiSceneInstance.GetComponentInChildren<TimerComponent>(true);

                if (timer != null)
                    builder.RegisterComponent(timer);

                ScoreBonusSpawner bonusSpawner =
                    uiSceneInstance.GetComponentInChildren<ScoreBonusSpawner>(true);

                if (bonusSpawner != null)
                    builder.RegisterComponent(bonusSpawner);
            }

            builder.RegisterInstance(_uiCamera);

            builder.RegisterComponentInHierarchy<ShopBackgroundView>();
            builder.RegisterComponentInHierarchy<GameplayStateMachineRunner>();
            builder.RegisterEntryPoint<GameplaySceneEntryPoint>();

            _gridConfig.InitializeFromBounds(
                _gridBoundsProvider.WorldBounds);

            BuildingCatalog selectedCatalog =
                GetSelectedBuildingCatalog();

            if (selectedCatalog == null)
            {
                Debug.LogError(
                    "[GameplayLifetimeScope] Не удалось найти BuildingCatalog " +
                    "для выбранного скина блоков.",
                    this);

                return;
            }

            builder.RegisterInstance(_gridConfig);
            builder.RegisterInstance(selectedCatalog);
            builder.RegisterInstance(_buildingPrefabsConfig);
            builder.RegisterComponent(_gridBoundsProvider);

            builder.Register<BuildingGrid>(Lifetime.Singleton)
                .As<IBuildingGrid>();

            builder.Register<BuildingFactory>(Lifetime.Singleton)
                .As<IBuildingFactory>();

            if (_buildingsParent != null)
            {
                builder.RegisterComponent(_buildingsParent);
            }
            else
            {
                BuildingsParent buildingsParent =
                    new GameObject("[Buildings]")
                        .AddComponent<BuildingsParent>();

                builder.RegisterComponent(buildingsParent);
            }

            builder.Register<BuildingSpawnService>(Lifetime.Singleton)
                .As<IBuildingSpawnService>();

            builder.Register<CameraControlService>(Lifetime.Singleton)
                .WithParameter("sensitivity", 0.01f)
                .As<ICameraControlService>();

            builder.Register<DraftRefreshSkill>(Lifetime.Singleton);
            builder.Register<RezoneSkill>(Lifetime.Singleton);
            builder.Register<DemolitionSkill>(Lifetime.Singleton);
            builder.Register<SkillService>(Lifetime.Singleton);

            builder.Register<TutorialGameService>(Lifetime.Singleton);
            builder.Register<TutorialScenarioService>(Lifetime.Singleton);
            builder.Register<MatchRewardService>(Lifetime.Singleton);
            builder.Register<GameplayStateMachine>(Lifetime.Singleton);
            builder.Register<GameOverCheckService>(Lifetime.Singleton);

            if (_demolishBlock != null)
                builder.RegisterComponent(_demolishBlock);

            if (_tutorialSceneRefs != null)
                builder.RegisterComponent(_tutorialSceneRefs);
        }

        private BuildingCatalog GetSelectedBuildingCatalog()
        {
            if (_buildingCatalogs == null ||
                _buildingCatalogs.Length == 0)
            {
                Debug.LogError(
                    "[GameplayLifetimeScope] Список BuildingCatalog пуст.",
                    this);

                return null;
            }

            ShopProgressService progressService =
                new ShopProgressService();

            string selectedItemId =
                progressService.GetSelectedItemId(
                    ShopCategory.Blocks);

            if (!string.IsNullOrWhiteSpace(selectedItemId))
            {
                foreach (ShopBuildingCatalogData data in _buildingCatalogs)
                {
                    if (data == null)
                        continue;

                    if (data.ItemId != selectedItemId)
                        continue;

                    if (data.BuildingCatalog == null)
                    {
                        Debug.LogError(
                            $"[GameplayLifetimeScope] Для предмета " +
                            $"{selectedItemId} не назначен BuildingCatalog.",
                            this);

                        break;
                    }

                    Debug.Log(
                        $"[GameplayLifetimeScope] Применён скин блоков: " +
                        $"{selectedItemId}",
                        this);

                    return data.BuildingCatalog;
                }
            }

            return GetDefaultBuildingCatalog(progressService);
        }

        private BuildingCatalog GetDefaultBuildingCatalog(
            ShopProgressService progressService)
        {
            foreach (ShopBuildingCatalogData data in _buildingCatalogs)
            {
                if (data == null)
                    continue;

                if (!data.IsDefault)
                    continue;

                if (data.BuildingCatalog == null)
                    continue;

                progressService.SelectItem(
                    ShopCategory.Blocks,
                    data.ItemId);

                Debug.Log(
                    $"[GameplayLifetimeScope] Применён базовый скин блоков: " +
                    $"{data.ItemId}",
                    this);

                return data.BuildingCatalog;
            }

            foreach (ShopBuildingCatalogData data in _buildingCatalogs)
            {
                if (data?.BuildingCatalog == null)
                    continue;

                progressService.SelectItem(
                    ShopCategory.Blocks,
                    data.ItemId);

                Debug.LogWarning(
                    "[GameplayLifetimeScope] Базовый каталог не отмечен. " +
                    $"Используется первый каталог: {data.ItemId}",
                    this);

                return data.BuildingCatalog;
            }

            return null;
        }
    }
}