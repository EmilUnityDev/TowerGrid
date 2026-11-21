using Game.Scripts.Gameplay.Core.Services;
using Game.Scripts.Gameplay.Core.StateMachine;
using Game.Scripts.Gameplay.Core.StateMachine.States;
using Game.Scripts.GameRoot;
using Game.Scripts.GameRoot.Services.Feedback;
using Game.Scripts.GameRoot.Services.Overlay;
using Game.Scripts.GameRoot.Services.Popups;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Gameplay.Root
{
    public class GameplayStateMachineRunner : MonoBehaviour
    {
        private GameplayStateMachine _stateMachine;
        private TutorialGameService _tutorialGameService;

        [Inject]
        public void Construct(
            GameplayStateMachine stateMachine,
            IPopupService popupService,
            IUIOverlayService overlayService,
            IFeedbackService feedbackService,
            Coroutines coroutines,
            TimerComponent timer,
            TutorialGameService tutorialGameService,
            TutorialScenarioService tutorialScenarioService,
            MatchRewardService matchRewardService,
            DailyQuestManager dailyQuestManager,
            GameOverCheckService gameOverCheckService,
            ScoreService scoreService)
        {
            _stateMachine = stateMachine;
            _tutorialGameService =
                tutorialGameService;

            _stateMachine.RegisterState(
                new TutorialState(
                    popupService,
                    _stateMachine));

            _stateMachine.RegisterState(
                new CountdownState(
                    overlayService,
                    coroutines,
                    _stateMachine));

            _stateMachine.RegisterState(
                new PlayingState(
                    timer,
                    _stateMachine,
                    tutorialGameService,
                    tutorialScenarioService,
                    dailyQuestManager,
                    gameOverCheckService,
                    scoreService));

            _stateMachine.RegisterState(
                new GameOverState(
                    popupService,
                    feedbackService,
                    matchRewardService,
                    dailyQuestManager,
                    tutorialGameService,
                    scoreService));
        }

        public void Run()
        {
            if (_tutorialGameService.IsTutorialGame)
            {
                _stateMachine.Enter<
                    PlayingState>();
            }
            else if (PlayerPrefs.GetInt(
                         "TutorialGame",
                         0) == 0)
            {
                _stateMachine.Enter<
                    TutorialState>();
            }
            else
            {
                _stateMachine.Enter<
                    CountdownState>();
            }
        }
    }
}