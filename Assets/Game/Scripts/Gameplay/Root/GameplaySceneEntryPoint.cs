using Game.Scripts.Gameplay.Building;
using Game.Scripts.Gameplay.Building.Data;
using Game.Scripts.Gameplay.Core.Services;
using Game.Scripts.Gameplay.Root.View;
using Game.Scripts.GameRoot.Services.Feedback;
using Game.Scripts.GameRoot.Services.Popups;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.Gameplay.Root
{
    public class GameplaySceneEntryPoint : IStartable
    {
        private readonly UIGameplayRootBinder _uiScene;
        private readonly Camera _camera;
        private readonly UIRootView _uiRoot;
        private readonly IPopupService _popupService;
        private readonly IObjectResolver _container;
        private readonly IFeedbackService _feedbackService;
        private readonly GameplayStateMachineRunner
            _gameplayStateMachineRunner;

        private readonly MatchRewardService
            _matchRewardService;

        public GameplaySceneEntryPoint(
            UIGameplayRootBinder uiScene,
            Camera camera,
            UIRootView uiRoot,
            IPopupService popupService,
            IObjectResolver container,
            IFeedbackService feedbackService,
            GameplayStateMachineRunner gameplayStateMachineRunner,
            MatchRewardService matchRewardService)
        {
            _uiScene = uiScene;
            _camera = camera;
            _uiRoot = uiRoot;
            _popupService = popupService;
            _container = container;
            _feedbackService = feedbackService;
            _gameplayStateMachineRunner =
                gameplayStateMachineRunner;

            _matchRewardService =
                matchRewardService;
        }

        public void Start()
        {
            AudioListener.pause = false;
            _matchRewardService.BeginMatch();

            _popupService.RegisterSceneResolver(
                _container);

            _feedbackService.PlayMusic(
                "BgMusic");

            _container.InjectGameObject(
                _uiScene.gameObject);

            _uiRoot.AttachSceneUI(
                _uiScene.gameObject,
                _camera);

            _uiScene.InitializeBinder();

            _gameplayStateMachineRunner.Run();
        }
    }
}