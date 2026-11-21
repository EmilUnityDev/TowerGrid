using Game.Scripts.Gameplay.Core.Services;
using Game.Scripts.GameRoot.Services.Analytics;
using Game.Scripts.GameRoot.Services.DataStorage;
using Game.Scripts.GameRoot.Services.DataStorage.Implementations;
using Game.Scripts.GameRoot.Services.DataStorage.Interfaces;
using Game.Scripts.GameRoot.Services.DataStorage.Json;
using Game.Scripts.GameRoot.Services.Feedback;
using Game.Scripts.GameRoot.Services.Overlay;
using Game.Scripts.GameRoot.Services.Popups;
using Game.Scripts.GameRoot.Services.Scenes;
using Game.Scripts.Services.Money;
using Lofelt.NiceVibrations;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameRoot
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private UIRootView _uiRootViewPrefab;
        [SerializeField] private ShopItemsDatabase _shopItemsDatabase;

        [Header("Daily Quests")]
        [SerializeField] private DailyQuestManager _dailyQuestManagerPrefab;

        protected override void Configure(
            IContainerBuilder builder)
        {
            UIRootView uiRootInstance = null;

            if (_uiRootViewPrefab != null)
            {
                uiRootInstance =
                    Instantiate(_uiRootViewPrefab);

                DontDestroyOnLoad(
                    uiRootInstance.gameObject);

                builder.RegisterComponent(
                    uiRootInstance);
            }
            else
            {
                Debug.LogError(
                    "[ProjectLifetimeScope] " +
                    "UIRootView prefab is not assigned!");
            }

            if (_shopItemsDatabase != null)
            {
                builder.RegisterInstance(
                    _shopItemsDatabase);
            }
            else
            {
                Debug.LogError(
                    "[ProjectLifetimeScope] " +
                    "ShopItemsDatabase is not assigned!");
            }

            builder.Register<PopupService>(
                    Lifetime.Singleton)
                .As<IPopupService>();

            builder.Register<UIOverlayService>(
                    Lifetime.Singleton)
                .As<IUIOverlayService>();

            GameObject coroutinesGameObject =
                new GameObject("[COROUTINES]");

            DontDestroyOnLoad(
                coroutinesGameObject);

            Coroutines coroutines =
                coroutinesGameObject
                    .AddComponent<Coroutines>();

            builder.RegisterComponent(
                coroutines);

            builder.Register<PlayerPrefsDataStorage>(
                    Lifetime.Singleton)
                .As<IDataStorage>();

            builder.Register<GameSettingsStorage>(
                    Lifetime.Singleton)
                .As<IGameSettingsStorage>();

            builder.Register<OfflineAnalyticEventService>(
                    Lifetime.Singleton)
                .As<IAnalyticEventService>();

            builder.Register<FeedbackService>(
                    Lifetime.Singleton)
                .As<IFeedbackService>();

            builder.Register<SceneService>(
                    Lifetime.Singleton)
                .As<ISceneService>();

            builder.Register<ShopProgressService>(
                Lifetime.Singleton);

            builder.Register<GameDataRepository>(
                Lifetime.Singleton);

            builder.Register<MoneyService>(
                Lifetime.Singleton);

            builder.Register<ScoreService>(
                Lifetime.Singleton);

            builder.Register<BattlePassService>(
                Lifetime.Singleton);

            builder.Register<ShopAppearanceService>(
                Lifetime.Singleton);

            builder.Register<JsonDataService>(
                    Lifetime.Singleton)
                .As<IJsonDataService>();

            RegisterDailyQuestManager(
                builder,
                uiRootInstance);

            builder.RegisterBuildCallback(
                container =>
                {
                    IGameSettingsStorage settings =
                        container.Resolve<
                            IGameSettingsStorage>();

                    HapticController.hapticsEnabled =
                        settings.IsHapticOn;

                });

            builder.RegisterEntryPoint<
                GameBootstrapper>();

            Application.targetFrameRate = 165;

            Screen.sleepTimeout =
                SleepTimeout.NeverSleep;
        }

        private void RegisterDailyQuestManager(
            IContainerBuilder builder,
            UIRootView uiRootInstance)
        {
            DailyQuestManager dailyQuestManager =
                null;

            if (uiRootInstance != null)
            {
                dailyQuestManager =
                    uiRootInstance
                        .GetComponentInChildren<
                            DailyQuestManager>(true);
            }

            if (dailyQuestManager == null &&
                _dailyQuestManagerPrefab != null)
            {
                dailyQuestManager =
                    Instantiate(
                        _dailyQuestManagerPrefab);

                DontDestroyOnLoad(
                    dailyQuestManager.gameObject);
            }

            if (dailyQuestManager == null)
            {
                Debug.LogError(
                    "[ProjectLifetimeScope] " +
                    "DailyQuestManager не найден внутри " +
                    "UIRootView и его prefab не назначен.",
                    this);

                return;
            }

            builder.RegisterComponent(
                dailyQuestManager);
        }
    }
}