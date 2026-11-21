using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SpriteToImageConverter : EditorWindow {
    [MenuItem("Tools/Replace Sprites with Images (In-Place)")]
    private static void ReplaceInPlace() {
        GameObject selected = Selection.activeGameObject;

        if (selected == null) {
            EditorUtility.DisplayDialog("Ошибка", "Выберите объект в Hierarchy!", "Ок");
            return;
        }

        // Регистрируем операцию в системе Undo
        Undo.RegisterFullObjectHierarchyUndo(selected, "Replace Sprites with Images");

        int convertedCount = 0;
        ConvertNodeInPlace(selected, ref convertedCount);

        Debug.Log($"<color=green>Готово!</color> Успешно конвертировано объектов: {convertedCount}.");
    }

    private static void ConvertNodeInPlace(GameObject targetGo, ref int count) {
        if (targetGo == null) return;

        // 1. Сначала собираем список всех детей, используя GameObject, 
        // чтобы избежать MissingReferenceException при уничтожении Transform компонентов.
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in targetGo.transform) {
            children.Add(child.gameObject);
        }

        // 2. Рекурсивно обрабатываем детей
        foreach (GameObject child in children) {
            ConvertNodeInPlace(child, ref count);
        }

        // 3. Работаем с текущим объектом
        SpriteRenderer spriteRenderer = targetGo.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            // Сохраняем данные спрайта
            Sprite sprite = spriteRenderer.sprite;
            Color color = spriteRenderer.color;
            Material material = spriteRenderer.material;

            // Удаляем SpriteRenderer
            Undo.DestroyObjectImmediate(spriteRenderer);

            // Безопасно получаем или добавляем RectTransform.
            // При добавлении RectTransform старый Transform уничтожится автоматически,
            // но так как мы ссылаемся на targetGo (GameObject), у нас ничего не сломается.
            RectTransform rectTransform = targetGo.GetComponent<RectTransform>();
            if (rectTransform == null) {
                rectTransform = Undo.AddComponent<RectTransform>(targetGo);
            }

            // Добавляем Image и возвращаем данные
            Image image = Undo.AddComponent<Image>(targetGo);
            image.sprite = sprite;
            image.color = color;
            image.material = material;

            // Восстанавливаем родной размер спрайта
            image.SetNativeSize();

            count++;
        }
    }
}