using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Feedback.Editor
{
    public static class UiVfxStarterBuilder
    {
        [MenuItem("Tools/Dreamy/Feedback/Create UI VFX Starter")]
        public static void Create()
        {
            CreateAt(AssetDatabase.GenerateUniqueAssetPath("Assets/DreamyUiVfx"));
        }

        /// <summary>Creates a new folder only; existing assets are never overwritten.</summary>
        public static void CreateAt(string folder)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before creating UI VFX assets.");
            if (!folder.StartsWith("Assets/", StringComparison.Ordinal) || Directory.Exists(folder))
                throw new InvalidOperationException("Choose a new folder under Assets.");
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
            var go = new GameObject("UiRewardBurst", typeof(RectTransform), typeof(UiBurstVfxInstance));
            try
            {
                go.SetActive(false);
                var prefab = PrefabUtility.SaveAsPrefabAsset(go, folder + "/UiRewardBurst.prefab");
                var effect = ScriptableObject.CreateInstance<VfxFeedbackAsset>();
                effect.Configure("effect.ui-reward-burst", new VfxFeedbackSettings
                { Prefab = prefab, Space = VfxSpace.UI, Scale = 1, MaximumDuration = .6f, PrewarmCount = 2 });
                AssetDatabase.CreateAsset(effect, folder + "/UiRewardBurst.asset");
                Selection.activeObject = effect;
                Debug.Log("UI VFX created in " + folder + ". Assign the effect to a FeedbackPlayer under your Canvas and bind its Host.");
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}
