using UnityEngine;

namespace Game.Scripts.Gameplay.Core.Services
{
    public class TutorialGameService
    {
        public bool IsTutorialGame { get; private set; }

        public TutorialGameService()
        {
            IsTutorialGame = PlayerPrefs.GetInt("TutorialGameCompleted", 0) == 0;
        }

        public void CompleteTutorial()
        {
            PlayerPrefs.SetInt("TutorialGameCompleted", 1);
            PlayerPrefs.Save();
        }
    }
}
