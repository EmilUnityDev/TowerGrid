using UnityEngine;
using VContainer;

public class ShopBackgroundView : MonoBehaviour {
    [Header("Renderers")]
    [SerializeField] private SpriteRenderer[] backgroundRenderers;

    [Header("Particle Systems")]
    [SerializeField] private GameObject particleSystem1;
    [SerializeField] private GameObject particleSystem2;
    [SerializeField] private GameObject particleSystem3;
    [SerializeField] private GameObject particleSystem4;

    [Header("Background Item IDs")]
    [SerializeField] private string particle1ItemId = "bg_forest";
    [SerializeField] private string particle2ItemId = "bg_snow";
    [SerializeField] private string particle3ItemId = "bg_volcano";
    [SerializeField] private string particle4ItemId = "bg_space";

    private ShopAppearanceService _appearanceService;

    [Inject]
    public void Construct(ShopAppearanceService appearanceService) {
        _appearanceService = appearanceService;
    }

    private void Start() {
        DisableAllParticles();
        ApplyBackground();
    }

    public void ApplyBackground() {
        if (_appearanceService == null) {
            Debug.LogError("[ShopBackgroundView] ShopAppearanceService не внедрён.", this);
            return;
        }

        Sprite selectedSprite = _appearanceService.GetSelectedBackgroundSprite();

        if (selectedSprite == null) {
            Debug.LogWarning("[ShopBackgroundView] Выбранный спрайт фона не найден.", this);
            DisableAllParticles();
            return;
        }

        if (backgroundRenderers != null && backgroundRenderers.Length > 0) {
            foreach (SpriteRenderer spriteRenderer in backgroundRenderers) {
                if (spriteRenderer != null) {
                    spriteRenderer.sprite = selectedSprite;
                }
            }
        }

        UpdateParticles();
    }

    private void UpdateParticles() {
        DisableAllParticles();

        string currentItemId = _appearanceService.GetSelectedBackgroundItemId();

        if (string.IsNullOrEmpty(currentItemId))
            return;

        if (currentItemId == particle1ItemId && particleSystem1 != null) {
            particleSystem1.SetActive(true);
        } else if (currentItemId == particle2ItemId && particleSystem2 != null) {
            particleSystem2.SetActive(true);
        } else if (currentItemId == particle3ItemId && particleSystem3 != null) {
            particleSystem3.SetActive(true);
        } else if (currentItemId == particle4ItemId && particleSystem4 != null) {
            particleSystem4.SetActive(true);
        }
    }

    private void DisableAllParticles() {
        if (particleSystem1 != null) particleSystem1.SetActive(false);
        if (particleSystem2 != null) particleSystem2.SetActive(false);
        if (particleSystem3 != null) particleSystem3.SetActive(false);
        if (particleSystem4 != null) particleSystem4.SetActive(false);
    }
}