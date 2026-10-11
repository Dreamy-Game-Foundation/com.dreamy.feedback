using UnityEditor;
using UnityEngine;
namespace Dreamy.Feedback.Editor
{
    [CustomEditor(typeof(FeedbackEffectAsset), true)]
    public sealed class FeedbackEffectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var id = serializedObject.FindProperty("id"); id.isExpanded = EditorGUILayout.Foldout(id.isExpanded, "Advanced"); if (id.isExpanded) EditorGUILayout.PropertyField(id);
            var settings = serializedObject.FindProperty("settings");
            if (settings != null) EditorGUILayout.PropertyField(settings, true);
            var format = serializedObject.FindProperty("format");
            if (format != null) EditorGUILayout.PropertyField(format);
            serializedObject.ApplyModifiedProperties();
            try
            {
                var node = ((FeedbackEffectAsset)target).Resolve();
                var error = FeedbackGraphValidation.ValidateNode(node, false);
                if (node.Type == FeedbackNodeType.FloatingText && node.TextSettings.Prefab &&
                    !node.TextSettings.Prefab.GetComponent<FloatingTextInstance>())
                    error = "Text prefab requires FloatingTextInstance on its root.";
                if (error != null) EditorGUILayout.HelpBox(error, MessageType.Error);
            }
            catch (System.Exception e) { EditorGUILayout.HelpBox(e.Message, MessageType.Error); }
            EditorGUILayout.HelpBox("Drop this preset into a Definition or play it directly. Scene anchors are supplied by the request.", MessageType.Info);
        }
    }

    [CustomPropertyDrawer(typeof(FeedbackEffectOverrides))]
    public sealed class FeedbackEffectOverridesDrawer : PropertyDrawer
    {
        private static readonly string[] Names = { "Duration", "Color", "Count", "Strength", "Text", "Icon", "Prefab", "IconOptions", "Haptic", "FlashOptions", "PunchOptions", "ShakeOptions" };
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded) return height;
            height += EditorGUIUtility.singleLineHeight + 3;
            int flags = property.FindPropertyRelative("Fields").intValue;
            for (int i = 0; i < Names.Length; i++) if ((flags & (1 << i)) != 0)
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(Names[i]), true) + 3;
            return height;
        }
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            var line = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(line, property.isExpanded, label, true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                line.y += line.height + 3;
                var fields = property.FindPropertyRelative("Fields");
                EditorGUI.PropertyField(line, fields);
                for (int i = 0; i < Names.Length; i++) if ((fields.intValue & (1 << i)) != 0)
                {
                    var value = property.FindPropertyRelative(Names[i]);
                    line.y += line.height + 3; line.height = EditorGUI.GetPropertyHeight(value, true);
                    EditorGUI.PropertyField(line, value, true);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }
    }
}
