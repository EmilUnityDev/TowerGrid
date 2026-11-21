using System;
using VContainer;

namespace Game.Scripts.GameRoot.Services.Popups
{
    public interface IPopupService
    {
        void ShowPopup<TPopup>() where TPopup : IPopup;
        void ShowPopup<TPopup>(Action onPopupClosed) where TPopup : IPopup;
        void ShowPopup<TPopup, TData>(TData popupData) where TPopup : IPopup where TData : IPopupData;
        void ShowPopup<TPopup, TData>(TData popupData, Action onPopupClosed) where TPopup : IPopup where TData : IPopupData;
        
        void CloseCurrentPopup();
        void RegisterSceneResolver(IObjectResolver sceneResolver);
        void UnregisterSceneResolver();
    }
}