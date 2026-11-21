using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Scripts.Gameplay.Core.Services;
using VContainer;

namespace Game.Scripts.GameRoot.Services.Popups.Implementations.TutorialHintsPopup
{
    public class TutorialHintsPopupView : MonoBehaviour, IPopup
    {
        public event Action OnClose;

        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _closePopup;
        [SerializeField] private CanvasGroup _baseHintRoot;
        [SerializeField] private CanvasGroup _finalHintRoot;
        [SerializeField] private GameObject _bricksRoot;
        [SerializeField] private Button _skipButton;
        [SerializeField] private Button _endTutorial;

        [SerializeField] private Animator _finalHintAnimator;

        [SerializeField] private float _typeSpeed = 0.03f;

        private const int FinalIndex = 24;

        private int _currentIndex;
        private Coroutine _typingCoroutine;

        private static readonly HashSet<int> TransitionIndices = new()
        {
            1, 3, 5, 8, 10, 13, 16, 17, 19, 22, 23
        };

        private static readonly Dictionary<int, int> TransitionMap = new()
        {
            { 1, 2 }, { 3, 4 }, { 5, 6 }, { 8, 9 }, { 10, 11 },
            { 13, 14 }, { 16, 17 }, { 17, 18 }, { 19, 20 },
            { 22, 23 }, { 23, 24 }
        };

        private TutorialScenarioService _tutorialScenarioService;

        [Inject]
        public void Construct(TutorialScenarioService tutorialScenarioService)
        {
            _tutorialScenarioService = tutorialScenarioService;
        }

        public void Bind(IPopupData data = null)
        {
            _currentIndex = 0;
            if (data is TutorialHintsPopupData hintData)
                _currentIndex = hintData.HintIndex;

            _closePopup.onClick.AddListener(OnCloseButtonPressed);
            _skipButton.onClick.AddListener(SkipTutorial);
            _endTutorial.onClick.AddListener(EndTutorial);
            ApplyIndex(_currentIndex);
        }

        private void OnCloseButtonPressed()
        {
            if (TransitionIndices.Contains(_currentIndex))
            {
                int nextIndex = TransitionMap[_currentIndex];
                if (nextIndex == FinalIndex)
                    TransitionToFinal(nextIndex);
                else
                    ChangeIndex(nextIndex);
            }
            else
            {
                ClosePopup();
            }
        }

        public void ChangeIndex(int newIndex)
        {
            _currentIndex = newIndex;
            ApplyIndex(_currentIndex);
        }

        private void ApplyIndex(int index)
        {
            if (_bricksRoot != null)
                _bricksRoot.SetActive(index == 12);

            if (index == 12 && _descriptionText != null)
            {
                var pos = _descriptionText.rectTransform.anchoredPosition;
                pos.y = 191f;
                _descriptionText.rectTransform.anchoredPosition = pos;
            }

            if (index == FinalIndex)
            {
                TransitionToFinal(index);
                return;
            }

            SetCanvasGroup(_baseHintRoot, true);
            SetCanvasGroup(_finalHintRoot, false);

            StartTyping(GetDescriptionForIndex(index));
        }

        private void TransitionToFinal(int index)
        {
            _currentIndex = index;

            _baseHintRoot.DOFade(0f, 0.3f).OnComplete(() =>
            {
                _baseHintRoot.gameObject.SetActive(false);
                _baseHintRoot.alpha = 0f;
                _baseHintRoot.blocksRaycasts = false;
            });

            _finalHintRoot.gameObject.SetActive(true);
            _finalHintRoot.alpha = 0f;
            _finalHintRoot.blocksRaycasts = true;
            _finalHintRoot.interactable = true;
            _finalHintRoot.DOFade(1f, 0.3f);

            if (_finalHintAnimator != null) {
                _finalHintAnimator.enabled = true;
            }
        }

        private void StartTyping(string text)
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);
            _descriptionText.text = "";
            _typingCoroutine = StartCoroutine(TypeTextCoroutine(text));
        }

        private IEnumerator TypeTextCoroutine(string text)
        {
            for (int i = 0; i <= text.Length; i++)
            {
                _descriptionText.text = text[..i];
                yield return new WaitForSecondsRealtime(_typeSpeed);
            }
            _typingCoroutine = null;
        }

        private static void SetCanvasGroup(CanvasGroup cg, bool active)
        {
            if (cg == null) return;
            cg.alpha = active ? 1f : 0f;
            cg.blocksRaycasts = active;
            cg.interactable = active;
            cg.gameObject.SetActive(active);
        }

        public void ClosePopup()
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            OnClose?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            _closePopup.onClick.RemoveListener(OnCloseButtonPressed);
            _skipButton.onClick.RemoveListener(SkipTutorial);
            _endTutorial.onClick.RemoveListener(EndTutorial);
        }

        private string GetDescriptionForIndex(int index)
        {
            return index switch
            {
                0 => "Build a high-rise from fragments, close floors and score maximum points before the timer runs out.",
                1 => "The more logical the house, the higher the final result.",
                2 => "Each turn you have 3 fragments to choose from.",
                3 => "Choose the best fragment for the current situation on the field",
                4 => "Let's start with the commercial section. Commercial space is more profitable at the bottom of the building.",
                5 => "The zone type affects the value of the fragment.",
                6 => "Place the fragment on the illuminated spot to close the first floor.",
                7 => "A completed row becomes a completed floor and gives bonus points.",
                8 => "Close floors in a row to speed up the result.",
                9 => "Fragments cannot be placed in mid-air. Each block must rest on a foundation or another fragment.",
                10 => "Plan your house from the bottom up.",
                11 => "Now place the residential fragment in the middle part of the building.",
                12 => "It is more advantageous to place housing in the middle part of the house.",
                13 => "Different zones benefit from different heights.",
                14 => "The service enhances neighboring residential areas.",
                15 => "A combo occurs when a single move fulfills several beneficial conditions at once.",
                16 => "After placement, the selected fragment is replaced with a new one.",
                17 => "Sometimes the current pool does not fit the best move.",
                18 => "If the fragments don't fit, use Draft Refresh.",
                19 => "Draft Refresh helps you control the hand and find a strong move.",
                20 => "Now there is a fragment that is suitable for a strong move.",
                21 => "Premium is more profitable at the top of the building.",
                22 => "A strong move is the right piece, the right height and the closed floor.",
                23 => "Now you know the main thing: choose fragments, close floors, place zones correctly and update the pool when you need a better option.",
                FinalIndex => "",
                _ => $"Hint #{index}"
            };
        }

        public void SkipTutorial()
        {
            _tutorialScenarioService?.SkipTutorial();
        }

        public void EndTutorial()
        {
            _tutorialScenarioService?.EndTutorial();
        }
    }
}
