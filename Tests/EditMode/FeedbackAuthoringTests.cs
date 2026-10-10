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
        [TestCase(FeedbackPreset.ButtonConfirm)]
        [TestCase(FeedbackPreset.ButtonReject)]
        [TestCase(FeedbackPreset.RewardPop)]
        [TestCase(FeedbackPreset.ComboPopup)]
        [TestCase(FeedbackPreset.LevelComplete)]
        [TestCase(FeedbackPreset.SoftShake)]
        [TestCase(FeedbackPreset.SuccessFlash)]
        public void CommonPresetSurvivesSerializationAndValidates(FeedbackPreset preset)
        {
            var definition = FeedbackPresetFactory.Create(preset);
            var copy = ScriptableObject.CreateInstance<FeedbackDefinition>();
            try
            {
                EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(definition), copy);
                Assert.That(FeedbackGraphValidation.Validate(copy), Is.Null);
                Assert.That(copy.Id, Is.EqualTo(definition.Id));
                Assert.That(copy.Root.Type, Is.EqualTo(definition.Root.Type));
                Assert.That(copy.Root.Children.Count, Is.EqualTo(definition.Root.Children.Count));
            }
            finally { Object.DestroyImmediate(definition); Object.DestroyImmediate(copy); }
        }
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
            var db=ScriptableObject.CreateInstance<FeedbackDefinition>(); var copy=ScriptableObject.CreateInstance<FeedbackDefinition>();
            try
            {
                db.Configure("test",FeedbackNode.Group(FeedbackNodeType.Sequence,new FeedbackNode { Type=FeedbackNodeType.ScreenFlash, Flash=ScreenFlashOptions.WhiteFlash(.8f) }));
                EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(db),copy);
                Assert.That(copy.Root.Children[0].Flash.MaxAlpha,Is.EqualTo(.65f));
                Assert.That(copy.Root.Type,Is.EqualTo(FeedbackNodeType.Sequence));
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
