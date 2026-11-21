// using UnityEngine;
// using VContainer;
//
// public class GameplayBackgroundView : MonoBehaviour
// {
//     [SerializeField] private SpriteRenderer[] backgroundRenderers;
//
//     private ShopAppearanceService _appearanceService;
//
//     [Inject]
//     public void Construct(
//         ShopAppearanceService appearanceService)
//     {
//         _appearanceService =
//             appearanceService;
//     }
//
//     private void Start()
//     {
//         ApplyBackground();
//     }
//
//     public void ApplyBackground()
//     {
//         if (_appearanceService == null)
//             return;
//
//         Sprite backgroundSprite =
//             _appearanceService
//                 .GetSelectedBackgroundSprite();
//
//         if (backgroundSprite == null)
//             return;
//
//         foreach (SpriteRenderer renderer in backgroundRenderers)
//         {
//             if (renderer == null)
//                 continue;
//
//             renderer.sprite =
//                 backgroundSprite;
//         }
//     }
// }