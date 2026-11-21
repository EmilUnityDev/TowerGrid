namespace Game.Scripts.Gameplay.UI
{
    public struct ScoreBonusData
    {
        public string Label;
        public int Value;
        public bool IsMultiplier;

        public ScoreBonusData(string label, int value, bool isMultiplier = false)
        {
            Label = label;
            Value = value;
            IsMultiplier = isMultiplier;
        }

        public string GetDisplayText()
        {
            if (IsMultiplier)
                return $"{Label} X{Value}";
            return $"{Label} +{Value}";
        }
    }
}
