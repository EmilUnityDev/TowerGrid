using UnityEngine;
using UnityEditor;
using UnityEditor.UI; // Обязательно для ImageEditor
using UnityEngine.UI;

[CustomEditor(typeof(Image), true)]
[CanEditMultipleObjects] // Позволяет редактировать несколько объектов сразу
public class ExtendedImageEditor : ImageEditor {
    public override void OnInspectorGUI() {
        // Отрисовываем стандартный инспектор компонента Image
        base.OnInspectorGUI();

        // Добавляем небольшой отступ
        EditorGUILayout.Space();

        // Получаем ссылку на целевой компонент Image
        Image image = (Image)target;

        // Делаем кнопку неактивной, если в Image не вставлен спрайт
        EditorGUI.BeginDisabledGroup(image.sprite == null);

        // Создаем кнопку в инспекторе
        if (GUILayout.Button("Set Native Size & Rename", GUILayout.Height(30))) {
            // Регистрируем изменения для поддержки функции Undo (отмена действия)
            Undo.RecordObjects(targets, "Set Native Size and Rename");

            // Проходимся по всем выделенным объектам (на случай Multi-Selection)
            foreach (var t in targets) {
                Image img = (Image)t;
                if (img != null && img.sprite != null) {
                    // 1. Применяем родной размер спрайта
                    img.SetNativeSize();

                    // 2. Переименовываем GameObject в имя спрайта
                    img.gameObject.name = img.sprite.name;

                    // Маркируем объект как измененный, чтобы Unity сохранил изменения в сцене
                    EditorUtility.SetDirty(img.gameObject);
                }
            }
        }

        EditorGUI.EndDisabledGroup();
    }
}