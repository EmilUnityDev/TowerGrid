namespace Game.Scripts.Gameplay.Core.Services
{
    public class MatchRewardService
    {
        private readonly ScoreService _scoreService;

        private bool _rewardGranted;

        public MatchRewardService(
            ScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        public void BeginMatch()
        {
            _rewardGranted = false;
            _scoreService.ResetScore();
        }

        public int GrantMatchReward()
        {
            if (_rewardGranted)
                return 0;

            _rewardGranted = true;

            return 0;
        }
    }
}