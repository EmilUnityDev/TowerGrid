using System.Collections;
using Game.Scripts.GameRoot.Services.Analytics;
using Game.Scripts.GameRoot.Services.Feedback;
using Game.Scripts.GameRoot.Services.Overlay;
using Game.Scripts.GameRoot.Services.Popups;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Scripts.GameRoot;
using VContainer;

namespace Game.Scripts.Tutorial.Root
{
    public class TutorialController : MonoBehaviour
    { 
        private IPopupService _popupService;
        private IUIOverlayService _overlayService;
        private UIRootView _uiRoot;
        private IFeedbackService _feedbackService;
        private IAnalyticEventService _analyticEventService;

        [Inject]
        public void Construct(
            IAnalyticEventService analyticEventService,
            IPopupService popupService,
            IUIOverlayService overlayService,
            UIRootView uiRoot,
            IFeedbackService feedbackService)
        {
            _popupService = popupService;
            _overlayService = overlayService;
            _uiRoot = uiRoot;
            _feedbackService = feedbackService;
            _analyticEventService = analyticEventService;
        }
        
        public void StartVideo()
        {
            if (_overlayService == null) return;
            
            _overlayService.ShowVideo(OnVideoCompleted);
        }

        private void OnVideoCompleted()
        {
            /*_popupService.ShowPopup<TutorialHintsView, TutorialHintsViewModel>(
                new TutorialHintsViewModel(TutorialHintGroup.FirstGroup)
            );*/
            
            _feedbackService?.PlayMusic("BgMusic");
        }
        
        public void EndTutorial()
        {
            _analyticEventService.SendEvent("ftue_1_completed");
            _overlayService.ShowLoadingWithTransition();
            StartCoroutine(OpenMainMenu());
        }

        public void SkipTutorial()
        {
            _analyticEventService.SendEvent("ftue_1_skipped");

            _popupService.CloseCurrentPopup();
            _overlayService.ShowLoadingWithTransition();
            StartCoroutine(OpenMainMenu());
        }

        private IEnumerator OpenMainMenu()
        {
            yield return new WaitForSeconds(0.3f);
            _uiRoot.ClearSceneUI();
            _popupService.CloseCurrentPopup();
            SceneManager.LoadScene(Scenes.MAIN_MENU);
        }
    }
}