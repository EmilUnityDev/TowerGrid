using System;

namespace Game.Scripts.GameRoot.Services.Popups.Implementations.TutorialHintsPopup.Observer
{
    public interface ITutorialUIService
    {
        event Action OnHintsPopupShown;
        event Action OnHintsPopupClosed;
        
        void NotifyHintsPopupShown();
        void NotifyHintsPopupClosed();
    }
}