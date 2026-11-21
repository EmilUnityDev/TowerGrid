using System.Collections;
using Game.Scripts.GameRoot;
using Game.Scripts.GameRoot.Services.Overlay;
using UnityEngine;

namespace Game.Scripts.Gameplay.Core.StateMachine.States
{
    public class CountdownState : IGameplayState
    {
        private const float Delay = 3f;

        private readonly IUIOverlayService _overlayService;
        private readonly Coroutines _coroutines;
        private readonly GameplayStateMachine _stateMachine;

        public CountdownState(
            IUIOverlayService overlayService,
            Coroutines coroutines,
            GameplayStateMachine stateMachine)
        {
            _overlayService = overlayService;
            _coroutines = coroutines;
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _overlayService.ShowLoadingWithTransition();
            _coroutines.StartCoroutine(CountdownCor());
        }

        public void Exit() { }

        private IEnumerator CountdownCor()
        {
            yield return new WaitForSeconds(Delay);
            _stateMachine.Enter<PlayingState>();
        }
    }
}

