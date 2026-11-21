using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlePassItemView : MonoBehaviour
{
    [Header("Item Image")]
    [SerializeField] private Image itemImage;
    [SerializeField] private Sprite lockedItemSprite;
    [SerializeField] private Sprite unlockedItemSprite;

    [Header("Reward")]
    [SerializeField] private TMP_Text rewardAmountText;

    [Header("Claim Button")]
    [SerializeField] private Button claimButton;
    [SerializeField] private Image claimButtonImage;
    [SerializeField] private TMP_Text claimButtonText;

    [Header("Button Sprites")]
    [SerializeField] private Sprite availableButtonSprite;
    [SerializeField] private Sprite claimedButtonSprite;
    [SerializeField] private Sprite lockedButtonSprite;

    [Header("Locked Button Content")]
    [SerializeField] private GameObject lockedObject;

    [Header("Button Labels")]
    [SerializeField] private string availableLabel = "claim";
    [SerializeField] private string claimedLabel = "done";

    private BattlePassService _battlePassService;
    private int _levelIndex;
    private bool _initialized;

    public int LevelIndex => _levelIndex;
    public int DisplayedLevel => _levelIndex + 1;

    private void Awake()
    {
        SetButtonContent(
            showText: false,
            showLockedObject: true);
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    public void Initialize(
        BattlePassService battlePassService,
        int levelIndex)
    {
        Unsubscribe();

        _battlePassService = battlePassService;
        _levelIndex = levelIndex;
        _initialized = true;

        if (claimButton != null)
        {
            claimButton.onClick.RemoveListener(ClaimReward);
            claimButton.onClick.AddListener(ClaimReward);
        }

        _battlePassService.OnBattlePassChanged += Refresh;

        Refresh();
    }

    public void Refresh()
    {
        if (!_initialized || _battlePassService == null)
            return;

        UpdateRewardText();
        UpdateItemImage();
        UpdateButtonState();
    }

    private void UpdateRewardText()
    {
        if (rewardAmountText == null)
            return;

        rewardAmountText.text =
            BattlePassService.ScoreReward.ToString();
    }

    private void UpdateItemImage()
    {
        if (itemImage == null)
            return;

        bool isLevelReached =
            _battlePassService.Points >=
            _battlePassService.GetRequiredPoints(_levelIndex);

        itemImage.sprite = isLevelReached
            ? unlockedItemSprite
            : lockedItemSprite;
    }

    private void UpdateButtonState()
    {
        BattlePassRewardState state =
            _battlePassService.GetRewardState(_levelIndex);

        switch (state)
        {
            case BattlePassRewardState.Locked:
                ShowLockedState();
                break;

            case BattlePassRewardState.Available:
                ShowAvailableState();
                break;

            case BattlePassRewardState.Claimed:
                ShowClaimedState();
                break;
        }
    }

    private void ClaimReward()
    {
        if (_battlePassService == null)
            return;

        _battlePassService.ClaimReward(_levelIndex);
    }

    private void ShowLockedState()
    {
        if (claimButtonImage != null)
            claimButtonImage.sprite = lockedButtonSprite;

        SetButtonContent(
            showText: false,
            showLockedObject: true);

        if (claimButton != null)
            claimButton.interactable = false;
    }

    private void ShowAvailableState()
    {
        if (claimButtonImage != null)
            claimButtonImage.sprite = availableButtonSprite;

        if (claimButtonText != null)
            claimButtonText.text = availableLabel;

        SetButtonContent(
            showText: true,
            showLockedObject: false);

        if (claimButton != null)
            claimButton.interactable = true;
    }

    private void ShowClaimedState()
    {
        if (claimButtonImage != null)
            claimButtonImage.sprite = claimedButtonSprite;

        if (claimButtonText != null)
            claimButtonText.text = claimedLabel;

        SetButtonContent(
            showText: true,
            showLockedObject: false);

        if (claimButton != null)
            claimButton.interactable = false;
    }

    private void SetButtonContent(
        bool showText,
        bool showLockedObject)
    {
        if (claimButtonText != null)
        {
            claimButtonText.gameObject.SetActive(
                showText);
        }

        if (lockedObject != null)
        {
            lockedObject.SetActive(
                showLockedObject);
        }
    }

    private void Unsubscribe()
    {
        if (claimButton != null)
            claimButton.onClick.RemoveListener(ClaimReward);

        if (_battlePassService != null)
            _battlePassService.OnBattlePassChanged -= Refresh;

        _initialized = false;
    }
}
