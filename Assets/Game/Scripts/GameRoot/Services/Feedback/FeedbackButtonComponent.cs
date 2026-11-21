using Lofelt.NiceVibrations;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Scripts.GameRoot.Services.Feedback
{
    [RequireComponent(typeof(Button))]
    public class FeedbackButtonComponent : MonoBehaviour
    {
        [SerializeField] private string _sfxClipName = "click1";
        [SerializeField] private HapticPatterns.PresetType _hapticPreset = HapticPatterns.PresetType.SoftImpact;

        private Button _button;
        private IFeedbackService _feedbackService;

        [Inject]
        private void Construct(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        private void Start()
        {
            _button = GetComponent<Button>();

            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (_feedbackService == null) return;

            if (!string.IsNullOrEmpty(_sfxClipName))
            {
                _feedbackService.PlaySfx(_sfxClipName);
            }

            _feedbackService.PlayHaptic(_hapticPreset);
        }

        private void OnDestroy()
        {
            if (_button)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}