using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Feedback.Editor
{
    public abstract class FeedbackDatabaseEditor : UnityEditor.Editor
    {
        protected abstract string ListName { get; }
        protected abstract string Hint { get; }
        protected abstract List<FeedbackValidationIssue> Validate();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(Hint, MessageType.Info);
            var entries = serializedObject.FindProperty(ListName);
            for (int i = 0; i < entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                var id = entry.FindPropertyRelative("id");
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    entry.isExpanded = EditorGUILayout.Foldout(entry.isExpanded,
                        string.IsNullOrWhiteSpace(id.stringValue) ? "New entry (set ID)" : id.stringValue, true);
                    if (entry.isExpanded) DrawFields(entry);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(i == 0))
                            if (GUILayout.Button("Up")) { entries.MoveArrayElement(i, i - 1); break; }
                        using (new EditorGUI.DisabledScope(i == entries.arraySize - 1))
                            if (GUILayout.Button("Down")) { entries.MoveArrayElement(i, i + 1); break; }
                        if (GUILayout.Button("Remove")) { entries.DeleteArrayElementAtIndex(i); break; }
                    }
                }
            }
            if (GUILayout.Button("Add entry"))
            {
                int index = entries.arraySize++;
                var entry = entries.GetArrayElementAtIndex(index);
                // Unity duplicates the previous array element. Reset from a new entry instead.
                SetFloat(entry, "prewarmCount", 0);
                var prefab = entry.FindPropertyRelative("prefab");
                if (prefab != null) prefab.objectReferenceValue = null;
                var mode = entry.FindPropertyRelative("despawnMode");
                if (mode != null) mode.enumValueIndex = (int)VfxDespawnMode.ParticleDuration;
                SetFloat(entry, "fixedLifetime", 1);
                SetFloat(entry, "duration", .8f);
                SetFloat(entry, "startScale", 1);
                SetFloat(entry, "endScale", 1.15f);
                var color = entry.FindPropertyRelative("color");
                if (color != null) color.colorValue = Color.white;
                var offset = entry.FindPropertyRelative("moveOffset");
                if (offset != null) offset.vector3Value = new Vector3(0, 80, 0);
                var options = entry.FindPropertyRelative("options");
                if (options != null)
                {
                    SetFloat(options, "Duration", .35f);
                    SetFloat(options, "Amplitude", .18f);
                    SetFloat(options, "Frequency", 30);
                    options.FindPropertyRelative("UnscaledTime").boolValue = true;
                }
                entry.FindPropertyRelative("id").stringValue = "";
                entry.isExpanded = true;
            }
            serializedObject.ApplyModifiedProperties();
            foreach (var issue in Validate())
                EditorGUILayout.HelpBox(issue.Message, issue.IsError ? MessageType.Error : MessageType.Warning);
        }

        private static void SetFloat(SerializedProperty entry, string name, float value)
        {
            var field = entry.FindPropertyRelative(name);
            if (field == null) return;
            if (field.propertyType == SerializedPropertyType.Integer) field.intValue = (int)value;
            else field.floatValue = value;
        }

        private static void DrawFields(SerializedProperty entry)
        {
            var field = entry.Copy(); var end = field.GetEndProperty();
            bool enter = true;
            while (field.NextVisible(enter) && !SerializedProperty.EqualContents(field, end))
            {
                enter = false;
                if (field.name == "fixedLifetime")
                {
                    var mode = entry.FindPropertyRelative("despawnMode");
                    if ((VfxDespawnMode)mode.enumValueIndex != VfxDespawnMode.FixedLifetime) continue;
                }
                EditorGUILayout.PropertyField(field, true);
            }
        }
    }

    [CustomEditor(typeof(VfxDatabase))]
    public sealed class VfxDatabaseEditor : FeedbackDatabaseEditor
    {
        protected override string ListName => "entries";
        protected override string Hint => "Add an ID and prefab; use that ID in a VFX node. Particle Duration suits one-shot particles. Prewarm Count is optional.";
        protected override List<FeedbackValidationIssue> Validate() => VfxDatabaseValidator.Validate((VfxDatabase)target);
    }
    [CustomEditor(typeof(FloatingTextDatabase))]
    public sealed class FloatingTextDatabaseEditor : FeedbackDatabaseEditor
    {
        protected override string ListName => "entries";
        protected override string Hint => "An ID selects the text style/prefab. A Floating Text node supplies text, amount and world position; Host needs a world camera.";
        protected override List<FeedbackValidationIssue> Validate() => FloatingTextDatabaseValidator.Validate((FloatingTextDatabase)target);
    }
    [CustomEditor(typeof(CameraShakeDatabase))]
    public sealed class CameraShakeDatabaseEditor : FeedbackDatabaseEditor
    {
        protected override string ListName => "presets";
        protected override string Hint => "Named presets for direct CameraShake.Shake(id) calls. Composite Camera Shake nodes use their own options; no database entry is needed.";
        protected override List<FeedbackValidationIssue> Validate() => CameraShakeDatabaseValidator.Validate((CameraShakeDatabase)target);
    }
}
