using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Gameplay.UI
{
    public class ScoreBonusSpawner : MonoBehaviour
    {
        [SerializeField] private ScoreBonusPopup _popupPrefab;
        [SerializeField] private RectTransform _spawnParent;
        [SerializeField] private float _verticalSpacing = 50f;
        [SerializeField] private float _delayPerItem = 0.15f;

        public RectTransform SpawnParent => _spawnParent != null ? _spawnParent : (RectTransform)transform;

        public void SpawnBonuses(List<ScoreBonusData> bonuses, Vector2 anchoredPosition)
        {
            var parent = SpawnParent;
            float currentY = 0f;

            for (int i = 0; i < bonuses.Count; i++)
            {
                var bonus = bonuses[i];
                var instance = Instantiate(_popupPrefab, parent);
                var rt = (RectTransform)instance.transform;

                rt.anchoredPosition = anchoredPosition + new Vector2(0f, currentY);

                float spacing = _verticalSpacing;
                if (bonus.Label is "SERVICE ADJACENCY" or "FLOOR COMPLETE")
                    spacing += 20f;

                currentY += spacing;

                instance.Show(bonus.GetDisplayText(), i * _delayPerItem);
            }
        }
    }
}
