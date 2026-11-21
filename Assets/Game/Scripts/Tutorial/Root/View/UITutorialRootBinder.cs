using Game.Scripts.GameRoot.Services.Feedback;
using Game.Scripts.GameRoot.Services.Popups;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Tutorial.Root.View
{
    public class UITutorialRootBinder : MonoBehaviour
    {
        private IFeedbackService _audioService;
        private IPopupService _popupService;

        [Inject]
        public void Construct(
            IFeedbackService audioService,
            IPopupService popupService)
        {
            _audioService = audioService;
            _popupService = popupService;
        }
    }
}