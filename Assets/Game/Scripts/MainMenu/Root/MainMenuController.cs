using Game.Scripts.GameRoot.Services.Feedback;
using UnityEngine;
using VContainer;

namespace Game.Scripts.MainMenu.Root
{
    public class MainMenuController : MonoBehaviour
    {
        private IFeedbackService _audioService;

        [Inject]
        public void Construct(IFeedbackService audioService)
        {
            _audioService = audioService;
        }

        private void Start()
        {
            AudioListener.pause = false;
            _audioService.PlayMusic("BgMusic");
        }
    }
}