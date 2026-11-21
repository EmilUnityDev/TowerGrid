using System;
using Game.Scripts.Gameplay.Building.Grid;
using Game.Scripts.Gameplay.Core.StateMachine;
using Game.Scripts.Gameplay.Core.StateMachine.States;
using Game.Scripts.Gameplay.Skills;
using UnityEngine;

namespace Game.Scripts.Gameplay.Core.Services
{
    public class GameOverCheckService
    {
        private readonly IBuildingGrid _grid;
        private readonly SkillService _skillService;
        private readonly GameplayStateMachine _stateMachine;
        private readonly DailyQuestManager _dailyQuestManager;
        private readonly TutorialGameService _tutorialGameService;

        private Func<int[]> _slotWidthsProvider;
        private bool _gameOverTriggered;

        public GameOverCheckService(
            IBuildingGrid grid,
            SkillService skillService,
            GameplayStateMachine stateMachine,
            DailyQuestManager dailyQuestManager,
            TutorialGameService tutorialGameService)
        {
            _grid = grid;
            _skillService = skillService;
            _stateMachine = stateMachine;
            _dailyQuestManager = dailyQuestManager;
            _tutorialGameService = tutorialGameService;
        }

        public void SetSlotWidthsProvider(
            Func<int[]> provider)
        {
            _slotWidthsProvider = provider;
        }

        public void Reset()
        {
            _gameOverTriggered = false;
        }

        public void Check()
        {
            if (_gameOverTriggered)
                return;

            if (_skillService.Demolition.IsActive ||
                _skillService.Rezone.IsActive)
            {
                return;
            }

            int[] slotWidths =
                _slotWidthsProvider?.Invoke();

            if (slotWidths == null ||
                slotWidths.Length == 0)
            {
                return;
            }

            bool hasAvailableTile = false;
            bool canPlaceAnyTile = false;

            foreach (int width in slotWidths)
            {
                if (width <= 0)
                    continue;

                hasAvailableTile = true;

                if (_grid.HasSpaceForSize(width))
                {
                    canPlaceAnyTile = true;
                    break;
                }
            }

            if (!hasAvailableTile)
                return;

            if (canPlaceAnyTile)
                return;

            TriggerGameOver();
        }

        private void TriggerGameOver()
        {
            if (_gameOverTriggered)
                return;

            _gameOverTriggered = true;

            if (!_tutorialGameService.IsTutorialGame)
            {
                _dailyQuestManager
                    ?.RegisterCriticalOverflow();
            }

            _stateMachine.Enter<GameOverState>();
        }
    }
}