using UnityEditor;
using UnityEngine;
namespace Dreamy.Feedback.Editor
{
    [CustomEditor(typeof(FeedbackDefinition))]
    public sealed class FeedbackDefinitionEditor : UnityEditor.Editor
    {
        private FeedbackHost previewHost;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unscaledTime"));
            EditorGUILayout.HelpBox("Definition = animation data. Play supplies world position, UI source/target and amount. Start from Assets > Create > Dreamy > Feedback > Common Presets, or duplicate a sample preset.", MessageType.Info);
            previewHost = (FeedbackHost)EditorGUILayout.ObjectField(new GUIContent("Lookup IDs from rig", "Optional editor lookup only; no Host reference is saved in the definition."), previewHost, typeof(FeedbackHost), true);
            DrawNode(serializedObject.FindProperty("root"),"Root",0);
            serializedObject.ApplyModifiedProperties();
            var error = FeedbackGraphValidation.Validate((FeedbackDefinition)target);
            if (error != null) EditorGUILayout.HelpBox(error,MessageType.Warning);
        }
        private void DrawNode(SerializedProperty property, string label, int depth)
        {
            if (depth > 16) return;
            if (property.managedReferenceValue == null) property.managedReferenceValue = new FeedbackNode();
            var nodeType = (FeedbackNodeType)property.FindPropertyRelative("Type").enumValueIndex;
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded,label + " · " + nodeType,true);
            if (!property.isExpanded) return;
            EditorGUI.indentLevel++;
            var type = property.FindPropertyRelative("Type"); EditorGUILayout.PropertyField(type);
            var kind = (FeedbackNodeType)type.enumValueIndex;
            if (kind != nodeType)
            {
                property.managedReferenceValue = FeedbackPresetFactory.CreateNode(kind);
                EditorGUI.indentLevel--;
                return;
            }
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
            foreach (var name in fields)
            {
                var field = property.FindPropertyRelative(name);
                if (name == "IconFly" || name == "Punch" || name == "Shake" || name == "Flash") DrawOptions(field);
                else
                {
                    EditorGUILayout.PropertyField(field, true);
                    if (name == "Id" && previewHost) DrawPresetLookup(field, kind);
                }
            }
            if (kind == FeedbackNodeType.IconFly)
                EditorGUILayout.HelpBox("Set sprite and animation here. Play supplies IconsFrom(UI anchor) and To(HUD anchor). Amount does not change Count.", MessageType.Info);
            if (kind == FeedbackNodeType.UiPunch) EditorGUILayout.LabelField("Play requires To(target).");
            if (kind == FeedbackNodeType.CameraShake) EditorGUILayout.LabelField("Host requires a game camera; options are inline.");
            if (kind == FeedbackNodeType.Vfx || kind == FeedbackNodeType.FloatingText)
                EditorGUILayout.LabelField("ID must exist in the database assigned to Host.");
            var children = property.FindPropertyRelative("Children");
            if (kind == FeedbackNodeType.Sequence || kind == FeedbackNodeType.Parallel)
            {
                for (int i = 0; i < children.arraySize; i++)
                {
                    DrawNode(children.GetArrayElementAtIndex(i),"Step " + (i+1),depth+1);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Up") && i > 0) { children.MoveArrayElement(i,i-1); break; }
                        if (GUILayout.Button("Down") && i < children.arraySize - 1) { children.MoveArrayElement(i,i+1); break; }
                        if (GUILayout.Button("Remove")) { children.DeleteArrayElementAtIndex(i); break; }
                    }
                }
                if (GUILayout.Button("Add effect…"))
                {
                    var menu = new GenericMenu();
                    var path = children.propertyPath;
                    foreach (FeedbackNodeType value in System.Enum.GetValues(typeof(FeedbackNodeType)))
                    {
                        var captured = value;
                        menu.AddItem(new GUIContent(value.ToString()), false, () =>
                        {
                            serializedObject.Update();
                            var list = serializedObject.FindProperty(path);
                            int index = list.arraySize++;
                            list.GetArrayElementAtIndex(index).managedReferenceValue = FeedbackPresetFactory.CreateNode(captured);
                            list.GetArrayElementAtIndex(index).isExpanded = true;
                            serializedObject.ApplyModifiedProperties();
                        });
                    }
                    menu.ShowAsContext();
                }
            }
            else if (children.arraySize > 0) children.arraySize = 0;
            EditorGUI.indentLevel--;
        }

        private static void DrawOptions(SerializedProperty options)
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

        private void DrawPresetLookup(SerializedProperty id, FeedbackNodeType kind)
        {
            var host = new SerializedObject(previewHost);
            var values = new System.Collections.Generic.List<string> { "Choose preset…" };
            if (kind == FeedbackNodeType.Vfx)
            {
                var db = host.FindProperty("vfxDatabase").objectReferenceValue as VfxDatabase;
                if (db) foreach (var entry in db.Entries)
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.Id) && !values.Contains(entry.Id)) values.Add(entry.Id);
            }
            else if (kind == FeedbackNodeType.FloatingText)
            {
                var db = host.FindProperty("floatingTextDatabase").objectReferenceValue as FloatingTextDatabase;
                if (db) foreach (var entry in db.Entries)
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.Id) && !values.Contains(entry.Id)) values.Add(entry.Id);
            }
            int current = values.IndexOf(id.stringValue);
            int selected = EditorGUILayout.Popup("Database preset", Mathf.Max(0, current), values.ToArray());
            if (selected > 0 && selected != current) id.stringValue = values[selected];
            if (!values.Contains(id.stringValue))
                EditorGUILayout.HelpBox("ID is missing in the selected rig's database.", MessageType.Warning);
        }
    }
}
