using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dreamy.Feedback.Tests
{
    public sealed class FeedbackEffectAuthoringTests
    {
        private readonly List<Object> owned = new List<Object>();
        private T Asset<T>() where T : ScriptableObject { var asset = ScriptableObject.CreateInstance<T>(); owned.Add(asset); return asset; }
        [TearDown] public void Cleanup() { foreach (var item in owned) if (item) Object.DestroyImmediate(item); owned.Clear(); }

        [Test] public void RequestOverridesDoNotMutateSharedAssets()
        {
            var preset = Asset<UiPunchFeedbackAsset>(); preset.Configure("punch", UiPunchOptions.Default);
            var resolved = FeedbackGraphResolver.Resolve(FeedbackNode.From(preset), new FeedbackEffectOverrides { Fields = FeedbackOverrideFields.Strength, Strength = 0 });
            Assert.That(resolved.Punch.Strength, Is.Zero);
            Assert.That(preset.Settings.Strength, Is.EqualTo(.2f));
            var serialized = new SerializedObject(preset);
            Assert.That(serialized.FindProperty("overrides"), Is.Null);
            Assert.That(serialized.FindProperty("baseAsset"), Is.Null);
        }
        [Test] public void DefinitionCycleIsRejectedButRepeatedAssetIsAllowed()
        {
            var a = Asset<FeedbackDefinition>(); var b = Asset<FeedbackDefinition>();
            a.Configure("a", FeedbackPlayMode.Sequence, new[] { new FeedbackStep(b) });
            b.Configure("b", FeedbackPlayMode.Sequence, new[] { new FeedbackStep(a) });
            Assert.Throws<InvalidOperationException>(() => a.Resolve());
            var effect = Asset<HapticFeedbackAsset>(); effect.Configure("haptic", HapticType.Light);
            a.Configure("a", FeedbackPlayMode.Parallel, new[] { new FeedbackStep(effect), new FeedbackStep(effect) });
            Assert.That(a.Resolve().Children.Count, Is.EqualTo(2));
        }

        [Test] public void LibraryReportsDuplicatesAndMissingReferences()
        {
            var a = Asset<HapticFeedbackAsset>(); a.Configure("same", HapticType.Light);
            var b = Asset<HapticFeedbackAsset>(); b.Configure("same", HapticType.Success);
            var library = Asset<FeedbackLibrary>(); library.Configure(new FeedbackEffectAsset[] { a, b, null }, Array.Empty<FeedbackDefinition>());
            Assert.That(library.Validate(), Has.Some.Contains("Duplicate effect ID"));
            Assert.That(library.Validate(), Has.Some.Contains("Missing effect"));
        }

        [TestCase(typeof(VfxFeedbackAsset))]
        [TestCase(typeof(FloatingTextFeedbackAsset))]
        [TestCase(typeof(IconFlyFeedbackAsset))]
        [TestCase(typeof(UiPunchFeedbackAsset))]
        [TestCase(typeof(CameraShakeFeedbackAsset))]
        [TestCase(typeof(ScreenFlashFeedbackAsset))]
        [TestCase(typeof(HapticFeedbackAsset))]
        public void EffectSettingsRoundTrip(Type type)
        {
            var original = (FeedbackEffectAsset)ScriptableObject.CreateInstance(type); owned.Add(original);
            var copy = (FeedbackEffectAsset)ScriptableObject.CreateInstance(type); owned.Add(copy);
            original.ConfigureIdentity("serialized.effect");
            EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(original), copy);
            Assert.That(copy.Id, Is.EqualTo(original.Id));
            Assert.That(copy.Resolve().Type, Is.EqualTo(original.Type));
        }

        [Test] public void SOReferenceSurvivesDefinitionSerialization()
        {
            var effect = Asset<UiPunchFeedbackAsset>(); effect.Configure("punch", UiPunchOptions.Default);
            var definition = Asset<FeedbackDefinition>(); definition.Configure("definition", FeedbackNode.From(effect));
            string effectPath = AssetDatabase.GenerateUniqueAssetPath("Assets/FeedbackSerializationEffect.asset");
            string definitionPath = AssetDatabase.GenerateUniqueAssetPath("Assets/FeedbackSerializationDefinition.asset");
            string copyPath = AssetDatabase.GenerateUniqueAssetPath("Assets/FeedbackSerializationCopy.asset");
            try
            {
                AssetDatabase.CreateAsset(effect, effectPath);
                AssetDatabase.CreateAsset(definition, definitionPath);
                AssetDatabase.SaveAssetIfDirty(definition);
                Assert.That(AssetDatabase.CopyAsset(definitionPath, copyPath), Is.True);
                var copy = AssetDatabase.LoadAssetAtPath<FeedbackDefinition>(copyPath);
                Assert.That(copy.Root.Effect, Is.SameAs(effect));
                Assert.That(FeedbackGraphValidation.Validate(copy), Is.Null);
            }
            finally { AssetDatabase.DeleteAsset(copyPath); AssetDatabase.DeleteAsset(definitionPath); AssetDatabase.DeleteAsset(effectPath); }
        }

        [Test] public void MigrationPreviewIsReadOnlyAndChecksMissingPrefabs()
        {
            var definition = Asset<FeedbackDefinition>();
            definition.Configure("migration", FeedbackNode.Group(FeedbackNodeType.Sequence,
                new FeedbackNode { Type = FeedbackNodeType.UiPunch },
                new FeedbackNode { Type = FeedbackNodeType.Vfx, Id = "missing", Duration = 1 }));
            Assert.Throws<InvalidOperationException>(() => Editor.FeedbackDefinitionMigration.Preview(definition, null));
            Assert.That(definition.Root.Children[0].Effect, Is.Null);
            definition.Root.Children.RemoveAt(1);
            Assert.That(Editor.FeedbackDefinitionMigration.Preview(definition, null), Does.Contain("1 inline effects"));
            Assert.That(definition.Root.Children[0].Effect, Is.Null);
        }

        [Test] public void ListMigrationExportsRuntimeSettingsWithoutSerializingOverrides()
        {
            var effect = Asset<UiPunchFeedbackAsset>(); effect.Configure("punch", UiPunchOptions.Default);
            var definition = Asset<FeedbackDefinition>(); var node = FeedbackNode.From(effect);
            node.Overrides = new FeedbackEffectOverrides { Fields = FeedbackOverrideFields.Strength, Strength = .7f };
            definition.Configure("migration-list", FeedbackNode.Group(FeedbackNodeType.Sequence, new FeedbackNode { Type = FeedbackNodeType.Delay, Duration = .2f }, node));
            string before = EditorJsonUtility.ToJson(definition);
            Assert.That(Editor.FeedbackListMigration.Preview(definition), Does.Contain("1 runtime bindings"));
            Assert.That(EditorJsonUtility.ToJson(definition), Is.EqualTo(before));
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/FeedbackListMigrationFixture.asset");
            try
            {
                AssetDatabase.CreateAsset(definition, path);
                var bindings = Editor.FeedbackListMigration.Convert(definition, true);
                Assert.That(definition.IsLegacy, Is.False); Assert.That(definition.Effects.Count, Is.EqualTo(1));
                Assert.That(definition.Effects[0].DelayBefore, Is.EqualTo(.2f));
                Assert.That(definition.Effects[0].Asset, Is.SameAs(effect));
                Assert.That(bindings[0].Path, Is.EqualTo("0")); Assert.That(bindings[0].Overrides.Strength, Is.EqualTo(.7f));
                Assert.That(EditorJsonUtility.ToJson(definition), Does.Not.Contain("Overrides"));
                Assert.That(Editor.FeedbackListMigration.Convert(definition, true), Is.Empty);
            }
            finally { AssetDatabase.DeleteAsset(path); }
        }
        [Test] public void IconAuthoringSettingsContainNoRuntimeAnchorsOrPositions()
        {
            var effect = Asset<IconFlyFeedbackAsset>(); effect.Configure("icon", IconFlyOptions.Create(null, Vector3.one, Vector3.one));
            var settings = new SerializedObject(effect).FindProperty("settings");
            Assert.That(settings.FindPropertyRelative("StartPosition"), Is.Null);
            Assert.That(settings.FindPropertyRelative("EndPosition"), Is.Null);
            Assert.That(settings.FindPropertyRelative("Target"), Is.Null);
        }

        [Test] public void ResolverFreezesDelayedStepsAndRejectsSharedNodes()
        {
            var punch = Asset<UiPunchFeedbackAsset>(); punch.Configure("punch", UiPunchOptions.Default);
            var step = FeedbackNode.From(punch);
            var root = FeedbackNode.Group(FeedbackNodeType.Sequence, new FeedbackNode { Type = FeedbackNodeType.Delay, Duration = .1f }, step);
            var snapshot = FeedbackGraphResolver.Resolve(root);
            punch.Configure("punch", new UiPunchOptions { Duration = 9, Strength = 9 });
            Assert.That(snapshot.Children[1].Punch.Strength, Is.EqualTo(.2f));
            root.Children.Add(step);
            Assert.Throws<InvalidOperationException>(() => FeedbackGraphResolver.Resolve(root));
        }
    }
}
