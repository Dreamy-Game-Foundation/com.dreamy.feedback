using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Feedback.Editor
{
    public sealed class DreamyFeedbackWindow : EditorWindow
    {
        private const string WindowTitle = "Dreamy Feedback";

        private VfxDatabase vfxDatabase;
        private FloatingTextDatabase floatingTextDatabase;
        private CameraShakeDatabase cameraShakeDatabase;
        private FeedbackDefinition sequenceDatabase;
        private Vector2 scroll;

        public static void Open()
        {
            GetWindow<DreamyFeedbackWindow>(WindowTitle);
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.LabelField("Databases", EditorStyles.boldLabel);
            vfxDatabase = (VfxDatabase)EditorGUILayout.ObjectField("VFX", vfxDatabase, typeof(VfxDatabase), false);
            floatingTextDatabase = (FloatingTextDatabase)EditorGUILayout.ObjectField("Floating Text", floatingTextDatabase, typeof(FloatingTextDatabase), false);
            cameraShakeDatabase = (CameraShakeDatabase)EditorGUILayout.ObjectField("Camera Shake", cameraShakeDatabase, typeof(CameraShakeDatabase), false);
            sequenceDatabase = (FeedbackDefinition)EditorGUILayout.ObjectField("Sequence", sequenceDatabase, typeof(FeedbackDefinition), false);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Create", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("VFX"))
                {
                    FeedbackMenuItems.CreateVfxDatabase();
                }

                if (GUILayout.Button("Floating Text"))
                {
                    FeedbackMenuItems.CreateFloatingTextDatabase();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Camera Shake"))
                {
                    FeedbackMenuItems.CreateCameraShakeDatabase();
                }

                if (GUILayout.Button("Sequence"))
                {
                    FeedbackMenuItems.CreateFeedbackDefinition();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
            if (GUILayout.Button("Validate Selected"))
            {
                ValidateSelected();
            }

            if (GUILayout.Button("Validate All"))
            {
                FeedbackMenuItems.ValidateAll();
            }

            if (GUILayout.Button("Generate IDs From Selection"))
            {
                FeedbackMenuItems.GenerateIds();
            }

            EditorGUILayout.EndScrollView();
        }

        private void ValidateSelected()
        {
            LogIssues("VFX Database", vfxDatabase ? VfxDatabaseValidator.Validate(vfxDatabase) : null);
            LogIssues("Floating Text Database", floatingTextDatabase ? FloatingTextDatabaseValidator.Validate(floatingTextDatabase) : null);
            LogIssues("Camera Shake Database", cameraShakeDatabase ? CameraShakeDatabaseValidator.Validate(cameraShakeDatabase) : null);
            LogIssues("Feedback Sequence Database", sequenceDatabase ? FeedbackDefinitionValidator.Validate(sequenceDatabase) : null);
        }

        private static void LogIssues(string label, IReadOnlyList<FeedbackValidationIssue> issues)
        {
            if (issues == null)
            {
                Debug.Log($"{label}: no asset selected.");
                return;
            }

            FeedbackValidatorUtility.LogIssues(label, issues);
        }
    }
}
