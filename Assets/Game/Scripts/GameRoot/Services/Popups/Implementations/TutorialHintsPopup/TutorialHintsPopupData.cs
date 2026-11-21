namespace Game.Scripts.GameRoot.Services.Popups.Implementations.TutorialHintsPopup
{
    public class TutorialHintsPopupData : IPopupData
    {
        public int HintIndex { get; }

        public TutorialHintsPopupData(int hintIndex)
        {
            HintIndex = hintIndex;
        }
    }
}
