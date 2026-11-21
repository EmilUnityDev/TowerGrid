using Game.Scripts.Gameplay.Core.Services;

namespace Game.Scripts.Gameplay.Core.StateMachine.States
{
    public class PlayingState : IGameplayState
    {
        private readonly TimerComponent _timer;
        private readonly GameplayStateMachine _stateMachine;
        private readonly TutorialGameService _tutorialGameService;
        private readonly TutorialScenarioService _tutorialScenarioService;
        private readonly DailyQuestManager _dailyQuestManager;
        private readonly GameOverCheckService _gameOverCheckService;
        private readonly ScoreService _scoreService;

        private int _previousScore;

        public PlayingState(
            TimerComponent timer,
            GameplayStateMachine stateMachine,
            TutorialGameService tutorialGameService,
            TutorialScenarioService tutorialScenarioService,
            DailyQuestManager dailyQuestManager,
            GameOverCheckService gameOverCheckService,
            ScoreService scoreService)
        {
            _timer = timer;
            _stateMachine = stateMachine;
            _tutorialGameService = tutorialGameService;
            _tutorialScenarioService = tutorialScenarioService;
            _dailyQuestManager = dailyQuestManager;
            _gameOverCheckService = gameOverCheckService;
            _scoreService = scoreService;
        }

        public void Enter()
        {
            _gameOverCheckService.Reset();

            if (_tutorialGameService.IsTutorialGame)
            {
                _tutorialGameService.CompleteTutorial();
                _tutorialScenarioService.StartScenario();
                return;
            }

            _dailyQuestManager.RegisterMatchStarted();

            _previousScore = _scoreService.Score;

            _scoreService.OnScoreChanged +=
                OnScoreChanged;

            _timer.OnCompleted +=
                OnTimerCompleted;

            _timer.StartTimer();
        }

        public void Exit()
        {
            _scoreService.OnScoreChanged -=
                OnScoreChanged;

            _timer.OnCompleted -=
                OnTimerCompleted;

            _timer.StopTimer();
        }

        private void OnScoreChanged(
            int newScore)
        {
            int addedScore =
                newScore - _previousScore;

            _previousScore =
                newScore;

            if (addedScore <= 0)
                return;

            _dailyQuestManager.RegisterMatchScoreAdded(
                addedScore);
        }

        private void OnTimerCompleted()
        {
            _stateMachine.Enter<GameOverState>();
        }
    }
}