using System.IO;
using Game.Scripts.Gameplay.Building.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Scripts.Editor
{
    public class BuildingDataGenerator : EditorWindow
    {
        private DefaultAsset _spriteFolder;
        private string _outputFolder = "Assets/Game/Data/Buildings/Generated";
        private BuildingType _defaultType = BuildingType.Living;

        private const int SmallWidth = 104;
        private const int MediumWidth = 208;
        private const int LargeWidth = 418;

        [MenuItem("Tools/Game/Building Data Generator")]
        public static void ShowWindow()
        {
            GetWindow<BuildingDataGenerator>("Building Data Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Генератор BuildingData из спрайтов", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _spriteFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Папка со спрайтами", _spriteFolder, typeof(DefaultAsset), false);

            _outputFolder = EditorGUILayout.TextField("Папка для SO", _outputFolder);
            _defaultType = (BuildingType)EditorGUILayout.EnumPopup("Тип зданий", _defaultType);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                $"Размер определяется по ширине спрайта:\n" +
                $"  {SmallWidth}px → Small\n" +
                $"  {MediumWidth}px → Medium\n" +
                $"  {LargeWidth}px → Large",
                MessageType.Info);

            EditorGUILayout.Space();

            GUI.enabled = _spriteFolder != null;
            if (GUILayout.Button("Сгенерировать", GUILayout.Height(30)))
            {
                Generate();
            }
            GUI.enabled = true;
        }

        private void Generate()
        {
            var folderPath = AssetDatabase.GetAssetPath(_spriteFolder);
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                EditorUtility.DisplayDialog("Ошибка", "Выберите корректную папку со спрайтами.", "OK");
                return;
            }

            if (!AssetDatabase.IsValidFolder(_outputFolder))
                CreateFolderRecursive(_outputFolder);

            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
            int created = 0;
            int skipped = 0;

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite == null) continue;

                var size = DetectSize(sprite);
                if (!size.HasValue)
                {
                    Debug.LogWarning($"[BuildingDataGenerator] Не удалось определить размер для {sprite.name} " +
                                     $"(ширина: {sprite.rect.width}px). Пропущен.");
                    skipped++;
                    continue;
                }

                var soPath = Path.Combine(_outputFolder, $"{sprite.name}.asset");
                soPath = AssetDatabase.GenerateUniqueAssetPath(soPath);

                var buildingData = CreateInstance<BuildingData>();
                buildingData.SetData(_defaultType, size.Value, sprite);

                AssetDatabase.CreateAsset(buildingData, soPath);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Готово",
                $"Создано: {created}\nПропущено: {skipped}\nПуть: {_outputFolder}", "OK");
        }

        private BuildingSize? DetectSize(Sprite sprite)
        {
            int width = Mathf.RoundToInt(sprite.rect.width);

            const int tolerance = 10;

            if (Mathf.Abs(width - SmallWidth) <= tolerance)
                return BuildingSize.Small;
            if (Mathf.Abs(width - MediumWidth) <= tolerance)
                return BuildingSize.Medium;
            if (Mathf.Abs(width - LargeWidth) <= tolerance)
                return BuildingSize.Large;

            return null;
        }

        private static void CreateFolderRecursive(string path)
        {
            var parts = path.Split('/');
            var current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
