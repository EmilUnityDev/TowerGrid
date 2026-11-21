using System.Collections.Generic;
using Game.Scripts.Gameplay.UI;

namespace Game.Scripts.Gameplay.Building
{
    public class ScoreBonusResult
    {
        public int TotalScore;
        public List<ScoreBonusData> Bonuses = new();
    }
}
