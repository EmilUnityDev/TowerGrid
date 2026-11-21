using System.Collections.Generic;
using UnityEngine;

public class DailyQuestPanelView : MonoBehaviour
{
    [SerializeField] private DailyQuestManager dailyQuestManager;

    [Header("Exactly 3 quest containers")]
    [SerializeField] private List<DailyQuestItemView> questItems = new();

    private void OnEnable()
    {
        if (dailyQuestManager == null)
            return;

        dailyQuestManager.OnQuestsChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (dailyQuestManager == null)
            return;

        dailyQuestManager.OnQuestsChanged -= Refresh;
    }

    private void Refresh()
    {
        IReadOnlyList<ActiveDailyQuestData> activeQuests =
            dailyQuestManager.ActiveQuests;

        for (int i = 0; i < questItems.Count; i++)
        {
            DailyQuestItemView item = questItems[i];

            if (item == null)
                continue;

            bool hasQuest =
                activeQuests != null &&
                i < activeQuests.Count;

            item.gameObject.SetActive(hasQuest);

            if (!hasQuest)
                continue;

            item.Bind(
                dailyQuestManager,
                activeQuests[i]
            );
        }
    }
}