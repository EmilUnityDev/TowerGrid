using System;

namespace Game.Scripts.Gameplay.Skills
{
    public interface ISkill
    {
        string Name { get; }
        bool IsCharged { get; }
        bool CanUse { get; }
        void MarkCharged();
        void Use();
        event Action OnStateChanged;
        event Action OnUsed;
    }
}
