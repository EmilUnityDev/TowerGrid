using Game.Scripts.GameRoot.Services.Popups;
using Game.Scripts.MainMenu.Root.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.MainMenu.Root
{
    public class MainMenuSceneEntryPoint : IStartable
    {
        private readonly UIMainMenuRootBinder _uiPrefab;
        private readonly Camera _camera;
        private readonly UIRootView _uiRootView;
        private readonly IObjectResolver _container;
        private readonly IPopupService _popupService;

        [Inject]
        public MainMenuSceneEntryPoint(
            UIMainMenuRootBinder uiPrefab,
            Camera camera,
            UIRootView uiRootView,
            IObjectResolver container,
            IPopupService popupService)
        {
           _uiPrefab = uiPrefab;
            _camera = camera;
            _uiRootView = uiRootView;
            _container = container;
            _popupService = popupService;
        }

        public void Start()
        {
            _popupService.RegisterSceneResolver(_container);
            
            if (_uiPrefab == null) return;

            var uiScene = Object.Instantiate(_uiPrefab);
            _container.InjectGameObject(uiScene.gameObject);
            
            _uiRootView.AttachSceneUI(uiScene.gameObject, _camera);
        }
    }
}

