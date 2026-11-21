using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(SymmetricImage))]
public class SymmetricImageEditor : ImageEditor {

    SerializedProperty sourceSide;

    protected override void OnEnable() {
        base.OnEnable();
        sourceSide = serializedObject.FindProperty("sourceSide");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        // стандартный инспектор Image
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(sourceSide);

        serializedObject.ApplyModifiedProperties();
    }
}