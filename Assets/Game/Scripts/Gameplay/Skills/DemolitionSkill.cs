using System;

namespace Game.Scripts.Gameplay.Skills
{
    public class DemolitionSkill : ISkill
    {
        public string Name => "Demolition";
        public bool IsCharged { get; private set; }
        public bool IsActive { get; private set; }
        public bool CanUse => IsCharged && !IsActive;
        public event Action OnStateChanged;
        public event Action OnUsed;
        public event Action OnActivated;
        public event Action OnDeactivated;

        public void MarkCharged()
        {
            IsCharged = true;
            OnStateChanged?.Invoke();
        }

        public void Use()
        {
            if (!IsCharged || IsActive) return;
            Activate();
        }

        public void Activate()
        {
            if (!IsCharged) return;
            IsActive = true;
            OnActivated?.Invoke();
            OnStateChanged?.Invoke();
        }

        public void ConfirmUse()
        {
            IsActive = false;
            IsCharged = false;
            OnUsed?.Invoke();
            OnStateChanged?.Invoke();
        }

        public void Cancel()
        {
            IsActive = false;
            OnDeactivated?.Invoke();
            OnStateChanged?.Invoke();
        }
    }
}
