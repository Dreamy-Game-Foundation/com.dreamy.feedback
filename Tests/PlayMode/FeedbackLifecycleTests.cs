using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Dreamy.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Dreamy.Feedback.Tests
{
    public sealed class FeedbackLifecycleTests
    {
        private readonly List<Object> objects = new List<Object>();
        private T Track<T>(T obj) where T : Object { objects.Add(obj); return obj; }
        private static void Set(object obj, string name, object value) => obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var obj in objects) if (obj) Object.Destroy(obj);
            objects.Clear();
            ServiceLocator.Unregister<IHapticService>();
            yield return null;
        }
        [UnityTest] public IEnumerator CancelDuringSequenceDelayDoesNotPlayAction()
        {
            var db = Track(ScriptableObject.CreateInstance<FeedbackSequenceDatabase>());
            var action = new FeedbackSequenceAction();
            Set(action, "type", FeedbackSequenceActionType.Haptic); Set(action, "delay", .1f);
            var entry = new FeedbackSequenceEntry(); Set(entry, "id", "test");
            Set(entry, "actions", new List<FeedbackSequenceAction> { action });
            Set(db, "entries", new List<FeedbackSequenceEntry> { entry });
            var haptic = new FakeHaptic();
            var service = new FeedbackSequenceService();
            service.Initialize(db, new FeedbackSequenceServices { Haptic = haptic });
            service.Play("test", default).Stop();
            yield return new WaitForSeconds(.15f);
            Assert.That(haptic.Count, Is.Zero, "Canceled delay must not dispatch its action.");
        }
        [UnityTest] public IEnumerator StaleShakeHandleDoesNotStopNewShake()
        {
            var camera = Track(new GameObject("TestCamera")).AddComponent<Camera>();
            var service = new CameraShakeService(); service.Initialize(null, camera);
            var old = service.Shake(new CameraShakeOptions { Duration = 1, Amplitude = 2, Frequency = 30 });
            service.Shake(new CameraShakeOptions { Duration = 1, Amplitude = 2, Frequency = 30 });
            old.Stop();
            yield return null; yield return null;
            Assert.That(camera.transform.localPosition.sqrMagnitude, Is.GreaterThan(.00001f));
        }
        private VfxService Vfx(out Transform root)
        {
            root = Track(new GameObject("VfxRoot")).transform;
            var prefab = Track(new GameObject("VfxPrefab")); prefab.AddComponent<VfxInstance>(); prefab.SetActive(false);
            var entry = new VfxEntry(); Set(entry, "id", "test"); Set(entry, "prefab", prefab); Set(entry, "despawnMode", VfxDespawnMode.Manual);
            var db = Track(ScriptableObject.CreateInstance<VfxDatabase>()); Set(db, "entries", new List<VfxEntry> { entry });
            var service = new VfxService(); service.Initialize(db, root); return service;
        }
        [UnityTest] public IEnumerator OldVfxHandleCannotStopReusedInstance()
        {
            var service = Vfx(out var root);
            var old = service.Play("test", Vector3.zero); old.Stop(); old.Stop();
            service.Play("test", Vector3.zero); old.Stop();
            yield return null;
            Assert.That(root.GetChild(0).gameObject.activeSelf, Is.True);
            service.Clear();
        }
        [UnityTest] public IEnumerator ClearDestroysActiveVfx()
        {
            var service = Vfx(out var root); service.Play("test", Vector3.zero); service.Clear();
            yield return null;
            Assert.That(root.childCount, Is.Zero);
        }
        [UnityTest] public IEnumerator ClearDestroysActiveIconsAndCanReinitialize()
        {
            var root = Track(new GameObject("IconRoot", typeof(RectTransform))).transform;
            var service = new IconFlyService(); service.Initialize(root);
            service.Fly(new IconFlyOptions { Count = 8, Duration = 1 }); service.Clear();
            yield return null;
            Assert.That(root.childCount, Is.Zero);
            service.Initialize(root); service.Fly(new IconFlyOptions { Count = 1, Duration = .1f });
            Assert.That(root.childCount, Is.EqualTo(1)); service.Dispose();
        }
        [UnityTest] public IEnumerator RepeatedFloatingTextStopCannotStopReusedLease()
        {
            var root = Track(new GameObject("TextRoot", typeof(RectTransform))).transform;
            var prefab = Track(new GameObject("TextPrefab", typeof(RectTransform))).AddComponent<FloatingTextInstance>(); prefab.gameObject.SetActive(false);
            var db = Track(ScriptableObject.CreateInstance<FloatingTextDatabase>());
            var entry = new FloatingTextEntry(); Set(entry, "id", "default"); Set(entry, "prefab", prefab); Set(entry, "duration", 1f);
            Set(db, "entries", new List<FloatingTextEntry> { entry });
            var service = new FloatingTextService(); service.Initialize(db, root);
            var old = service.Play("first", Vector3.zero); old.Stop(); old.Stop();
            service.Play("second", Vector3.zero); old.Stop();
            yield return null;
            Assert.That(root.childCount, Is.EqualTo(1)); Assert.That(root.GetChild(0).gameObject.activeSelf, Is.True);
            service.Clear(); yield return null; Assert.That(root.childCount, Is.Zero);
        }
        [UnityTest] public IEnumerator FlashStopClearsOverlayAndSuppressesCompletion()
        {
            var obj = Track(new GameObject("ScreenFlash")); var view = obj.AddComponent<ScreenFlashView>();
            int completions = 0;
            var handle = view.Flash(ScreenFlashOptions.WhiteFlash(1), () => completions++);
            yield return null; handle.Stop(); yield return new WaitForSeconds(.1f);
            Assert.That(completions, Is.Zero);
            Assert.That(obj.GetComponentInChildren<UnityEngine.UI.Image>().color.a, Is.Zero);
        }
        [UnityTest] public IEnumerator ShakeRestoresPositionCapturedAtPlayback()
        {
            var camera = Track(new GameObject("MovedCamera")).AddComponent<Camera>();
            var service = new CameraShakeService(); service.Initialize(null, camera);
            var moved = new Vector3(5, 3, -10); camera.transform.localPosition = moved;
            var handle = service.Shake(CameraShakeOptions.Small); yield return null; handle.Stop();
            Assert.That(camera.transform.localPosition, Is.EqualTo(moved)); service.Dispose();
        }
        [Test] public void OldRegistryOwnerDoesNotUnregisterReplacement()
        {
            var first = new FakeHaptic(); var second = new FakeHaptic();
            var old = FeedbackServiceRegistry.RegisterOwned(haptic: first);
            var current = FeedbackServiceRegistry.RegisterOwned(haptic: second);
            old.Dispose(); Assert.That(ServiceLocator.Get<IHapticService>(), Is.SameAs(second));
            current.Dispose(); Assert.That(ServiceLocator.IsRegistered<IHapticService>(), Is.False);
        }
        private sealed class FakeHaptic : IHapticService
        {
            public bool Enabled { get; set; } = true;
            public int Count;
            public void Play(HapticType type) { Count++; }
        }
    }
}
