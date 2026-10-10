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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unscaledTime"));
            DrawNode(serializedObject.FindProperty("root"),"Root",0);
            serializedObject.ApplyModifiedProperties();
            var error = FeedbackGraphValidation.Validate((FeedbackDefinition)target);
            if (error != null) EditorGUILayout.HelpBox(error,MessageType.Warning);
        }
        private void DrawNode(SerializedProperty property, string label, int depth)
        {
            if (depth > 16) return;
            if (property.managedReferenceValue == null) property.managedReferenceValue = new FeedbackNode();
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded,label,true);
            if (!property.isExpanded) return;
            EditorGUI.indentLevel++;
            var type = property.FindPropertyRelative("Type"); EditorGUILayout.PropertyField(type);
            var kind = (FeedbackNodeType)type.enumValueIndex;
            EditorGUILayout.PropertyField(property.FindPropertyRelative("Required"));
            string[] fields;
            switch (kind)
            {
                case FeedbackNodeType.Vfx: fields = new[] {"Id","Duration"}; break;
                case FeedbackNodeType.Delay: fields = new[] {"Duration"}; break;
                case FeedbackNodeType.FloatingText: fields = new[] {"Id","Text"}; break;
                case FeedbackNodeType.Haptic: fields = new[] {"Haptic"}; break;
                case FeedbackNodeType.IconFly: fields = new[] {"IconFly"}; break;
                case FeedbackNodeType.UiPunch: fields = new[] {"Punch"}; break;
                case FeedbackNodeType.CameraShake: fields = new[] {"Shake"}; break;
                case FeedbackNodeType.ScreenFlash: fields = new[] {"Flash"}; break;
                default: fields = new string[0]; break;
            }
            foreach (var name in fields) EditorGUILayout.PropertyField(property.FindPropertyRelative(name),true);
            var children = property.FindPropertyRelative("Children");
            if (kind == FeedbackNodeType.Sequence || kind == FeedbackNodeType.Parallel)
            {
                for (int i = 0; i < children.arraySize; i++)
                {
                    DrawNode(children.GetArrayElementAtIndex(i),"Step " + (i+1),depth+1);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Up") && i > 0) { children.MoveArrayElement(i,i-1); break; }
                        if (GUILayout.Button("Remove")) { children.DeleteArrayElementAtIndex(i); break; }
                    }
                }
                if (GUILayout.Button("Add child")) { int i = children.arraySize++; children.GetArrayElementAtIndex(i).managedReferenceValue = new FeedbackNode(); }
            }
            else if (children.arraySize > 0) children.arraySize = 0;
            EditorGUI.indentLevel--;
        }
    }
}
