using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.GameRoot.Services.Popups;
using UnityEngine;
using UnityEngine.UI;

public class DailyTaskPopupView : MonoBehaviour, IPopup
{
    [SerializeField] private Button _closeButton;
    public event Action OnClose;

    public void Bind(IPopupData data = null)
    {
        _closeButton.onClick.AddListener(ClosePopup);
    }

    public void ClosePopup()
    {
        OnClose?.Invoke();
        Destroy(gameObject);
    }
}
