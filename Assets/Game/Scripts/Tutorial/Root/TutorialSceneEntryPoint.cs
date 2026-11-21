using Game.Scripts.GameRoot.Services.Popups;
using Game.Scripts.Tutorial.Root.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.Tutorial.Root
{
    public class TutorialSceneEntryPoint : IStartable
    {
        private readonly UITutorialRootBinder _uiPrefab;
        private readonly Camera _camera;
        private readonly UIRootView _uiRootView;
        private readonly TutorialController _tutorialController;
        private readonly IObjectResolver _container;
        private readonly IPopupService _popupService;

        public TutorialSceneEntryPoint(
            UITutorialRootBinder uiPrefab,
            Camera camera,
            UIRootView uiRootView,
            TutorialController tutorialController,
            IObjectResolver container,
            IPopupService popupService)
        {
            _uiPrefab = uiPrefab;
            _camera = camera;
            _uiRootView = uiRootView;
            _tutorialController = tutorialController;
            _container = container;
            _popupService = popupService;
        }

        public void Start()
        {
            _popupService.RegisterSceneResolver(_container);
            
            if (_uiPrefab == null)
            {
                return;
            }

            var uiScene = Object.Instantiate(_uiPrefab);
            _container.InjectGameObject(uiScene.gameObject);
            
            _uiRootView.AttachSceneUI(uiScene.gameObject, _camera);
            
            _tutorialController.StartVideo();
        }
    }
}

