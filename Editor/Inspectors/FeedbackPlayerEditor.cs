using UnityEditor;
namespace Dreamy.Feedback.Editor
{
    [CustomEditor(typeof(FeedbackPlayer))]
    public sealed class FeedbackPlayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            foreach (var field in new[] { "host", "asset" }) EditorGUILayout.PropertyField(serializedObject.FindProperty(field));
            var asset = serializedObject.FindProperty("asset").objectReferenceValue as FeedbackAsset;
            if (asset is FeedbackDefinition || asset is IconFlyFeedbackAsset)
                foreach (var field in new[] { "iconSource", "target", "icon", "useRuntimeCount" }) EditorGUILayout.PropertyField(serializedObject.FindProperty(field));
            else if (asset is UiPunchFeedbackAsset) EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
            if (serializedObject.FindProperty("useRuntimeCount").boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("iconCount"));
            if (asset is FeedbackDefinition || asset is FloatingTextFeedbackAsset) EditorGUILayout.PropertyField(serializedObject.FindProperty("amount"));
            var advanced = serializedObject.FindProperty("customize"); advanced.isExpanded = EditorGUILayout.Foldout(advanced.isExpanded, "Runtime settings");
            if (advanced.isExpanded)
            { EditorGUILayout.PropertyField(advanced); if (advanced.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("runtimeSettings"), true); if (asset is FeedbackDefinition) EditorGUILayout.PropertyField(serializedObject.FindProperty("bindings"), true); }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
