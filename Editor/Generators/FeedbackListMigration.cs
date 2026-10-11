using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Dreamy.Feedback.Editor
{
    public static class FeedbackListMigration
    {
        public static string Preview(FeedbackDefinition definition)
        {
            var bindings = new List<FeedbackStepBinding>(); var created = new List<FeedbackAsset>();
            FeedbackDefinition temporary = null;
            try { temporary = Build(definition.Root, definition.Id, "", bindings, created, false); return "List conversion: " + created.Count + " nested definitions, " + bindings.Count + " runtime bindings. No assets written."; }
            finally { if (temporary) UnityEngine.Object.DestroyImmediate(temporary); foreach (var asset in created) UnityEngine.Object.DestroyImmediate(asset); }
        }
        public static FeedbackStepBinding[] Convert(FeedbackDefinition definition, bool exportRuntimeBindings = false)
        {
            if (!definition.IsLegacy) return Array.Empty<FeedbackStepBinding>();
            string path = AssetDatabase.GetAssetPath(definition);
            if (!path.StartsWith("Assets/") || EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Migration requires a saved project asset in Edit Mode.");
            var bindings = new List<FeedbackStepBinding>(); var created = new List<FeedbackAsset>();
            FeedbackDefinition converted = null;
            try
            {
                converted = Build(definition.Root, definition.Id, "", bindings, created, !exportRuntimeBindings);
                var mode = converted.PlayMode; var savedSteps = new List<FeedbackStep>(converted.Effects);
                if (savedSteps.Count == 0) throw new InvalidOperationException("Migration produced an empty list; source kept intact.");
                string folder = Path.GetDirectoryName(path).Replace('\\','/') + "/Groups";
                if (created.Count > 0) { Directory.CreateDirectory(folder); AssetDatabase.Refresh(); }
                foreach (var asset in created) AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(folder + "/" + asset.name + ".asset"));
                Undo.RecordObject(definition, "Convert feedback to list");
                definition.Configure(definition.Id, mode, savedSteps, definition.UnscaledTime);
                EditorUtility.SetDirty(definition); AssetDatabase.SaveAssetIfDirty(definition);
                foreach (var asset in created) { EditorUtility.SetDirty(asset); AssetDatabase.SaveAssetIfDirty(asset); }
                return bindings.ToArray();
            }
            finally
            {
                if (converted && !AssetDatabase.Contains(converted)) UnityEngine.Object.DestroyImmediate(converted);
                foreach (var asset in created) if (asset && !AssetDatabase.Contains(asset)) UnityEngine.Object.DestroyImmediate(asset);
            }
        }
        private static FeedbackDefinition Build(FeedbackNode root, string id, string prefix, List<FeedbackStepBinding> bindings, List<FeedbackAsset> created, bool bake)
        {
            var steps = new List<FeedbackStep>();
            bool group = root.Type == FeedbackNodeType.Sequence || root.Type == FeedbackNodeType.Parallel;
            var nodes = group ? root.Children : new List<FeedbackNode> { root };
            float delay = 0;
            foreach (var node in nodes)
            {
                if (node.Type == FeedbackNodeType.Delay)
                { if (!group || root.Type != FeedbackNodeType.Sequence) throw new InvalidOperationException("Standalone/parallel Delay needs an explicit timing redesign."); delay += node.Duration; continue; }
                string key = prefix.Length == 0 ? steps.Count.ToString() : prefix + "/" + steps.Count;
                FeedbackAsset asset;
                if (node.Type == FeedbackNodeType.Sequence || node.Type == FeedbackNodeType.Parallel)
                { asset = Build(node, id + ".group" + steps.Count, key, bindings, created, bake); created.Add(asset); }
                else
                {
                    if (!node.Effect) throw new InvalidOperationException("Convert inline effects to SO first using legacy migration.");
                    asset = node.Effect;
                    if (node.Overrides.Fields != FeedbackOverrideFields.None)
                    {
                        if (bake)
                        { var resolved = FeedbackGraphResolver.Resolve(node); var effect = FeedbackDefinitionMigration.CreateEffect(resolved, id + ".preset" + steps.Count, resolved.VfxSettings.Prefab, resolved.TextSettings); effect.name = id.Replace('.', '-') + "-" + steps.Count; created.Add(effect); asset = effect; }
                        else bindings.Add(new FeedbackStepBinding { Path = key, Overrides = node.Overrides });
                    }
                }
                steps.Add(new FeedbackStep(asset, delay, node.Required)); delay = 0;
            }
            if (delay > 0) throw new InvalidOperationException("Trailing delay requires an explicit timing redesign; migration leaves source intact.");
            var result = ScriptableObject.CreateInstance<FeedbackDefinition>(); result.name = id.Replace('.', '-');
            result.Configure(id, group && root.Type == FeedbackNodeType.Parallel ? FeedbackPlayMode.Parallel : FeedbackPlayMode.Sequence, steps);
            return result;
        }
    }
    public sealed class FeedbackAuthoringMigrationWindow : EditorWindow
    {
        private FeedbackDefinition definition;
        private FeedbackHost host;
        private string status;
        public static void Open(FeedbackDefinition value) { var window = GetWindow<FeedbackAuthoringMigrationWindow>("Feedback migration"); window.definition = value; }
        private void OnGUI()
        {
            definition = (FeedbackDefinition)EditorGUILayout.ObjectField("Definition", definition, typeof(FeedbackDefinition), false);
            if (!definition || !definition.IsLegacy) return;
            host = (FeedbackHost)EditorGUILayout.ObjectField("Legacy ID lookup rig", host, typeof(FeedbackHost), true);
            if (GUILayout.Button("Preview (read only)")) Run(() => { status = FeedbackDefinitionMigration.Preview(definition, host) + "\n" + FeedbackListMigration.Preview(definition); });
            EditorGUILayout.HelpBox("Convert bakes differing legacy settings into independent presets. Existing GUIDs remain; newly created assets remain after Undo.", MessageType.Info);
            if (GUILayout.Button("Convert legacy to list")) Run(() => { FeedbackDefinitionMigration.Migrate(definition, host); FeedbackListMigration.Convert(definition); status = "Converted."; });
            if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, MessageType.Info);
        }
        private void Run(Action action) { try { action(); } catch (Exception e) { status = e.Message; } }
    }
}
