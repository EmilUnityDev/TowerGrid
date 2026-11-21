using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SymmetricImageConverter : Editor {
    // Добавляет пункт в контекстное меню компонента Image в Инспекторе
    [MenuItem("CONTEXT/Image/Convert to Symmetric (Left Half)")]
    public static void ConvertImageToSymmetric(MenuCommand command) {
        // 1. Получаем компонент Image, на который кликнули
        Image originalImage = (Image)command.context;
        GameObject go = originalImage.gameObject;

        if (originalImage.sprite == null) {
            EditorUtility.DisplayDialog("Ошибка", "У этого Image нет назначенного спрайта!", "Ок");
            return;
        }

        Sprite origSprite = originalImage.sprite;
        Texture2D originalTex = origSprite.texture;

        // 2. Проверяем Read/Write текстуры
        try {
            originalTex.GetPixels(0, 0, 1, 1);
        } catch (UnityException) {
            EditorUtility.DisplayDialog("Ошибка", $"У текстуры '{originalTex.name}' должна быть включена опция 'Read/Write' в настройках импорта!", "Ок");
            return;
        }

        // Регистрируем операцию Undo, чтобы можно было отменить замену компонента
        Undo.RegisterCompleteObjectUndo(go, "Convert to Symmetric Image");

        // 3. Обрезаем текстуру (берем левую часть исходного спрайта)
        // Считаем относительно самого спрайта, на случай если это Атлас/Multiple
        Rect spriteRect = origSprite.rect;
        int x = (int)spriteRect.x;
        int y = (int)spriteRect.y;
        int halfWidth = (int)(spriteRect.width / 2);
        int height = (int)spriteRect.height;

        Color[] leftPixels = originalTex.GetPixels(x, y, halfWidth, height);

        // Путь для сохранения новой текстуры
        string origPath = AssetDatabase.GetAssetPath(originalTex);
        string dir = Path.GetDirectoryName(origPath);
        string name = Path.GetFileNameWithoutExtension(origPath);
        string ext = Path.GetExtension(origPath);
        string newPath = Path.Combine(dir, $"{name}_Half{ext}");

        // Создаем и сохраняем половинку
        Texture2D croppedTex = new Texture2D(halfWidth, height, TextureFormat.RGBA32, false);
        croppedTex.SetPixels(leftPixels);
        croppedTex.Apply();
        croppedTex.SetPixels(leftPixels);
        croppedTex.Apply();

        byte[] bytes = newPath.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) ? croppedTex.EncodeToJPG() : croppedTex.EncodeToPNG();
        File.WriteAllBytes(newPath, bytes);
        DestroyImmediate(croppedTex);

        // Импортируем новый ассет в проект
        AssetDatabase.ImportAsset(newPath, ImportAssetOptions.ForceUpdate);

        // Настраиваем импортированный файл как Sprite (UI)
        TextureImporter importer = AssetImporter.GetAtPath(newPath) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite) {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }

        // Загружаем созданный спрайт
        Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(newPath);

        // 4. Магия замены компонентов "на лету"
        // Сохраняем настройки старого Image
        Color savedColor = originalImage.color;
        Material savedMaterial = originalImage.material;
        bool savedRaycast = originalImage.raycastTarget;

        // Удаляем старый Image и добавляем SymmetricImage
        Undo.DestroyObjectImmediate(originalImage);
        SymmetricImage symImage = go.AddComponent<SymmetricImage>();

        // Восстанавливаем настройки и задаем новые
        symImage.sprite = newSprite;
        symImage.Side = SymmetricImage.SourceSide.Left;
        symImage.color = savedColor;
        symImage.material = savedMaterial;
        symImage.raycastTarget = savedRaycast;

        // Отмечаем сцену как измененную
        EditorUtility.SetDirty(go);

        Debug.Log($"Компонент на объекте '{go.name}' успешно заменен на SymmetricImage, а спрайт обрезан!");
    }
}