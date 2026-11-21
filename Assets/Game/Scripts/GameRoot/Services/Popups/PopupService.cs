using System;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Game.Scripts.GameRoot.Services.Popups
{
    public class PopupService : IPopupService
    {
        private readonly UIRootView _uiRoot;
        private readonly IObjectResolver _rootResolver;
        private IObjectResolver _activeSceneResolver;
        private IPopup _currentPopup;

        [Inject]
        public PopupService(UIRootView uiRoot, IObjectResolver resolver)
        {
            _uiRoot = uiRoot;
            _rootResolver = resolver;
            _activeSceneResolver = resolver;
        }

        public void RegisterSceneResolver(IObjectResolver sceneResolver)
        {
            if (sceneResolver == null)
            {
                _activeSceneResolver = _rootResolver;
                return;
            }
        
            _activeSceneResolver = sceneResolver;
        }

        public void UnregisterSceneResolver()
        {
            _activeSceneResolver = _rootResolver;
        }

        public void ShowPopup<TPopup>() where TPopup : IPopup
        {
            ShowPopupInternal<TPopup>(null, null);
        }

        public void ShowPopup<TPopup>(Action onPopupClosed) where TPopup : IPopup
        {
            ShowPopupInternal<TPopup>(null, onPopupClosed);
        }

        public void ShowPopup<TPopup, TData>(TData popupData) where TPopup : IPopup where TData : IPopupData
        {
            ShowPopupInternal<TPopup>(popupData, null);
        }

        public void ShowPopup<TPopup, TData>(TData popupData, Action onPopupClosed) where TPopup : IPopup where TData : IPopupData
        {
            ShowPopupInternal<TPopup>(popupData, onPopupClosed);
        }

        private void ShowPopupInternal<TPopup>(IPopupData popupData, Action onPopupClosed) where TPopup : IPopup
        {
            var popupTypeName = typeof(TPopup).Name;
            CreatePopup(popupTypeName, popupData, onPopupClosed);
        }

        private void CreatePopup(string prefabName, IPopupData popupData = null, Action onPopupClosed = null)
        {
            var prefab = FindPrefabInResources("Popups", prefabName);

            if (prefab == null)
            {
                Debug.LogError($"[PopupService] Prefab '{prefabName}' not found.");
                return;
            }

            var popupObject = _activeSceneResolver.Instantiate(prefab);
            var popup = popupObject.GetComponent<IPopup>();

            if (popup == null)
            {
                Object.Destroy(popupObject);
                return;
            }

            if (_currentPopup != null)
            {
                CloseCurrentPopup();
            }

            _currentPopup = popup;

            if (popupObject.TryGetComponent<MonoBehaviour>(out var viewMono))
            {
                _uiRoot.AttachPopupUI(viewMono.gameObject);
            }

            popup.Bind(popupData);

            if (onPopupClosed != null)
            {
                popup.OnClose += onPopupClosed;
            }
        }
    
        public void CloseCurrentPopup()
        {
            if (_currentPopup == null)
            {
                return;
            }

            var monoBehaviour = _currentPopup as MonoBehaviour;
            if (monoBehaviour)
            {
                _currentPopup.ClosePopup();
            }
        
            _currentPopup = null;
        }

        private GameObject FindPrefabInResources(string basePath, string prefabName)
        {
            var directPath = $"{basePath}/{prefabName}";
            var directLoad = Resources.Load<GameObject>(directPath);
            if (directLoad != null) return directLoad;

            var allPrefabs = Resources.LoadAll<GameObject>(basePath);
            return allPrefabs.FirstOrDefault(p => p.name.Equals(prefabName, StringComparison.OrdinalIgnoreCase));
        }
    }
}