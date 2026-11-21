using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Game.Scripts.GameRoot.Services.Popups
{
    public class UIRootView : MonoBehaviour
    {
        [Header("Popup Container")]
        [SerializeField] private Transform _popupContainer;

        [Header("Nav Panel Container")]
        [SerializeField] private Transform _navPanelContainer;

        [Header("Scene UI")]
        [SerializeField] private Transform _uiSceneContainer;
        [SerializeField] private Canvas _canvas;

        [Header("Loading Screens")]
        [SerializeField] private GameObject _loadingScreenPrimary;
        [SerializeField] private GameObject _loadingScreenSecondary;

        [Header("Transition Screens")]
        [SerializeField] private GameObject _transitionScreenPrimary;
        [SerializeField] private GameObject _transitionScreenSecondary;

        [Header("Video Player")]
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private RawImage _videoScreen;
        [SerializeField] private Button _skipButton;

        [Header("Timing Configuration")]
        [SerializeField] private float _loadingScreenDuration = 3f;
        [SerializeField] private float _transitionScreenDelay = 2f;
        [SerializeField] private float _transitionScreenDuration = 1.8f;
        [SerializeField] private float _videoDelayBeforePlay = 4f;
        [SerializeField] private float _videoTransitionBeforeEndDelay = 19f;
        [SerializeField] private float _videoCloseDelayAfterTransition = 1.8f;

        private Camera _currentCamera;


        public Canvas Canvas => _canvas;
        public Camera CurrentCamera => _currentCamera;
        public GameObject LoadingScreenPrimary => _loadingScreenPrimary;
        public GameObject LoadingScreenSecondary => _loadingScreenSecondary;
        public GameObject TransitionScreenPrimary => _transitionScreenPrimary;
        public GameObject TransitionScreenSecondary => _transitionScreenSecondary;
        public VideoPlayer VideoPlayer => _videoPlayer;
        public RawImage VideoScreen => _videoScreen;
        public Button SkipButton => _skipButton;
        public float LoadingScreenDuration => _loadingScreenDuration;
        public float TransitionScreenDelay => _transitionScreenDelay;
        public float TransitionScreenDuration => _transitionScreenDuration;
        public float VideoDelayBeforePlay => _videoDelayBeforePlay;
        public float VideoTransitionBeforeEndDelay => _videoTransitionBeforeEndDelay;
        public float VideoCloseDelayAfterTransition => _videoCloseDelayAfterTransition;
        public Transform PopupContainer => _popupContainer;
        public Transform NavPanelContainer => _navPanelContainer;


        public void AttachPopupUI(GameObject popup)
        {
            var container = _popupContainer != null ? _popupContainer : transform;
            popup.transform.SetParent(container, false);
        }

        public void AttachSceneUI(GameObject sceneUI, Camera sceneCamera)
        {
            ClearSceneUI();
            sceneUI.transform.SetParent(_uiSceneContainer, false);
            SetupCanvasCamera(sceneCamera);
        }

        public void UpdateCanvasOrder(int order)
        {
            if (_canvas != null)
            {
                _canvas.sortingOrder = order;
            }
        }

        private void SetupCanvasCamera(Camera sceneCamera)
        {
            if (_canvas != null && sceneCamera != null)
            {
                _canvas.worldCamera = sceneCamera;
                _currentCamera = sceneCamera;
            }
        }

        public void ClearSceneUI()
        {
            if (_uiSceneContainer == null) return;

            var childCount = _uiSceneContainer.childCount;
            for (var i = childCount - 1; i >= 0; i--)
            {
                Destroy(_uiSceneContainer.GetChild(i).gameObject);
            }
        }
    }
}