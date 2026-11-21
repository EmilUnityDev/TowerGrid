using Game.Scripts.Game.SlideshowGamePopup;
using Game.Scripts.GameRoot.Services.Popups;
using UnityEngine;

namespace Game.Scripts.Gameplay.Core.StateMachine.States
{
    public class TutorialState : IGameplayState
    {
        private readonly IPopupService _popupService;
        private readonly GameplayStateMachine _stateMachine;

        public TutorialState(IPopupService popupService, GameplayStateMachine stateMachine)
        {
            _popupService = popupService;
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            PlayerPrefs.SetInt("TutorialGame", 1);
            PlayerPrefs.Save();

            _popupService.ShowPopup<SlideshowGamePopupView>(onPopupClosed: OnTutorialClosed);
        }

        public void Exit() { }

        private void OnTutorialClosed()
        {
            _stateMachine.Enter<CountdownState>();
        }
    }
}

