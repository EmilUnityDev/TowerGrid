using System;
using DG.Tweening;
using Game.Scripts.Gameplay.Building.Data;
using Game.Scripts.Gameplay.Building.Grid;
using Game.Scripts.Gameplay.Core.Services;
using Game.Scripts.Gameplay.UI;
using Game.Scripts.GameRoot.Services.Feedback;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Gameplay.Building
{
    [RequireComponent(typeof(BuildingInstance))]
    public class BuildingDragHandler : MonoBehaviour
    {
        private IBuildingGrid _grid;
        private Camera _sceneCamera;
        private BuildingInstance _building;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _collider;
        private Bounds _gridBounds;
        private GridConfig _gridConfig;
        private Vector3 _returnWorldPosition;
        private bool _isDragging;
        private bool _inputWasActive;
        private bool _placementCompleted;

        private readonly Color _validColor =
            new(1f, 1f, 1f, 0.8f);

        private readonly Color _invalidColor =
            new(1f, 0.3f, 0.3f, 0.6f);

        private ScoreService _scoreService;
        private BuildingScoreCalculator _scoreCalculator;
        private GameOverCheckService _gameOverCheckService;
        private ScoreBonusSpawner _bonusSpawner;
        private Camera _uiCamera;
        private IFeedbackService _feedbackService;

        public event Action OnPlaced;
        public event Action OnFailed;

        [SerializeField] private ParticleSystem _dustPS;
        [SerializeField] private ParticleSpawner destroyParticles;
        [SerializeField] private string _sfxClipName1 = "gainCoins1";
        [SerializeField] private string _sfxClipName2 = "gainCoins2";
        private string _sfxClipName3 = "setBuilding";
        private string _sfxClipName4 = "setBuilding2";

        public void Initialize(
            IBuildingGrid grid,
            Camera sceneCamera,
            Bounds gridBounds,
            GridConfig gridConfig,
            ScoreService scoreService = null,
            BuildingScoreCalculator scoreCalculator = null,
            GameOverCheckService gameOverCheckService = null,
            ScoreBonusSpawner bonusSpawner = null,
            Camera uiCamera = null,
            IFeedbackService feedbackService = null)
        {
            _grid = grid;
            _sceneCamera = sceneCamera;
            _gridBounds = gridBounds;
            _gridConfig = gridConfig;
            _building =
                GetComponent<BuildingInstance>();

            _spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();

            _collider =
                GetComponent<BoxCollider2D>();

            _scoreService = scoreService;
            _scoreCalculator = scoreCalculator;
            _gameOverCheckService =
                gameOverCheckService;

            _bonusSpawner = bonusSpawner;
            _uiCamera = uiCamera;
            _feedbackService = feedbackService;
        }

        public void SetReturnPosition(
            Vector3 worldPosition)
        {
            _returnWorldPosition =
                worldPosition;
        }

        public void BeginDrag()
        {
            _isDragging = true;
            _inputWasActive = false;
            _placementCompleted = false;

            SetSortingOrder(10);
        }

        private void Update()
        {
            if (!_isDragging)
                return;

            bool inputActive =
                IsInputActive();

            if (inputActive)
            {
                _inputWasActive = true;

                Vector2 screenPosition =
                    GetInputScreenPosition();

                Vector3 worldPosition =
                    _sceneCamera.ScreenToWorldPoint(
                        screenPosition);

                worldPosition.z = 0f;

                worldPosition.x =
                    Mathf.Clamp(
                        worldPosition.x,
                        _gridBounds.min.x,
                        _gridBounds.max.x);

                worldPosition.y =
                    Mathf.Clamp(
                        worldPosition.y,
                        _gridBounds.min.y,
                        _gridBounds.max.y);

                transform.position =
                    worldPosition;

                float colliderWidth =
                    GetColliderWidth();

                bool canPlace =
                    _grid.TryGetSlotAt(
                        worldPosition,
                        _building.Data,
                        out _,
                        out int previewLevel);

                if (canPlace)
                {
                    canPlace =
                        !_grid.IsOverlapping(
                            worldPosition,
                            colliderWidth,
                            previewLevel);
                }

                if (_spriteRenderer != null)
                {
                    _spriteRenderer.color =
                        canPlace
                            ? _validColor
                            : _invalidColor;
                }
            }
            else if (_inputWasActive)
            {
                FinishDrag();
            }
        }

        private void FinishDrag()
        {
            if (_placementCompleted)
                return;

            _isDragging = false;

            SetSortingOrder(9);

            Vector2 worldPosition =
                transform.position;

            float colliderWidth =
                GetColliderWidth();

            if (_grid.TryGetSlotAt(
                    worldPosition,
                    _building.Data,
                    out int slot,
                    out int level))
            {
                bool isNotOverlapping =
                    !_grid.IsOverlapping(
                        worldPosition,
                        colliderWidth,
                        level);

                bool placed =
                    isNotOverlapping &&
                    _grid.TryPlace(
                        slot,
                        level,
                        _building.Data);

                if (placed)
                {
                    CompletePlacement(
                        slot,
                        level,
                        colliderWidth);

                    return;
                }
            }

            AnimateFailure();
        }

        private void CompletePlacement(
            int slot,
            int level,
            float colliderWidth)
        {
            _placementCompleted = true;

            Vector2 snapPosition =
                _grid.GetTightSnapPosition(
                    slot,
                    level,
                    _building.Data,
                    colliderWidth,
                    _gridBounds.min.x,
                    _gridBounds.max.x);

            transform.position =
                new Vector3(
                    snapPosition.x,
                    snapPosition.y,
                    0f);

            transform.localScale =
                Vector3.one;

            Vector3 localPosition =
                transform.localPosition;

            localPosition.y =
                _gridConfig.LevelYPositions[level];

            transform.localPosition =
                localPosition;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color =
                    Color.white;
            }

            _building.MarkPlaced();

            OnPlaced?.Invoke();

            AwardScore(
                slot,
                level);

            AnimatePlacement();
        }

        private void AwardScore(
            int slot,
            int level)
        {
            if (_scoreService == null ||
                _scoreCalculator == null)
            {
                return;
            }

            ScoreBonusResult result =
                _scoreCalculator.CalculateDetailed(
                    _building.Data,
                    slot,
                    level);

            _scoreService.AddPoints(
                result.TotalScore);

            if (_bonusSpawner == null ||
                result.Bonuses == null ||
                result.Bonuses.Count == 0)
            {
                return;
            }

            Vector3 screenPosition =
                _sceneCamera.WorldToScreenPoint(
                    transform.position);

            RectTransform parentRect =
                _bonusSpawner.SpawnParent;

            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    parentRect,
                    screenPosition,
                    _uiCamera,
                    out Vector2 localPosition);

            _bonusSpawner.SpawnBonuses(
                result.Bonuses,
                localPosition);
        }

        private void AnimatePlacement()
        {
            enabled = false;

            _dustPS?.Play();

            if (destroyParticles != null)
            {
                destroyParticles
                    .SpawnAndAttract();
            }


            string sfxToPlay =
                UnityEngine.Random.Range(0, 2) == 0
                    ? _sfxClipName3
                    : _sfxClipName4;
            _feedbackService?.PlaySfx(
                        sfxToPlay);

            DOVirtual.DelayedCall(
                0.2f,
                () =>
                {
                    string sfxToPlay =
               UnityEngine.Random.Range(0, 2) == 0
                   ? _sfxClipName1
                   : _sfxClipName2;

                    _feedbackService?.PlaySfx(
                        sfxToPlay);
                });
            DOVirtual.DelayedCall(
                0.1f,
                () => {
                    string sfxToPlay =
               UnityEngine.Random.Range(0, 2) == 0
                   ? _sfxClipName1
                   : _sfxClipName2;

                    _feedbackService?.PlaySfx(
                        sfxToPlay);
                });
            DOVirtual.DelayedCall(
             0.25f,
             () => {
                 string sfxToPlay =
            UnityEngine.Random.Range(0, 2) == 0
                ? _sfxClipName1
                : _sfxClipName2;

                 _feedbackService?.PlaySfx(
                     sfxToPlay);
             });




            transform
                .DOPunchScale(
                    Vector3.one * 0.15f,
                    0.3f,
                    6,
                    0.5f)
                .SetEase(Ease.OutBack)
                .OnComplete(
                    () =>
                    {
                        _gameOverCheckService
                            ?.Check();
                    });
        }

        private void AnimateFailure()
        {
            enabled = false;

            SetSortingOrder(11);

            Sequence sequence =
                DOTween.Sequence();

            sequence.Append(
                transform
                    .DOMove(
                        _returnWorldPosition,
                        0.35f)
                    .SetEase(
                        Ease.InOutCubic));

            sequence.Join(
                transform
                    .DOScale(
                        Vector3.one * 0.5f,
                        0.35f)
                    .SetEase(
                        Ease.InCubic));

            if (_spriteRenderer != null)
            {
                sequence.Join(
                    _spriteRenderer
                        .DOFade(
                            0f,
                            0.25f)
                        .SetDelay(
                            0.1f));
            }

            sequence.OnComplete(
                () =>
                {
                    OnFailed?.Invoke();

                    Destroy(
                        gameObject);
                });
        }

        private float GetColliderWidth()
        {
            if (_collider != null)
            {
                return _collider.size.x *
                       transform.lossyScale.x;
            }

            if (_spriteRenderer != null)
            {
                return _spriteRenderer.bounds.size.x;
            }

            return _building.Data.SlotWidth *
                   _gridConfig.SlotWidth;
        }

        private bool IsInputActive()
        {
            if (Input.touchCount > 0)
            {
                TouchPhase phase =
                    Input.GetTouch(0).phase;

                return phase ==
                           TouchPhase.Began ||
                       phase ==
                           TouchPhase.Moved ||
                       phase ==
                           TouchPhase.Stationary;
            }

            return Input.GetMouseButton(0);
        }

        private Vector2 GetInputScreenPosition()
        {
            if (Input.touchCount > 0)
            {
                return Input
                    .GetTouch(0)
                    .position;
            }

            return Input.mousePosition;
        }

        private void SetSortingOrder(
            int order)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingOrder =
                    order;
            }
        }
    }
}