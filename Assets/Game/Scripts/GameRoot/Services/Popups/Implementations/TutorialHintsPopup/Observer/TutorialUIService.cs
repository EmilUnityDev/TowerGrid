using System;

namespace Game.Scripts.GameRoot.Services.Popups.Implementations.TutorialHintsPopup.Observer
{
    public class TutorialUIService : ITutorialUIService
    {
        public event Action OnHintsPopupShown;
        public event Action OnHintsPopupClosed;

        public void NotifyHintsPopupShown()
        {
            OnHintsPopupShown?.Invoke();
        }

        public void NotifyHintsPopupClosed()
        {
            OnHintsPopupClosed?.Invoke();
        }
    }
}