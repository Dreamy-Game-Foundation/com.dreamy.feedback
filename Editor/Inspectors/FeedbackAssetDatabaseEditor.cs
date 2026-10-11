using UnityEditor;
namespace Dreamy.Feedback.Editor
{
    [CustomEditor(typeof(FeedbackDatabaseAsset), true)]
    public sealed class FeedbackAssetDatabaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update(); EditorGUILayout.PropertyField(serializedObject.FindProperty("assets"), true);
            serializedObject.ApplyModifiedProperties();
            try { ((FeedbackDatabaseAsset)target).Rebuild(); } catch (System.Exception e) { EditorGUILayout.HelpBox(e.Message, MessageType.Error); }
        }
    }
}
