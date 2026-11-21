using Game.Scripts.MainMenu.Root.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.MainMenu.Root
{
    public class MainMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] private UIMainMenuRootBinder _sceneUIRootPrefab;
        [SerializeField] private Camera _uiCamera;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<MainMenuController>();
            
            builder.RegisterInstance(_sceneUIRootPrefab);
            builder.RegisterInstance(_uiCamera);
            
            builder.RegisterEntryPoint<MainMenuSceneEntryPoint>();
        }
    }
}