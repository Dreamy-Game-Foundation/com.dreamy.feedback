using UnityEditor;
namespace Dreamy.Feedback.Editor
{
    [CustomEditor(typeof(FeedbackHost))]
    public sealed class FeedbackHostEditor : UnityEditor.Editor
    {
        private bool databases;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            foreach (var name in new[] { "root", "worldCamera", "animationProvider", "haptics", "iconFly", "screenFlash" }) EditorGUILayout.PropertyField(serializedObject.FindProperty(name));
            databases = EditorGUILayout.Foldout(databases, "ID lookup databases (optional)");
            if (databases)
            {
                foreach (var name in new[] { "vfxDatabase", "floatingTextDatabase", "cameraShakeDatabase", "uiPunchDatabase", "definitionDatabase" }) EditorGUILayout.PropertyField(serializedObject.FindProperty(name));
                if (serializedObject.FindProperty("iconFly").boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("iconFlyDatabase"));
                if (serializedObject.FindProperty("screenFlash").boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("screenFlashDatabase"));
                if (serializedObject.FindProperty("haptics").boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("hapticDatabase"));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
