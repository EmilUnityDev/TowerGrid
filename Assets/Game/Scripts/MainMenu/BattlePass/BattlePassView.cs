using UnityEngine;
using VContainer;

public class BattlePassView : MonoBehaviour
{
    [Header("Battle Pass Items")]
    [SerializeField] private BattlePassItemView[] items;

    private BattlePassService _battlePassService;
    private bool _initialized;

    [Inject]
    public void Construct(BattlePassService battlePassService)
    {
        _battlePassService = battlePassService;
        Initialize();
    }

    private void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (!_initialized)
            return;

        RefreshItems();
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        if (_battlePassService == null)
        {
            Debug.LogError(
                "BattlePassView: BattlePassService не был внедрён через VContainer.",
                this);

            return;
        }

        int itemsToInitialize = Mathf.Min(
            items.Length,
            BattlePassService.LevelsCount);

        for (int i = 0; i < itemsToInitialize; i++)
        {
            if (items[i] == null)
                continue;

            items[i].Initialize(
                _battlePassService,
                i);
        }

        if (items.Length != BattlePassService.LevelsCount)
        {
            Debug.LogWarning(
                $"BattlePassView: в массиве находится {items.Length} элементов, " +
                $"но должно быть {BattlePassService.LevelsCount} уровней.",
                this);
        }

        _initialized = true;

        RefreshItems();
    }

    private void RefreshItems()
    {
        if (items == null)
            return;

        foreach (BattlePassItemView item in items)
        {
            if (item == null)
                continue;

            item.Refresh();
        }
    }
}