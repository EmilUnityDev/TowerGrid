using Game.Scripts.Gameplay.Core.Services;
using Game.Scripts.GameRoot.Services.Popups.Implementations.TutorialHintsPopup.Observer;
using Game.Scripts.Tutorial.Root.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.Tutorial.Root
{
    public class TutorialLifetimeScope : LifetimeScope
    {
        [SerializeField] private UITutorialRootBinder _sceneUITutorialRootPrefab;
        [SerializeField] private Camera _uiCamera;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<TutorialUIService>(Lifetime.Scoped).As<ITutorialUIService>();
            
            builder.RegisterComponentInHierarchy<TutorialController>();
            
            builder.RegisterInstance(_sceneUITutorialRootPrefab);
            builder.RegisterInstance(_uiCamera);

            builder.RegisterEntryPoint<TutorialSceneEntryPoint>();
        }
    }
}
