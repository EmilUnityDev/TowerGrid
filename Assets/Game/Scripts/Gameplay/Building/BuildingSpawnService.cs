using Game.Scripts.Gameplay.Building.Data;
using Game.Scripts.Gameplay.Building.Grid;
using Game.Scripts.Gameplay.Core.Services;
using Game.Scripts.Gameplay.Skills;
using Game.Scripts.Gameplay.UI;
using Game.Scripts.GameRoot.Services.Feedback;
using UnityEngine;

namespace Game.Scripts.Gameplay.Building
{
    public class BuildingSpawnService :
        IBuildingSpawnService
    {
        private readonly IBuildingFactory _factory;
        private readonly IBuildingGrid _grid;
        private readonly BuildingsParent _buildingsParent;
        private readonly GridBoundsProvider _boundsProvider;
        private readonly GridConfig _gridConfig;
        private readonly ScoreService _scoreService;
        private readonly BuildingScoreCalculator _scoreCalculator;
        private readonly GameOverCheckService _gameOverCheckService;
        private readonly ScoreBonusSpawner _bonusSpawner;
        private readonly Camera _uiCamera;
        private readonly TutorialGameService _tutorialGameService;
        private readonly DailyQuestManager _dailyQuestManager;
        private readonly IFeedbackService _feedbackService;

        public BuildingSpawnService(
            IBuildingFactory factory,
            IBuildingGrid grid,
            BuildingsParent buildingsParent,
            GridBoundsProvider boundsProvider,
            GridConfig gridConfig,
            ScoreService scoreService,
            SkillService skillService,
            GameOverCheckService gameOverCheckService,
            ScoreBonusSpawner bonusSpawner,
            Camera uiCamera,
            TutorialGameService tutorialGameService,
            DailyQuestManager dailyQuestManager,
            IFeedbackService feedbackService)
        {
            _factory = factory;
            _grid = grid;
            _buildingsParent = buildingsParent;
            _boundsProvider = boundsProvider;
            _gridConfig = gridConfig;
            _scoreService = scoreService;
            _gameOverCheckService = gameOverCheckService;
            _bonusSpawner = bonusSpawner;
            _uiCamera = uiCamera;
            _tutorialGameService = tutorialGameService;
            _dailyQuestManager = dailyQuestManager;
            _feedbackService = feedbackService;

            _scoreCalculator =
                new BuildingScoreCalculator(
                    grid,
                    skillService,
                    tutorialGameService,
                    dailyQuestManager);
        }

        public BuildingDragHandler SpawnAndBeginDrag(
            BuildingData data,
            Vector2 screenPosition)
        {
            Camera sceneCamera =
                Camera.main;

            GameObject gameObject =
                _factory.Create(
                    data,
                    _buildingsParent.transform);

            Vector3 worldPosition =
                sceneCamera.ScreenToWorldPoint(
                    screenPosition);

            worldPosition.z = 0f;

            gameObject.transform.position =
                worldPosition;

            BuildingDragHandler dragHandler =
                gameObject.GetComponent<
                    BuildingDragHandler>();

            dragHandler.Initialize(
                _grid,
                sceneCamera,
                _boundsProvider.WorldBounds,
                _gridConfig,
                _scoreService,
                _scoreCalculator,
                _gameOverCheckService,
                _bonusSpawner,
                _uiCamera,
                _feedbackService);

            dragHandler.BeginDrag();

            return dragHandler;
        }
    }
}