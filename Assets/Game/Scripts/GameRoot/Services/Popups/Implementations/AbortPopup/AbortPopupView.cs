using System;
using DG.Tweening;
using Game.Scripts.Gameplay.Core.StateMachine;
using Game.Scripts.Gameplay.Core.StateMachine.States;
using Game.Scripts.GameRoot.Services.Feedback;
using Game.Scripts.GameRoot.Services.Popups;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Scripts.Services.Popups.SkillsPopup
{
    public class AbortPopupView : MonoBehaviour, IPopup
    {
        public event Action OnClose;

        private IFeedbackService _audioService;
        private GameplayStateMachine _stateMachine;

        [SerializeField] private Button _closePopupButton;
        [SerializeField] private Button _endGameButton;
        
        [SerializeField] private Image _backImage;

        [Inject]
        public void Construct(IFeedbackService audioService, GameplayStateMachine stateMachine)
        {
            _audioService = audioService;
            _stateMachine = stateMachine;
        }


        private void Awake() {
            _audioService?.PlaySfx("popupAppear");
        }

        public void Bind(IPopupData data)
        {
            
            PopupAnimationHandler.PlaySpawnAnimation(gameObject);

            
            _closePopupButton.onClick.AddListener(() =>
            {
                ClosePopup();
            });
            
            _endGameButton.onClick.AddListener(() =>
            {
                EndGamePopup();
            });
        }
        

        public void EndGamePopup()
        {
            _stateMachine.Enter<GameOverState>();
            Destroy(gameObject);
        }
        
        public void ClosePopup()
        {

            OnClose?.Invoke();
            if (_backImage != null)
                _backImage.DOFade(0f, 0.1f)
                    .OnComplete(() => Destroy(gameObject));
            else
                Destroy(gameObject);
        }
    }
}