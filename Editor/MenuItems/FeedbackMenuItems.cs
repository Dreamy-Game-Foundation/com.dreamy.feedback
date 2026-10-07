using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Feedback.Editor
{
    public static class FeedbackMenuItems
    {
        private const string MenuRoot = "Dreamy/Feedback/";
        private const string DefaultAssetFolder = "Assets";

        [MenuItem(MenuRoot + "Open Window")]
        public static void OpenWindow()
        {
            DreamyFeedbackWindow.Open();
        }

        [MenuItem(MenuRoot + "Create/VFX Database")]
        public static void CreateVfxDatabase()
        {
            CreateAsset<VfxDatabase>("VfxDatabase.asset");
        }

        [MenuItem(MenuRoot + "Create/Floating Text Database")]
        public static void CreateFloatingTextDatabase()
        {
            CreateAsset<FloatingTextDatabase>("FloatingTextDatabase.asset");
        }

        [MenuItem(MenuRoot + "Create/Camera Shake Database")]
        public static void CreateCameraShakeDatabase()
        {
            CreateAsset<CameraShakeDatabase>("CameraShakeDatabase.asset");
        }

        [MenuItem(MenuRoot + "Create/Feedback Sequence Database")]
        public static void CreateFeedbackSequenceDatabase()
        {
            CreateAsset<FeedbackSequenceDatabase>("FeedbackSequenceDatabase.asset");
        }

        [MenuItem(MenuRoot + "Validate All")]
        public static void ValidateAll()
        {
            ValidateAssets(FindAssets<VfxDatabase>(), "VFX Database", VfxDatabaseValidator.Validate);
            ValidateAssets(FindAssets<FloatingTextDatabase>(), "Floating Text Database", FloatingTextDatabaseValidator.Validate);
            ValidateAssets(FindAssets<CameraShakeDatabase>(), "Camera Shake Database", CameraShakeDatabaseValidator.Validate);
            ValidateAssets(FindAssets<FeedbackSequenceDatabase>(), "Feedback Sequence Database", FeedbackSequenceDatabaseValidator.Validate);
        }

        [MenuItem(MenuRoot + "Generate IDs")]
        public static void GenerateIds()
        {
            FeedbackIdsGenerator.GenerateSelected();
        }

        private static void CreateAsset<T>(string fileName) where T : ScriptableObject
        {
            var folder = GetSelectedFolder();
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{fileName}");
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static string GetSelectedFolder()
        {
            if (Selection.activeObject)
            {
                var path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        return path;
                    }

                    var directory = System.IO.Path.GetDirectoryName(path);
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        return directory.Replace("\\", "/");
                    }
                }
            }

            return DefaultAssetFolder;
        }

        private static List<T> FindAssets<T>() where T : Object
        {
            var assets = new List<T>();
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }

        private static void ValidateAssets<T>(IReadOnlyList<T> assets, string label, System.Func<T, List<FeedbackValidationIssue>> validator) where T : Object
        {
            if (assets.Count == 0)
            {
                Debug.Log($"{label}: no assets found.");
                return;
            }

            for (var i = 0; i < assets.Count; i++)
            {
                var issues = validator(assets[i]);
                FeedbackValidatorUtility.LogIssues($"{label} '{assets[i].name}'", issues);
            }
        }
    }
}
