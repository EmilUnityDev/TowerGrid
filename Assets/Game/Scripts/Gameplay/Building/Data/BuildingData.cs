using UnityEngine;

namespace Game.Scripts.Gameplay.Building.Data
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "Game/Building Data")]
    public class BuildingData : ScriptableObject
    {
        [field: SerializeField] public BuildingType Type { get; private set; }
        [field: SerializeField] public BuildingSize Size { get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }

        public int SlotWidth => (int)Size;

        public void SetData(BuildingType type, BuildingSize size, Sprite sprite)
        {
            Type = type;
            Size = size;
            Sprite = sprite;
        }
    }
}
