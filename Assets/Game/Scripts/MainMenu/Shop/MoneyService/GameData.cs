using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.GameRoot.Services.DataStorage.Json
{
    [Serializable]
    public class GameData : ISerializationCallbackReceiver
    {
        public bool IsSoundOn = true;
        public bool IsMusicOn = true;
        public bool IsHapticOn = true;
        public bool IsTutorialCompleted;
        public bool IsSlideshowShown;
        public int Money = 0;
        public int SelectedTrainIndex = 0;
        public int UnlockedTrainIndex = 0;
        public string DailyQuestDate = "";
        // public List<DailyQuestProgress> DailyQuests = new List<DailyQuestProgress>();
        public List<string> UnlockedCollections = new List<string>();

        public bool IsElementSkin2Purchased;
        public bool IsRobotSkin2Purchased;
        public int SelectedElementSkinIndex = 0;
        public int SelectedRobotSkinIndex = 0;

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            // if (DailyQuests == null)
            //     DailyQuests = new List<DailyQuestProgress>();
            if (UnlockedCollections == null)
                UnlockedCollections = new List<string>();
        }
    }
}