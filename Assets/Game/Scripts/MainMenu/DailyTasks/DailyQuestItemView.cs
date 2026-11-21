using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyQuestItemView : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text rewardAmountText;
    [SerializeField] private TMP_Text claimButtonText;

    [Header("Progress")]
    [SerializeField] private Image fillImage;

    [Header("Claim button")]
    [SerializeField] private Button claimButton;
    [SerializeField] private Image claimButtonImage;

    [Header("Button sprites")]
    [SerializeField] private Sprite progressButtonSprite;
    [SerializeField] private Sprite availableButtonSprite;
    [SerializeField] private Sprite claimedButtonSprite;

    [Header("Button texts")]
    [SerializeField] private string progressButtonLabel = "claim";
    [SerializeField] private string availableButtonLabel = "claim";
    [SerializeField] private string claimedButtonLabel = "done";

    private DailyQuestManager _manager;
    private string _questId;

    private void OnDestroy()
    {
        Unsubscribe();
    }

    public void Bind(
        DailyQuestManager manager,
        ActiveDailyQuestData activeQuest
    )
    {
        Unsubscribe();

        _manager = manager;
        _questId = activeQuest.questId;

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(ClaimReward);

        _manager.OnQuestProgressChanged += HandleQuestProgressChanged;

        Refresh();
    }

    private void Unsubscribe()
    {
        if (_manager != null)
        {
            _manager.OnQuestProgressChanged -=
                HandleQuestProgressChanged;
        }
    }

    private void HandleQuestProgressChanged(string changedQuestId)
    {
        if (changedQuestId != _questId)
            return;

        Refresh();
    }

    private void Refresh()
    {
        if (_manager == null)
            return;

        ActiveDailyQuestData activeQuest =
            _manager.GetActiveQuest(_questId);

        DailyQuestDefinition definition =
            _manager.GetDefinition(_questId);

        if (activeQuest == null || definition == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        int target = Mathf.Max(1, definition.targetValue);
        int progress = Mathf.Clamp(activeQuest.progress, 0, target);

        float normalizedProgress =
            Mathf.Clamp01((float)progress / target);

        if (titleText != null)
            titleText.text = definition.title;

        if (fillImage != null)
            fillImage.fillAmount = normalizedProgress;

        if (progressText != null)
        {
            progressText.text = $"{progress}/{target}";
        }

        if (rewardAmountText != null)
        {
            rewardAmountText.text =
                definition.softCurrencyReward.ToString();
        }

        bool completed = progress >= target;
        bool claimed = activeQuest.rewardClaimed;

        if (claimed)
        {
            ApplyClaimedState();
        }
        else if (completed)
        {
            ApplyAvailableState();
        }
        else
        {
            ApplyProgressState();
        }
    }

    private void ApplyProgressState()
    {
        claimButton.interactable = false;

        if (claimButtonImage != null &&
            progressButtonSprite != null)
        {
            claimButtonImage.sprite = progressButtonSprite;
            claimButtonImage.SetNativeSize();
        }

        if (claimButtonText != null)
            claimButtonText.text = progressButtonLabel;
    }

    private void ApplyAvailableState()
    {
        claimButton.interactable = true;

        if (claimButtonImage != null &&
            availableButtonSprite != null)
        {
            claimButtonImage.sprite = availableButtonSprite;
            claimButtonImage.SetNativeSize();

        }

        if (claimButtonText != null)
            claimButtonText.text = availableButtonLabel;
    }

    private void ApplyClaimedState()
    {
        claimButton.interactable = false;

        if (claimButtonImage != null &&
            claimedButtonSprite != null)
        {
            claimButtonImage.sprite = claimedButtonSprite;
        }

        if (claimButtonText != null)
            claimButtonText.text = claimedButtonLabel;
    }

    private void ClaimReward()
    {
        if (_manager == null ||
            string.IsNullOrEmpty(_questId))
        {
            return;
        }

        _manager.ClaimReward(_questId);
    }
}