using System;
using System.Collections.Generic;

namespace Game.Scripts.Gameplay.Skills
{
    public class SkillService
    {
        private const int ChargeThreshold = 8;

        private int _currentCharge;
        private readonly Queue<ISkill> _chargeQueue = new();

        public int CurrentCharge => _currentCharge;
        public int MaxCharge => ChargeThreshold;

        public DraftRefreshSkill DraftRefresh { get; }
        public RezoneSkill Rezone { get; }
        public DemolitionSkill Demolition { get; }

        public event Action OnChargeChanged;

        public SkillService(DraftRefreshSkill draftRefresh, RezoneSkill rezone, DemolitionSkill demolition)
        {
            DraftRefresh = draftRefresh;
            Rezone = rezone;
            Demolition = demolition;

            _chargeQueue.Enqueue(draftRefresh);
            _chargeQueue.Enqueue(rezone);
            _chargeQueue.Enqueue(demolition);

            draftRefresh.OnUsed += () => EnqueueForRecharge(draftRefresh);
            rezone.OnUsed += () => EnqueueForRecharge(rezone);
            demolition.OnUsed += () => EnqueueForRecharge(demolition);
        }

        public void AddChargeForPlacement(int slotWidth, bool floorCompleted)
        {
            int units = slotWidth >= 4 ? 2 : 1;
            if (floorCompleted) units += 2;

            AddRawCharge(units);
        }

        public void AddChargeForTutorial(int segments)
        {
            AddRawCharge(segments);
        }

        private void AddRawCharge(int units)
        {
            if (_chargeQueue.Count == 0)
                return;

            _currentCharge += units;

            while (_currentCharge >= ChargeThreshold && _chargeQueue.Count > 0)
            {
                _currentCharge -= ChargeThreshold;
                var skill = _chargeQueue.Dequeue();
                skill.MarkCharged();
            }

            if (_chargeQueue.Count == 0)
                _currentCharge = 0;

            OnChargeChanged?.Invoke();
        }

        private void EnqueueForRecharge(ISkill skill)
        {
            _chargeQueue.Enqueue(skill);
            OnChargeChanged?.Invoke();
        }

        private bool QueueContains(ISkill skill)
        {
            foreach (var s in _chargeQueue)
                if (s == skill) return true;
            return false;
        }
    }
}
