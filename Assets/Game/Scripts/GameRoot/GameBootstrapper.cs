using Game.Scripts.GameRoot.Services.Overlay;
using Game.Scripts.GameRoot.Services.Popups;
using Game.Scripts.GameRoot.Services.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameRoot
{
    public class GameBootstrapper : IStartable
    {
        private readonly ISceneService _sceneService;
        private readonly IUIOverlayService _overlayService;
        private readonly UIRootView _uiRoot;
        private readonly IObjectResolver _objectResolver;

        [Inject]
        public GameBootstrapper(ISceneService sceneService, IUIOverlayService overlayService, UIRootView uiRoot, IObjectResolver objectResolver)
        {
            _sceneService = sceneService;
            _overlayService = overlayService;
            _uiRoot = uiRoot;
            _objectResolver = objectResolver;
        }

        public void Start()
        {
            CheckAndLoadInitialScene();
            _objectResolver.InjectGameObject(_uiRoot.gameObject);
        }

        private void CheckAndLoadInitialScene()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (sceneName != Scenes.BOOT)
                return;

            if (PlayerPrefs.GetInt("TutorialGameCompleted", 0) == 0)
            {
                Debug.Log("[GameBootstrapper] Loading Gameplay scene (tutorial game)");
                _sceneService.LoadScene(Scenes.GAMEPLAY, OnLoadStart, null);
            }
            else
            {
                Debug.Log("[GameBootstrapper] Loading Main Menu scene");
                _sceneService.LoadScene(Scenes.MAIN_MENU, OnLoadStart, null);
            }
        }

        private void OnLoadStart()
        {
            _uiRoot.ClearSceneUI();
            _overlayService.ShowLoadingWithTransition();
        }
    }
}