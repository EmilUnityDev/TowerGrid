using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

// Этот пустой класс-заглушка заставляет Unity корректно переопределить стандартный ImageEditor
[CustomEditor(typeof(Image), true)]
[CanEditMultipleObjects]
public class ImageEditorShortcut : ImageEditor {
    // Оставляем пустым, всю работу делает первый скрипт
}