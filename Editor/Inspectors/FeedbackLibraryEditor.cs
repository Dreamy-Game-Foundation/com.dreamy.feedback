using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Dreamy.Feedback.Editor
{
    [CustomEditor(typeof(FeedbackLibrary))]
    public sealed class FeedbackLibraryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Add selected assets")) Add(Selection.objects);
            if (GUILayout.Button("Scan this library's folder"))
            {
                var folder = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(target)).Replace('\\','/');
                Add(AssetDatabase.FindAssets("t:FeedbackEffectAsset t:FeedbackDefinition", new[] { folder })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid))).ToArray());
            }
            foreach (var error in ((FeedbackLibrary)target).Validate()) EditorGUILayout.HelpBox(error, MessageType.Error);
        }
        private void Add(Object[] assets)
        {
            var library = (FeedbackLibrary)target;
            Undo.RecordObject(library, "Add feedback assets");
            library.Configure(library.Effects.Concat(assets.OfType<FeedbackEffectAsset>()).Distinct(),
                library.Definitions.Concat(assets.OfType<FeedbackDefinition>()).Distinct());
            EditorUtility.SetDirty(library);
        }
    }
}
