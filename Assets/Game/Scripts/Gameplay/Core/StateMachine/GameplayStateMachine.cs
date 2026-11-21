using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Gameplay.Core.StateMachine
{
    public class GameplayStateMachine
    {
        private readonly Dictionary<Type, IGameplayState> _states = new();
        private IGameplayState _currentState;

        public void RegisterState<TState>(TState state) where TState : IGameplayState
        {
            _states[typeof(TState)] = state;
        }

        public void Enter<TState>() where TState : IGameplayState
        {
            if (!_states.TryGetValue(typeof(TState), out var nextState))
            {
                Debug.LogError($"[GameplayStateMachine] State {typeof(TState).Name} is not registered.");
                return;
            }

            _currentState?.Exit();
            _currentState = nextState;
            _currentState.Enter();
        }
    }
}

