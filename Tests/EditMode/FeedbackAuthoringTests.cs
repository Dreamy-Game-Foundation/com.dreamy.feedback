using System.Collections.Generic;
using System.Reflection;
using Dreamy.Feedback.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Feedback.Tests
{
    public sealed class FeedbackAuthoringTests
    {
        private static void Set(object obj, string field, object value) => obj.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
        [Test] public void CameraOptionsRoundTripThroughUnitySerialization()
        {
            var db = ScriptableObject.CreateInstance<CameraShakeDatabase>();
            var copy = ScriptableObject.CreateInstance<CameraShakeDatabase>();
            try
            {
                var preset = new CameraShakePreset(); Set(preset, "id", "test"); Set(preset, "options", new CameraShakeOptions { Duration = .4f, Amplitude = .15f, Frequency = 20 });
                Set(db, "presets", new List<CameraShakePreset> { preset });
                EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(db), copy);
                Assert.That(copy.Presets[0].Options.Duration, Is.EqualTo(.4f));
                Assert.That(copy.Presets[0].Options.Amplitude, Is.EqualTo(.15f));
            }
            finally { Object.DestroyImmediate(db); Object.DestroyImmediate(copy); }
        }
        [Test] public void SequenceOptionsRoundTripThroughUnitySerialization()
        {
            var db = ScriptableObject.CreateInstance<FeedbackSequenceDatabase>(); var copy = ScriptableObject.CreateInstance<FeedbackSequenceDatabase>();
            try
            {
                var action = new FeedbackSequenceAction(); Set(action, "type", FeedbackSequenceActionType.ScreenFlash); Set(action, "screenFlashOptions", ScreenFlashOptions.WhiteFlash(.8f));
                Set(action, "iconFlyOptions", new IconFlyOptions { Count = 7, Duration = .9f });
                var entry = new FeedbackSequenceEntry(); Set(entry, "id", "test"); Set(entry, "actions", new List<FeedbackSequenceAction> { action }); Set(db, "entries", new List<FeedbackSequenceEntry> { entry });
                EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(db), copy);
                var result = copy.Entries[0].Actions[0]; Assert.That(result.ScreenFlashOptions.MaxAlpha, Is.EqualTo(.65f));
                Assert.That(result.IconFlyOptions.Count, Is.EqualTo(7));
            }
            finally { Object.DestroyImmediate(db); Object.DestroyImmediate(copy); }
        }
        [Test] public void DuplicateIdAndMissingPrefabAreReported()
        {
            var db = ScriptableObject.CreateInstance<VfxDatabase>();
            try
            {
                var a = new VfxEntry(); var b = new VfxEntry(); Set(a, "id", "duplicate"); Set(b, "id", "duplicate"); Set(db, "entries", new List<VfxEntry> { a, b });
                var issues = VfxDatabaseValidator.Validate(db);
                Assert.That(issues.Exists(i => i.Message.ToLowerInvariant().Contains("duplicate")), Is.True);
                Assert.That(issues.Exists(i => i.Message.ToLowerInvariant().Contains("prefab")), Is.True);
            }
            finally { Object.DestroyImmediate(db); }
        }
        [Test] public void IdGeneratorEscapesControlCharactersAndResolvesNameCollisions()
        {
            string code = FeedbackIdsGenerator.GenerateClass("FeedbackIds", new[] { "level-up", "level_up", "quote\"\nline" });
            Assert.That(code, Does.Contain("LevelUp2"));
            Assert.That(code, Does.Contain("\\\"\\nline"));
        }
    }
}
