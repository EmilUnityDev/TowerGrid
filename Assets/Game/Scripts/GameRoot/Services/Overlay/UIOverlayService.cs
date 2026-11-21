using System;
using System.Collections;
using Game.Scripts.GameRoot.Services.Analytics;
using Game.Scripts.GameRoot.Services.Feedback;
using Game.Scripts.GameRoot.Services.Popups;
using UnityEngine;
using UnityEngine.Video;

namespace Game.Scripts.GameRoot.Services.Overlay
{
    public sealed class UIOverlayService : IUIOverlayService
    {
        private readonly UIRootView _uiRoot;
        private readonly IAnalyticEventService _analyticService;
        private readonly IFeedbackService _feedbackService;

        private Coroutine _currentLoadingCoroutine;
        private Coroutine _currentTransitionCoroutine;
        private Coroutine _currentVideoCoroutine;
        private bool _videoEnded;
        private bool _videoSkipped;
        
        private Action _currentLoadingCallback;
        private Action _currentTransitionCallback;
        private Action _currentVideoCallback;

        public UIOverlayService(UIRootView uiRoot, IAnalyticEventService analyticService,
            IFeedbackService feedbackService)
        {
            _uiRoot = uiRoot ?? throw new ArgumentNullException(nameof(uiRoot));
            _analyticService = analyticService ?? throw new ArgumentNullException(nameof(analyticService));
            _feedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));

            InitializeScreens();
            InitializeVideo();
        }

        private void InitializeScreens()
        {
            HideAllScreens();
            AudioListener.volume = 1f;
        }

        private void InitializeVideo()
        {
            if (_uiRoot.VideoPlayer != null)
            {
                _uiRoot.VideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                _uiRoot.VideoPlayer.Stop();
            }

            if (_uiRoot.SkipButton != null)
            {
                _uiRoot.SkipButton.onClick.AddListener(OnSkipButtonClicked);
                _uiRoot.SkipButton.gameObject.SetActive(false);
            }
            
            if (_uiRoot.VideoScreen != null)
            {
                _uiRoot.VideoScreen.gameObject.SetActive(false);
            }
        }

        public void ShowLoading(bool useSecondary = false)
        {
            ShowLoading(null, useSecondary);
        }

        public void ShowLoading(Action onLoadingComplete, bool useSecondary = false)
        {
            ShowLoading(onLoadingComplete, _uiRoot.LoadingScreenDuration, useSecondary);
        }

        public void ShowLoading(float duration, bool useSecondary = false)
        {
            ShowLoading(null, duration, useSecondary);
        }

        public void ShowLoading(Action onLoadingComplete, float duration, bool useSecondary = false)
        {
            var loadingScreen = GetLoadingScreen(useSecondary);
            if (!loadingScreen) return;

            loadingScreen.SetActive(true);
            StopCurrentLoadingCoroutine();
            _currentLoadingCallback = onLoadingComplete;
            _currentLoadingCoroutine = _uiRoot.StartCoroutine(HideLoadingAfterDuration(loadingScreen, duration));
        }

        public void HideAllLoading()
        {
            StopCurrentLoadingCoroutine();
            HideLoadingScreen(true);
            HideLoadingScreen(false);
        }

        public void ShowTransition(bool useSecondary = false)
        {
            ShowTransition(null, useSecondary);
        }

        public void ShowTransition(Action onTransitionMidpoint, bool useSecondary = false)
        {
            var transitionScreen = GetTransitionScreen(useSecondary);
            if (transitionScreen != null)
            {
                transitionScreen.SetActive(true);
                StopCurrentTransitionCoroutine();
                _currentTransitionCallback = onTransitionMidpoint;
                _currentTransitionCoroutine = _uiRoot.StartCoroutine(
                    HideTransitionAfterDuration(transitionScreen)
                );
            }
        }

        public void ShowLoadingWithTransition(bool useSecondaryLoading = false, bool useSecondaryTransition = false)
        {
            ShowLoadingWithTransition(null, null, useSecondaryLoading, useSecondaryTransition);
        }

        public void ShowLoadingWithTransition(Action onLoadingComplete, Action onTransitionMidpoint,
            bool useSecondaryLoading = false, bool useSecondaryTransition = false)
        {
            var loadingScreen = GetLoadingScreen(useSecondaryLoading);

            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
                StopCurrentLoadingCoroutine();
                StopCurrentTransitionCoroutine();
                
                _currentLoadingCallback = onLoadingComplete;
                _currentTransitionCallback = onTransitionMidpoint;
                _currentLoadingCoroutine = _uiRoot.StartCoroutine(
                    ShowLoadingWithTransitionSequence(loadingScreen, useSecondaryTransition)
                );
            }
        }

        public void ShowVideo()
        {
            ShowVideo(null);
        }

        public void ShowVideo(Action onVideoCompleted)
        {
            if (_uiRoot.VideoPlayer == null)
            {
                return;
            }

            _videoEnded = false;
            _videoSkipped = false;
            _currentVideoCallback = onVideoCompleted;
            StopCurrentVideoCoroutine();
            _currentVideoCoroutine = _uiRoot.StartCoroutine(ShowVideoSequence());
        }

        public void CloseVideo()
        {
            if (_videoEnded) return;
            
            _videoEnded = true;
            StopCurrentVideoCoroutine();
            _currentVideoCoroutine = _uiRoot.StartCoroutine(CloseVideoSequence());
        }

        private void HideLoadingScreen(bool isSecondary)
        {
            var loadingScreen = GetLoadingScreen(isSecondary);
            if (loadingScreen)
            {
                loadingScreen.SetActive(false);
            }
        }

        private IEnumerator ShowVideoSequence()
        {
            _uiRoot.VideoScreen.gameObject.SetActive(true);
            _uiRoot.VideoScreen.enabled = false;
            
            if (_uiRoot.SkipButton != null)
                _uiRoot.SkipButton.gameObject.SetActive(false);

            _uiRoot.VideoPlayer.Prepare();
            
            while (!_uiRoot.VideoPlayer.isPrepared)
            {
                yield return null;
            }

            yield return new WaitForSeconds(_uiRoot.VideoDelayBeforePlay);

            if (_uiRoot.SkipButton != null)
                _uiRoot.SkipButton.gameObject.SetActive(true);
            
            if (_uiRoot.VideoScreen != null)
            {
                _uiRoot.VideoScreen.enabled = true;
                _uiRoot.VideoScreen.texture = _uiRoot.VideoPlayer.texture;
            }

            _uiRoot.VideoPlayer.Play();
            _feedbackService?.PlayLoopingSfx("Ftue_video_sound");

            yield return new WaitForSeconds(_uiRoot.VideoTransitionBeforeEndDelay);

            if (_videoEnded || _videoSkipped)
            {
                yield break;
            }

            _analyticService?.SendEvent("video_ftue_completed");
            CloseVideo();
        }

        private IEnumerator CloseVideoSequence()
        {
            ShowTransition();

            if (_uiRoot.SkipButton != null)
                _uiRoot.SkipButton.gameObject.SetActive(false);

            yield return new WaitForSeconds(_uiRoot.VideoCloseDelayAfterTransition);

            _feedbackService?.StopAllLoopingSfx();
            
            if (_uiRoot.VideoScreen != null)
                _uiRoot.VideoScreen.gameObject.SetActive(false);
            
            var callback = _currentVideoCallback;
            _currentVideoCallback = null;
            callback?.Invoke();
        }

        private void OnSkipButtonClicked()
        {
            if (_videoEnded || _videoSkipped) return;

            _videoSkipped = true;
            _analyticService?.SendEvent("video_ftue_skipped");
            CloseVideo();
        }

        private IEnumerator HideLoadingAfterDuration(GameObject loadingScreen, float duration)
        {
            var loadingDuration = duration <= 0 ? _uiRoot.LoadingScreenDuration : duration;
            yield return new WaitForSeconds(loadingDuration);
            
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }

            var callback = _currentLoadingCallback;
            _currentLoadingCallback = null;
            callback?.Invoke();
        }

        private IEnumerator HideTransitionAfterDuration(GameObject transitionScreen)
        {
            var halfDuration = _uiRoot.TransitionScreenDuration * 0.5f;
            yield return new WaitForSeconds(halfDuration);
            
            var callback = _currentTransitionCallback;
            _currentTransitionCallback = null;
            callback?.Invoke();
            
            yield return new WaitForSeconds(halfDuration);
            
            if (transitionScreen != null)
            {
                transitionScreen.SetActive(false);
            }
        }

        private IEnumerator ShowLoadingWithTransitionSequence(GameObject loadingScreen, bool useSecondaryTransition)
        {
            yield return new WaitForSeconds(_uiRoot.TransitionScreenDelay);

            var transitionScreen = GetTransitionScreen(useSecondaryTransition);
            if (transitionScreen != null)
            {
                transitionScreen.SetActive(true);
                _currentTransitionCoroutine = _uiRoot.StartCoroutine(
                    HideTransitionAfterDuration(transitionScreen)
                );
            }

            var remainingLoadingTime = Mathf.Max(0f, _uiRoot.LoadingScreenDuration - _uiRoot.TransitionScreenDelay);
            yield return new WaitForSeconds(remainingLoadingTime);
            
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }

            var callback = _currentLoadingCallback;
            _currentLoadingCallback = null;
            callback?.Invoke();
        }

        private GameObject GetLoadingScreen(bool useSecondary)
        {
            if (useSecondary && _uiRoot.LoadingScreenSecondary != null)
            {
                return _uiRoot.LoadingScreenSecondary;
            }
            
            return _uiRoot.LoadingScreenPrimary;
        }

        private GameObject GetTransitionScreen(bool useSecondary)
        {
            if (useSecondary && _uiRoot.TransitionScreenSecondary != null)
            {
                return _uiRoot.TransitionScreenSecondary;
            }
            
            return _uiRoot.TransitionScreenPrimary;
        }

        private void HideAllScreens()
        {
            HideAllLoadingScreens();
            HideAllTransitionScreens();
        }

        private void HideAllLoadingScreens()
        {
            if (_uiRoot.LoadingScreenPrimary != null)
            {
                _uiRoot.LoadingScreenPrimary.SetActive(false);
            }
            
            if (_uiRoot.LoadingScreenSecondary != null)
            {
                _uiRoot.LoadingScreenSecondary.SetActive(false);
            }
        }

        private void HideAllTransitionScreens()
        {
            if (_uiRoot.TransitionScreenPrimary != null)
            {
                _uiRoot.TransitionScreenPrimary.SetActive(false);
            }
            
            if (_uiRoot.TransitionScreenSecondary != null)
            {
                _uiRoot.TransitionScreenSecondary.SetActive(false);
            }
        }

        private void StopCurrentLoadingCoroutine()
        {
            if (_currentLoadingCoroutine != null)
            {
                _uiRoot.StopCoroutine(_currentLoadingCoroutine);
                _currentLoadingCoroutine = null;
            }
        }

        private void StopCurrentTransitionCoroutine()
        {
            if (_currentTransitionCoroutine != null)
            {
                _uiRoot.StopCoroutine(_currentTransitionCoroutine);
                _currentTransitionCoroutine = null;
            }
        }

        private void StopCurrentVideoCoroutine()
        {
            if (_currentVideoCoroutine != null)
            {
                _uiRoot.StopCoroutine(_currentVideoCoroutine);
                _currentVideoCoroutine = null;
            }
        }
    }
}