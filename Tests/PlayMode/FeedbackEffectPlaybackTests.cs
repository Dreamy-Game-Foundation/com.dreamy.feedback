using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Dreamy.Feedback.Tests
{
    public sealed class FeedbackEffectPlaybackTests
    {
        private readonly List<Object> owned = new List<Object>();
        private readonly List<IDisposable> services = new List<IDisposable>();
        private T Asset<T>() where T : ScriptableObject { var a = ScriptableObject.CreateInstance<T>(); owned.Add(a); return a; }
        private GameObject GO(string name, params Type[] types) { var go = new GameObject(name, types); owned.Add(go); return go; }
        [UnityTearDown] public IEnumerator Cleanup()
        { foreach (var s in services) s.Dispose(); services.Clear(); foreach (var o in owned) if (o) Object.Destroy(o); owned.Clear(); Time.timeScale = 1; yield return null; }

        [UnityTest] public IEnumerator DirectVfxAndTextPlayWithoutDatabaseAndReusePrefabPool()
        {
            var root = GO("Pool");
            var prefab = GO("VFX", typeof(VfxInstance)); prefab.SetActive(false);
            var vfx = new VfxService(); vfx.Initialize(null, root.transform); services.Add(vfx);
            var vfxAsset = Asset<VfxFeedbackAsset>(); vfxAsset.Configure("vfx", new VfxFeedbackSettings { Prefab = prefab, MaximumDuration = .05f, Scale = 1, PrewarmCount = 2 });
            var texts = new FloatingTextService(); texts.Initialize(null, root.transform); services.Add(texts);
            var textPrefab = GO("Text", typeof(RectTransform), typeof(FloatingTextInstance)); textPrefab.SetActive(false);
            var textAsset = Asset<FloatingTextFeedbackAsset>();
            var settings = FloatingTextFeedbackSettings.Default; settings.Prefab = textPrefab; settings.PrewarmCount = 1; settings.Duration = .05f;
            textAsset.Configure("text", settings);
            var facade = new FeedbackService(new FeedbackServices { Vfx = vfx, FloatingText = texts }, new UnityFeedbackAnimationBackend()); services.Add(facade);
            var a = facade.Play(FeedbackRequestBuilder.For(vfxAsset).At(Vector3.one).Build());
            var b = facade.Play(FeedbackRequestBuilder.For(textAsset).WithAmount(12).Build());
            Assert.That(a.IsRunning && b.IsRunning, Is.True);
            a.Stop(); b.Complete();
            var again = facade.Play(FeedbackRequestBuilder.For(textAsset).WithOverrides(new FeedbackEffectOverrides { Fields = FeedbackOverrideFields.Duration, Duration = .1f }).Build());
            Assert.That(root.GetComponentsInChildren<FloatingTextInstance>(true).Length, Is.EqualTo(1));
            var current = facade.Play(FeedbackRequestBuilder.For(vfxAsset).Build());
            a.Stop(); Assert.That(current.IsRunning, Is.True);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(current.Status, Is.EqualTo(FeedbackStatus.Completed));
            Assert.That(again.Status, Is.EqualTo(FeedbackStatus.Completed));
        }

        [UnityTest] public IEnumerator MultipleUniTaskWaitersAndWaitCancellationAreIndependent()
        {
            var playback = new FeedbackPlayback();
            using (var cancellation = new CancellationTokenSource())
            {
                var a = playback.Handle.WaitAsync().AsTask();
                var b = playback.Handle.WaitAsync().AsTask();
                var canceled = playback.Handle.WaitAsync(cancellation.Token).SuppressCancellationThrow().AsTask();
                cancellation.Cancel(); yield return null;
                Assert.That(canceled.IsCompleted && canceled.Result.IsCanceled, Is.True);
                Assert.That(playback.Status, Is.EqualTo(FeedbackStatus.Running));
                playback.Finish(); yield return null;
                Assert.That(a.Result, Is.EqualTo(FeedbackStatus.Completed));
                Assert.That(b.Result, Is.EqualTo(FeedbackStatus.Completed));
            }
        }

        [Test] public void ZeroVfxStrengthIsAnExplicitNoSpawnOverride()
        {
            var root = GO("Pool");
            var prefab = GO("Prefab", typeof(VfxInstance));
            var preset = Asset<VfxFeedbackAsset>();
            preset.Configure("vfx", new VfxFeedbackSettings { Prefab = prefab, MaximumDuration = 1, Scale = 1, PrewarmCount = 4 });
            var vfx = new VfxService(); vfx.Initialize(null, root.transform); services.Add(vfx);
            var facade = new FeedbackService(new FeedbackServices { Vfx = vfx }, new UnityFeedbackAnimationBackend()); services.Add(facade);
            var handle = facade.Play(FeedbackRequestBuilder.For(preset).WithOverrides(
                new FeedbackEffectOverrides { Fields = FeedbackOverrideFields.Strength, Strength = 0 }).Build());
            Assert.That(handle.IsValid, Is.True);
            Assert.That(root.transform.childCount, Is.Zero);
            Assert.That(preset.Settings.Scale, Is.EqualTo(1));
        }

        [UnityTest] public IEnumerator DelayedAssetSnapshotAndRequestOverridesStayIndependent()
        {
            var target = GO("HUD", typeof(RectTransform));
            var punch = Asset<UiPunchFeedbackAsset>(); punch.Configure("punch", new UiPunchOptions { Duration = .2f, Strength = .1f, Vibrato = 2 });
            var definition = Asset<FeedbackDefinition>();
            definition.Configure("delayed", FeedbackNode.Group(FeedbackNodeType.Sequence,
                new FeedbackNode { Type = FeedbackNodeType.Delay, Duration = .04f }, FeedbackNode.From(punch)));
            var ui = new CaptureUi();
            var facade = new FeedbackService(new FeedbackServices { Ui = ui }, new UnityFeedbackAnimationBackend()); services.Add(facade);
            var a = facade.Play(FeedbackRequestBuilder.For(definition).To(target.transform).WithOverrides(new FeedbackEffectOverrides { Fields = FeedbackOverrideFields.Strength, Strength = .3f }).Build());
            var b = facade.Play(FeedbackRequestBuilder.For(definition).To(target.transform).Build());
            punch.Configure("punch", new UiPunchOptions { Duration = 10, Strength = 8 });
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(ui.Values, Is.EquivalentTo(new[] { .3f, .1f }));
            Assert.That(a.Status, Is.EqualTo(FeedbackStatus.Completed));
            Assert.That(b.Status, Is.EqualTo(FeedbackStatus.Completed));
        }

        [Test] public void SynchronousRequiredFailureDoesNotStartLaterParallelEffects()
        {
            var punch = Asset<UiPunchFeedbackAsset>(); punch.Configure("missing", UiPunchOptions.Default);
            var haptic = Asset<HapticFeedbackAsset>(); haptic.Configure("haptic", HapticType.Light);
            var definition = Asset<FeedbackDefinition>(); definition.Configure("failure", FeedbackNode.Group(FeedbackNodeType.Parallel, FeedbackNode.From(punch), FeedbackNode.From(haptic)));
            var haptics = new CaptureHaptic();
            var facade = new FeedbackService(new FeedbackServices { Haptic = haptics }, new UnityFeedbackAnimationBackend()); services.Add(facade);
            Assert.That(facade.Play(FeedbackRequestBuilder.For(definition).Build()).Status, Is.EqualTo(FeedbackStatus.Faulted));
            Assert.That(haptics.Count, Is.Zero);
        }

        [Test] public void DisabledOwnerDoesNotDispatchOneShot()
        {
            var owner = GO("Owner"); owner.SetActive(false);
            var effect = Asset<HapticFeedbackAsset>(); effect.Configure("haptic", HapticType.Light);
            var haptics = new CaptureHaptic();
            var facade = new FeedbackService(new FeedbackServices { Haptic = haptics }, new UnityFeedbackAnimationBackend()); services.Add(facade);
            Assert.That(facade.Play(FeedbackRequestBuilder.For(effect).From(owner).Build()).IsValid, Is.False);
            Assert.That(haptics.Count, Is.Zero);
        }
        [UnityTest] public IEnumerator ListModesHaveDistinctStartTimingAndCompleteSkipsPendingDelay()
        {
            var effect = Asset<UiPunchFeedbackAsset>(); effect.Configure("punch", UiPunchOptions.Default);
            var ui = new PendingUi(); var facade = new FeedbackService(new FeedbackServices { Ui = ui }, new UnityFeedbackAnimationBackend()); services.Add(facade);
            var definition = Asset<FeedbackDefinition>();
            definition.Configure("sequence", FeedbackPlayMode.Sequence, new[] { new FeedbackStep(effect), new FeedbackStep(effect) });
            var handle = facade.Play(FeedbackRequestBuilder.For(definition).Build());
            Assert.That(ui.States.Count, Is.EqualTo(1)); ui.States[0].Finish();
            float deadline = Time.realtimeSinceStartup + 1; while (ui.States.Count < 2 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(ui.States.Count, Is.EqualTo(2)); handle.Complete();
            ui.States.Clear();
            definition.Configure("parallel", FeedbackPlayMode.Parallel, new[] { new FeedbackStep(effect), new FeedbackStep(effect) });
            handle = facade.Play(FeedbackRequestBuilder.For(definition).Build()); Assert.That(ui.States.Count, Is.EqualTo(2)); handle.Stop();
            ui.States.Clear();
            definition.Configure("stagger", FeedbackPlayMode.Staggered, new[] { new FeedbackStep(effect), new FeedbackStep(effect) }, staggerInterval: .2f);
            handle = facade.Play(FeedbackRequestBuilder.For(definition).Build()); Assert.That(ui.States.Count, Is.EqualTo(1));
            yield return new WaitForSecondsRealtime(.25f); Assert.That(ui.States.Count, Is.EqualTo(2)); handle.Stop();
            ui.States.Clear();
            definition.Configure("delay", FeedbackPlayMode.Sequence, new[] { new FeedbackStep(effect, 10) });
            handle = facade.Play(FeedbackRequestBuilder.For(definition).Build()); Assert.That(ui.States, Is.Empty); handle.Complete();
            Assert.That(ui.States.Count, Is.EqualTo(1)); Assert.That(handle.Status, Is.EqualTo(FeedbackStatus.Completed));
        }
        [UnityTest] public IEnumerator IdIconArgsUseDefaultsAndZeroWithoutMutatingPreset()
        {
            var root = GO("icons", typeof(RectTransform)); var source = GO("source", typeof(RectTransform)); var target = GO("target", typeof(RectTransform));
            var texture = new Texture2D(2,2); owned.Add(texture); var sprite = Sprite.Create(texture,new Rect(0,0,2,2),Vector2.one * .5f); owned.Add(sprite);
            var preset = Asset<IconFlyFeedbackAsset>(); var options = IconFlyOptions.Create(sprite, Vector3.zero, Vector3.zero); options.Count = 3; options.Duration = 1; preset.Configure("icons", options);
            var database = Asset<IconFlyDatabase>(); database.Configure(new[] { preset });
            var service = new IconFlyService(); service.Initialize(root.transform); service.SetDatabase(database); services.Add(service);
            IIconFlyService primitive = service;
            var empty = primitive.Play("icons", new IconFlyPlayArgs { Count = 0 }); Assert.That(empty.Status, Is.EqualTo(FeedbackStatus.Completed)); Assert.That(root.transform.childCount, Is.Zero);
            var args = new IconFlyPlayArgs { Source = source.transform, Target = target.transform };
            var handle = primitive.Play("icons", args); Assert.That(root.transform.childCount, Is.EqualTo(3)); handle.Stop();
            args.Count = 2; var second = primitive.Play("icons", args); Assert.That(preset.Settings.Count, Is.EqualTo(3));
            Object.Destroy(source); yield return null; yield return null; Assert.That(second.Status, Is.EqualTo(FeedbackStatus.Stopped));
            Assert.That(new FeedbackContext(Vector3.zero, amount:100).IconCount, Is.Null);
        }
        [Test] public void PerTypeDatabaseRejectsDuplicateIdsAndDefinitionLookupIsSeparate()
        {
            var a = Asset<HapticFeedbackAsset>(); a.Configure("same", HapticType.Light);
            var b = Asset<HapticFeedbackAsset>(); b.Configure("same", HapticType.Success);
            var database = Asset<HapticDatabase>(); Assert.Throws<InvalidOperationException>(() => database.Configure(new[] {a,b}));
            database.Configure(new[] {a}); Assert.That(database.TryGetAsset("same", out var result), Is.True); Assert.That(result, Is.SameAs(a));
            var ui = Asset<UiPunchFeedbackAsset>(); ui.Configure("same", UiPunchOptions.Default);
            var uiDatabase = Asset<UiPunchDatabase>(); uiDatabase.Configure(new[] {ui}); Assert.That(uiDatabase.TryGetAsset("same", out _), Is.True);
        }
        [UnityTest] public IEnumerator UiVfxRunsOnOverlayWhilePausedAndReusesImages()
        {
            var pool = GO("Pool");
            var canvas = GO("Canvas", typeof(RectTransform), typeof(Canvas));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var uiRoot = GO("UiVfxRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            uiRoot.SetParent(canvas.transform, false);
            uiRoot.localRotation = Quaternion.Euler(0, 0, 15);
            var prefab = GO("UiBurst", typeof(RectTransform), typeof(UiBurstVfxInstance)); prefab.SetActive(false);
            var asset = Asset<VfxFeedbackAsset>();
            asset.Configure("ui", new VfxFeedbackSettings { Prefab = prefab, Space = VfxSpace.UI, MaximumDuration = .15f, Scale = 1, PrewarmCount = 1 });
            var vfx = new VfxService(); vfx.Initialize(null, pool.transform, uiRoot); services.Add(vfx);
            var facade = new FeedbackService(new FeedbackServices { Vfx = vfx }, new UnityFeedbackAnimationBackend()); services.Add(facade);
            Time.timeScale = 0;
            var position = uiRoot.position + new Vector3(30, 40, 0);
            var handle = facade.Play(FeedbackRequestBuilder.For(asset).At(position).Build());
            var instance = uiRoot.GetComponentInChildren<UiBurstVfxInstance>();
            Assert.That(instance, Is.Not.Null);
            Assert.That(Vector3.Distance(instance.transform.position, position), Is.LessThan(.001f));
            Assert.That(instance.transform.localRotation, Is.EqualTo(Quaternion.identity), "UI pieces must stay in their Canvas plane.");
            var images = instance.GetComponentsInChildren<UnityEngine.UI.Image>();
            Assert.That(images.Length, Is.EqualTo(12));
            foreach (var image in images) Assert.That(image.raycastTarget, Is.False);
            yield return new WaitForSecondsRealtime(.06f);
            Assert.That(images[0].rectTransform.anchoredPosition.sqrMagnitude, Is.GreaterThan(1));
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(handle.Status, Is.EqualTo(FeedbackStatus.Completed));
            var again = facade.Play(FeedbackRequestBuilder.For(asset).At(position).Build());
            Assert.That(uiRoot.GetComponentInChildren<UiBurstVfxInstance>(), Is.SameAs(instance));
            Assert.That(instance.GetComponentsInChildren<UnityEngine.UI.Image>().Length, Is.EqualTo(12));
            handle.Stop(); Assert.That(again.IsRunning, Is.True);
            again.Stop(); Assert.That(instance.gameObject.activeSelf, Is.False);
        }

        [UnityTest] public IEnumerator UiVfxRespectsScaledTimeAndDisabledOwner()
        {
            var canvas = GO("Canvas", typeof(RectTransform), typeof(Canvas));
            var pool = GO("Pool"); var owner = GO("Owner");
            var prefab = GO("UI", typeof(RectTransform), typeof(UiBurstVfxInstance)); prefab.SetActive(false);
            var vfx = new VfxService(); vfx.Initialize(null, pool.transform, canvas.GetComponent<RectTransform>()); services.Add(vfx);
            var asset = Asset<VfxFeedbackAsset>(); asset.Configure("ui", new VfxFeedbackSettings { Prefab = prefab, Space = VfxSpace.UI, MaximumDuration = .1f, Scale = 1 });
            var facade = new FeedbackService(new FeedbackServices { Vfx = vfx }, new UnityFeedbackAnimationBackend()); services.Add(facade);
            Time.timeScale = 0;
            var handle = facade.Play(FeedbackRequestBuilder.For(asset).From(owner).UseUnscaledTime(false).Build());
            var instance = canvas.GetComponentInChildren<UiBurstVfxInstance>();
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(handle.IsRunning, Is.True);
            Assert.That(instance.GetComponentInChildren<UnityEngine.UI.Image>().rectTransform.anchoredPosition, Is.EqualTo(Vector2.zero));
            owner.SetActive(false); yield return null; yield return null;
            Assert.That(handle.Status, Is.EqualTo(FeedbackStatus.Stopped));
            Assert.That(instance.gameObject.activeSelf, Is.False);
        }

        [UnityTest] public IEnumerator AtUiPlacesVfxAndRewardTextAtAnchor()
        {
            var canvas = GO("Canvas", typeof(RectTransform), typeof(Canvas));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var anchor = GO("RewardAnchor", typeof(RectTransform)).GetComponent<RectTransform>();
            anchor.SetParent(canvas.transform, false);
            anchor.anchoredPosition = new Vector2(120, -80);
            var uiRoot = GO("UiVfxRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            uiRoot.SetParent(canvas.transform, false);
            var textRoot = GO("TextRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            textRoot.SetParent(canvas.transform, false);
            var burstPrefab = GO("Burst", typeof(RectTransform), typeof(UiBurstVfxInstance)); burstPrefab.SetActive(false);
            var textPrefab = GO("Text", typeof(RectTransform), typeof(FloatingTextInstance)); textPrefab.SetActive(false);
            var burstAsset = Asset<VfxFeedbackAsset>();
            burstAsset.Configure("burst", new VfxFeedbackSettings { Prefab = burstPrefab, Space = VfxSpace.UI, MaximumDuration = 1, Scale = 1 });
            var textAsset = Asset<FloatingTextFeedbackAsset>();
            var textSettings = FloatingTextFeedbackSettings.Default;
            textSettings.Prefab = textPrefab; textSettings.Duration = 1; textSettings.MoveOffset = Vector3.zero;
            textAsset.Configure("reward", textSettings);
            var vfx = new VfxService(); vfx.Initialize(null, GO("WorldPool").transform, uiRoot); services.Add(vfx);
            var texts = new FloatingTextService(); texts.Initialize(null, textRoot); services.Add(texts);
            var facade = new FeedbackService(new FeedbackServices { Vfx = vfx, FloatingText = texts }, new UnityFeedbackAnimationBackend()); services.Add(facade);
            var burst = facade.Play(FeedbackRequestBuilder.For(burstAsset).AtUi(anchor).Build());
            var reward = facade.Play(FeedbackRequestBuilder.For(textAsset).AtUi(anchor).WithText("+321").Build());
            yield return null;
            var expected = anchor.TransformPoint(anchor.rect.center);
            var burstInstance = uiRoot.GetComponentInChildren<UiBurstVfxInstance>();
            var textInstance = textRoot.GetComponentInChildren<FloatingTextInstance>();
            Assert.That(burst.IsRunning && reward.IsRunning, Is.True);
            Assert.That(Vector3.Distance(burstInstance.transform.position, expected), Is.LessThan(1));
            Assert.That(Vector3.Distance(textInstance.transform.position, expected), Is.LessThan(1));
            burst.Stop(); reward.Stop();
        }

        private sealed class PendingUi : IUiFeedbackService
        {
            public readonly List<FeedbackPlayback> States = new List<FeedbackPlayback>();
            public FeedbackHandle Punch(Transform target, UiPunchOptions options) { var state = new FeedbackPlayback(); States.Add(state); return state.Handle; }
        }
        private sealed class CaptureUi : IUiFeedbackService
        { public readonly List<float> Values = new List<float>(); public FeedbackHandle Punch(Transform target, UiPunchOptions options) { Values.Add(options.Strength); return FeedbackHandle.Completed; } }
        private sealed class CaptureHaptic : IHapticService
        { public bool Enabled { get; set; } = true; public int Count; public void Play(HapticType type) => Count++; }
    }
}
