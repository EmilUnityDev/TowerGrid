using UnityEngine;

namespace Game.Scripts.Gameplay.Core.Services
{
    public class TutorialSceneRefs : MonoBehaviour
    {
        [SerializeField] private GameObject _fieldHintBottom;
        [SerializeField] private GameObject _fieldSecondHintBottom;
        [SerializeField] private GameObject _fieldThirdHintBottom;
        [SerializeField] private GameObject _fieldFourthHintBottom;
        [SerializeField] private GameObject _floor1Completed;
        [SerializeField] private GameObject _redHint;

        public GameObject FieldHintBottom => _fieldHintBottom;
        public GameObject FieldSecondHintBottom => _fieldSecondHintBottom;
        public GameObject FieldThirdHintBottom => _fieldThirdHintBottom;
        public GameObject FieldFourthHintBottom => _fieldFourthHintBottom;
        public GameObject Floor1Completed => _floor1Completed;
        public GameObject RedHint => _redHint;

        private void Start()
        {
            if (_fieldHintBottom != null) _fieldHintBottom.SetActive(false);
            if (_fieldSecondHintBottom != null) _fieldSecondHintBottom.SetActive(false);
            if (_fieldThirdHintBottom != null) _fieldThirdHintBottom.SetActive(false);
            if (_fieldFourthHintBottom != null) _fieldFourthHintBottom.SetActive(false);
            if (_floor1Completed != null) _floor1Completed.SetActive(false);
            if (_redHint != null) _redHint.SetActive(false);
        }
    }
}
