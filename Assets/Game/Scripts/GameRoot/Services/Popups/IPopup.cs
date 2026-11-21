using System;

namespace Game.Scripts.GameRoot.Services.Popups
{
    public interface IPopup
    {
        void Bind(IPopupData data = null);
        event Action OnClose;
        void ClosePopup();
    }
}