using System;
using DG.Tweening;
using Game.Scripts.GameRoot;
using Game.Scripts.GameRoot.Services.Feedback;
using Game.Scripts.GameRoot.Services.Overlay;
using Game.Scripts.GameRoot.Services.Popups;
using Game.Scripts.GameRoot.Services.Scenes;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Scripts.Services.Popups.GameOverPopup
{
    public class GameOverPopupView : MonoBehaviour, IPopup
    {
        [SerializeField] private Button _submitButton;
        [SerializeField] private Image _filledImage;

        public event Action OnClose;

        private IFeedbackService _audioService;
        private IUIOverlayService _overlayService;
        private UIRootView _uiRoot;
        private ISceneService _sceneService;
        private Tween _fillTween;
        private bool _isFinishing;

        [Inject]
        public void Construct(IFeedbackService audioService, IUIOverlayService overlayService, UIRootView uiRoot, ISceneService sceneService)
        {
            _audioService = audioService;
            _overlayService = overlayService;
            _uiRoot = uiRoot;
            _sceneService = sceneService;
        }

        public void Bind(IPopupData data)
        {
            PopupAnimationHandler.PlaySpawnAnimation(gameObject);
            _audioService?.PlaySfx("submitPopup");

            if (_submitButton != null)
                _submitButton.onClick.AddListener(FinishMatch);

            if (_filledImage != null)
            {
                _filledImage.fillAmount = 0f;
                _fillTween = _filledImage.DOFillAmount(1f, 10f).SetEase(Ease.Linear).OnComplete(FinishMatch);
            }
        }

        private void FinishMatch()
        {
            if (_isFinishing) return;
            _isFinishing = true;
            _fillTween?.Kill();
            _audioService?.StopAllLoopingSfx();

            if (_sceneService != null)
            {
                _sceneService.LoadScene(Scenes.MAIN_MENU, () =>
                {
                    _overlayService?.ShowLoadingWithTransition();
                    _uiRoot?.ClearSceneUI();
                }, null);
            }
            else
            {
                Debug.LogError("[GameOverPopupView] Scene service is unavailable.");
            }

            ClosePopup();
        }

        public void ClosePopup()
        {
            _fillTween?.Kill();
            OnClose?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            _fillTween?.Kill();
            if (_submitButton != null)
                _submitButton.onClick.RemoveListener(FinishMatch);
        }
    }
}
