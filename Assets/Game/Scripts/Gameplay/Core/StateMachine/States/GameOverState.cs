using Game.Scripts.Gameplay.Core.Services;
using Game.Scripts.GameRoot.Services.Feedback;
using Game.Scripts.GameRoot.Services.Popups;
using Game.Scripts.Services.Popups.GameOverPopup;

namespace Game.Scripts.Gameplay.Core.StateMachine.States
{
    public class GameOverState : IGameplayState
    {
        private readonly IPopupService _popupService;
        private readonly IFeedbackService _feedbackService;
        private readonly MatchRewardService _matchRewardService;
        private readonly DailyQuestManager _dailyQuestManager;
        private readonly TutorialGameService _tutorialGameService;
        private readonly ScoreService _scoreService;

        public GameOverState(
            IPopupService popupService,
            IFeedbackService feedbackService,
            MatchRewardService matchRewardService,
            DailyQuestManager dailyQuestManager,
            TutorialGameService tutorialGameService,
            ScoreService scoreService)
        {
            _popupService = popupService;
            _feedbackService = feedbackService;
            _matchRewardService = matchRewardService;
            _dailyQuestManager = dailyQuestManager;
            _tutorialGameService = tutorialGameService;
            _scoreService = scoreService;
        }

        public void Enter()
        {
            if (!_tutorialGameService.IsTutorialGame)
            {
                _dailyQuestManager.RegisterMatchFinished(
                    matchCompleted: true,
                    hadCriticalOverflow: false);
            }

            _matchRewardService.GrantMatchReward();
            LocalScoreStorage.RecordScore(_scoreService.Score);

            _feedbackService.PlaySfx(
                "WinSound");

            _popupService.ShowPopup<
                GameOverPopupView>();
        }

        public void Exit()
        {
        }
    }
}