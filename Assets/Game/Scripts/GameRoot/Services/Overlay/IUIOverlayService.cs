using System;

namespace Game.Scripts.GameRoot.Services.Overlay
{
    public interface IUIOverlayService
    {
        void ShowLoading(bool useSecondary = false);
        void ShowLoading(Action onLoadingComplete, bool useSecondary = false);
        void ShowLoading(float duration, bool useSecondary = false);
        void ShowLoading(Action onLoadingComplete, float duration, bool useSecondary = false);
        void HideAllLoading();

        void ShowTransition(bool useSecondary = false);
        void ShowTransition(Action onTransitionMidpoint, bool useSecondary = false);

        void ShowLoadingWithTransition(bool useSecondaryLoading = false, bool useSecondaryTransition = false);
        void ShowLoadingWithTransition(Action onLoadingComplete, Action onTransitionMidpoint, bool useSecondaryLoading = false, bool useSecondaryTransition = false);

        void ShowVideo();
        void ShowVideo(Action onVideoCompleted);

        void CloseVideo();
    }
}