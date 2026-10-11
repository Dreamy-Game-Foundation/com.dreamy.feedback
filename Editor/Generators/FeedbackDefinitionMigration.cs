using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Feedback.Editor
{
    /// <summary>Explicit single-definition migration. Preview performs the same reference checks without writing assets.</summary>
    public static class FeedbackDefinitionMigration
    {
        private sealed class Step
        {
            public FeedbackNode Node;
            public FeedbackEffectAsset Asset;
        }
        public static string Preview(FeedbackDefinition definition, FeedbackHost host)
        {
            var steps = Prepare(definition, host);
            try { return $"Ready: {steps.Count} inline effects will become ScriptableObjects. Existing SO steps and group timing stay intact."; }
            finally { foreach (var step in steps) UnityEngine.Object.DestroyImmediate(step.Asset); }
        }
        public static int Migrate(FeedbackDefinition definition, FeedbackHost host)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before migrating assets.");
            string source = AssetDatabase.GetAssetPath(definition);
            if (string.IsNullOrEmpty(source) || !source.StartsWith("Assets/"))
                throw new InvalidOperationException("Save a project-owned Definition under Assets first.");
            var steps = Prepare(definition, host);
            if (steps.Count == 0) return 0;
            string folder = AssetDatabase.GenerateUniqueAssetPath(Path.GetDirectoryName(source).Replace('\\','/') + "/" + definition.name + " Effects");
            try
            {
                Directory.CreateDirectory(folder);
                AssetDatabase.Refresh();
                for (int i = 0; i < steps.Count; i++)
                    AssetDatabase.CreateAsset(steps[i].Asset, folder + "/" + (i + 1).ToString("D2") + "-" + steps[i].Asset.Type + ".asset");
                Undo.RecordObject(definition, "Migrate feedback definition to SO");
                foreach (var step in steps)
                {
                    step.Node.Effect = step.Asset;
                    step.Node.Overrides = default;
                }
                EditorUtility.SetDirty(definition);
                AssetDatabase.SaveAssetIfDirty(definition);
                foreach (var step in steps) AssetDatabase.SaveAssetIfDirty(step.Asset);
                return steps.Count;
            }
            catch
            {
                foreach (var step in steps) if (step.Asset && !AssetDatabase.Contains(step.Asset)) UnityEngine.Object.DestroyImmediate(step.Asset);
                throw;
            }
        }
        private static List<Step> Prepare(FeedbackDefinition definition, FeedbackHost host)
        {
            string error = FeedbackGraphValidation.Validate(definition);
            if (error != null) throw new InvalidOperationException(error);
            var steps = new List<Step>();
            VfxDatabase vfx = null; FloatingTextDatabase text = null;
            if (host)
            {
                var data = new SerializedObject(host);
                vfx = data.FindProperty("vfxDatabase").objectReferenceValue as VfxDatabase;
                text = data.FindProperty("floatingTextDatabase").objectReferenceValue as FloatingTextDatabase;
            }
            try { Visit(definition.Root); return steps; }
            catch { foreach (var step in steps) UnityEngine.Object.DestroyImmediate(step.Asset); throw; }
            void Visit(FeedbackNode node)
            {
                if (node.Effect) return;
                if (node.Type == FeedbackNodeType.Sequence || node.Type == FeedbackNodeType.Parallel)
                { foreach (var child in node.Children) Visit(child); return; }
                if (node.Type == FeedbackNodeType.Delay) return;
                GameObject prefab = null; FloatingTextFeedbackSettings textSettings = default;
                if (node.Type == FeedbackNodeType.Vfx)
                {
                    if (!vfx || !vfx.TryGet(node.Id, out var entry) || !entry.Prefab)
                        throw new InvalidOperationException("Select a rig with a valid VFX prefab for " + node.Id);
                    prefab = entry.Prefab;
                }
                if (node.Type == FeedbackNodeType.FloatingText)
                {
                    if (!text || !text.TryGet(node.Id, out var entry) || !entry.Prefab)
                        throw new InvalidOperationException("Select a rig with a valid text prefab for " + node.Id);
                    textSettings = new FloatingTextFeedbackSettings { Prefab = entry.Prefab.gameObject, Color = entry.Color,
                        Duration = entry.Duration, MoveOffset = entry.MoveOffset, StartScale = entry.StartScale, EndScale = entry.EndScale,
                        PrewarmCount = entry.PrewarmCount };
                }
                var asset = CreateEffect(node, definition.Id + "." + steps.Count, prefab, textSettings);
                steps.Add(new Step { Node = node, Asset = asset });
            }
        }
        public static FeedbackEffectAsset CreateEffect(FeedbackNode node, string id, GameObject vfxPrefab = null,
            FloatingTextFeedbackSettings textSettings = default)
        {
            switch (node.Type)
            {
                case FeedbackNodeType.Vfx:
                    var vfx = ScriptableObject.CreateInstance<VfxFeedbackAsset>();
                    vfx.Configure(id, node.DirectAsset ? node.VfxSettings : new VfxFeedbackSettings { Prefab = vfxPrefab, MaximumDuration = node.Duration, Scale = 1 }); return vfx;
                case FeedbackNodeType.FloatingText:
                    var text = ScriptableObject.CreateInstance<FloatingTextFeedbackAsset>(); text.Configure(id, textSettings, node.Text); return text;
                case FeedbackNodeType.IconFly:
                    var icons = ScriptableObject.CreateInstance<IconFlyFeedbackAsset>(); icons.Configure(id, node.IconFly); return icons;
                case FeedbackNodeType.UiPunch:
                    var punch = ScriptableObject.CreateInstance<UiPunchFeedbackAsset>(); punch.Configure(id, node.Punch); return punch;
                case FeedbackNodeType.CameraShake:
                    var shake = ScriptableObject.CreateInstance<CameraShakeFeedbackAsset>(); shake.Configure(id, node.Shake); return shake;
                case FeedbackNodeType.ScreenFlash:
                    var flash = ScriptableObject.CreateInstance<ScreenFlashFeedbackAsset>(); flash.Configure(id, node.Flash); return flash;
                case FeedbackNodeType.Haptic:
                    var haptic = ScriptableObject.CreateInstance<HapticFeedbackAsset>(); haptic.Configure(id, node.Haptic); return haptic;
                default: throw new ArgumentException("Groups and Delay do not need separate effect assets.");
            }
        }
    }
}
