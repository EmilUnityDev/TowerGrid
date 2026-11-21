using System;

namespace Game.Scripts.Gameplay.Skills
{
    public class DraftRefreshSkill : ISkill
    {
        public string Name => "Draft Refresh";
        public bool IsCharged { get; private set; }
        public bool CanUse => IsCharged;
        public event Action OnStateChanged;
        public event Action OnUsed;

        public void MarkCharged()
        {
            IsCharged = true;
            OnStateChanged?.Invoke();
        }

        public void Use()
        {
            if (!CanUse) return;
            IsCharged = false;
            OnUsed?.Invoke();
            OnStateChanged?.Invoke();
        }
    }
}
