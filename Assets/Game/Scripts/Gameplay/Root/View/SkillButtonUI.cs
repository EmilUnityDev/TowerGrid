using Game.Scripts.Gameplay.Skills;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.Root.View
{
    [RequireComponent(typeof(Button))]
    public class SkillButtonUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Sprite _noChargeSprite;
        [SerializeField] private Sprite _chargedSprite;
        [SerializeField] private Sprite _activeSprite;

        private Button _button;

        public Button Button => _button ??= GetComponent<Button>();

        public void UpdateState(ISkill skill, bool isActive)
        {
            if (isActive)
                _icon.sprite = _activeSprite;
            else if (skill.CanUse)
                _icon.sprite = _chargedSprite;
            else
                _icon.sprite = _noChargeSprite;

            Button.interactable = skill.CanUse && !isActive;
        }
    }
}
