using UnityEditor;
using UnityEngine;
namespace Dreamy.Feedback.Editor
{
    [CustomEditor(typeof(FeedbackDefinition))]
    public sealed class FeedbackDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var definition = (FeedbackDefinition)target;
            if (definition.IsLegacy)
            {
                EditorGUILayout.HelpBox("Legacy graph: use the migration window to convert. New definitions use Mode + Effects.", MessageType.Info);
                if (GUILayout.Button("Open migration")) FeedbackAuthoringMigrationWindow.Open(definition);
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("playMode"));
                if (definition.PlayMode == FeedbackPlayMode.Staggered) EditorGUILayout.PropertyField(serializedObject.FindProperty("staggerInterval"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("effects"), true);
                var rect = GUILayoutUtility.GetRect(0, 35, GUILayout.ExpandWidth(true));
                GUI.Box(rect, "Drop effect or definition assets here");
                if (rect.Contains(Event.current.mousePosition) && (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag(); var list = serializedObject.FindProperty("effects");
                        foreach (var item in DragAndDrop.objectReferences) if (item is FeedbackAsset && item != target)
                        { int i = list.arraySize++; var row = list.GetArrayElementAtIndex(i); row.FindPropertyRelative("Asset").objectReferenceValue = item; row.FindPropertyRelative("DelayBefore").floatValue = 0; row.FindPropertyRelative("Required").boolValue = true; }
                    }
                    Event.current.Use();
                }
            }
            var id = serializedObject.FindProperty("id"); id.isExpanded = EditorGUILayout.Foldout(id.isExpanded, "Advanced");
            if (id.isExpanded) { EditorGUILayout.PropertyField(id); EditorGUILayout.PropertyField(serializedObject.FindProperty("unscaledTime")); }
            serializedObject.ApplyModifiedProperties();
            var error = FeedbackGraphValidation.Validate(definition); if (error != null) EditorGUILayout.HelpBox(error, MessageType.Warning);
        }
        internal static void DrawOptions(SerializedProperty options)
        {
            var field = options.Copy(); var end = field.GetEndProperty();
            bool enter = true;
            while (field.NextVisible(enter) && !SerializedProperty.EqualContents(field, end))
            {
                enter = false;
                if (field.name == "StartPosition" || field.name == "EndPosition" || field.name == "UnscaledTime") continue;
                EditorGUILayout.PropertyField(field, true);
            }
        }

    }
    [CustomPropertyDrawer(typeof(FeedbackStep))]
    public sealed class FeedbackStepDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight * (property.isExpanded ? 4 : 1) + 6;
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            var line = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(new Rect(line.x, line.y, 15, line.height), property.isExpanded, GUIContent.none);
            EditorGUI.PropertyField(new Rect(line.x + 18, line.y, line.width - 18, line.height), property.FindPropertyRelative("Asset"), GUIContent.none);
            if (property.isExpanded)
            {
                line.y += line.height + 2; EditorGUI.PropertyField(line, property.FindPropertyRelative("DelayBefore"));
                line.y += line.height + 2; EditorGUI.PropertyField(line, property.FindPropertyRelative("Required"));
            }
            EditorGUI.EndProperty();
        }
    }
}
