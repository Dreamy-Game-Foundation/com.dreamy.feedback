using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
namespace Dreamy.Feedback.Tests
{
    public sealed class FeedbackCompositeTests
    {
        private readonly List<Object> objects = new List<Object>();
        private FeedbackService service;
        private FeedbackDefinition Definition(FeedbackNode root)
        {
            var d = ScriptableObject.CreateInstance<FeedbackDefinition>(); objects.Add(d); d.Configure("test", root); return d;
        }
        private FeedbackService Service(Clock clock, IHapticService haptic = null, IVfxService vfx = null)
            => service = new FeedbackService(new FeedbackServices { Haptic = haptic, Vfx = vfx }, clock);
        private static FeedbackNode Delay() => new FeedbackNode { Type = FeedbackNodeType.Delay, Duration = 1 };
        [TearDown] public void Cleanup() { service?.Dispose(); foreach(var o in objects) if(o) Object.DestroyImmediate(o); objects.Clear(); Time.timeScale = 1; }
        [UnityTest] public IEnumerator SequenceWaitsForCompletionAndParallelJoinsAll()
        {
            var clock=new Clock(); var d=Definition(FeedbackNode.Group(FeedbackNodeType.Sequence,
                FeedbackNode.Group(FeedbackNodeType.Parallel,Delay(),Delay()),Delay()));
            var h=Service(clock).Play(new FeedbackRequest(d,FeedbackContext.At(Vector3.zero)));
            Assert.That(clock.Plays.Count,Is.EqualTo(2)); clock.Plays[0].Finish(); yield return null;
            Assert.That(clock.Plays.Count,Is.EqualTo(2)); Assert.That(h.IsRunning,Is.True);
            clock.Plays[1].Finish(); for(int frame=0;frame<10 && clock.Plays.Count<3;frame++) yield return null; Assert.That(clock.Plays.Count,Is.EqualTo(3));
            clock.Plays[2].Finish(); for(int frame=0;frame<10 && h.IsRunning;frame++) yield return null; Assert.That(h.Status,Is.EqualTo(FeedbackStatus.Completed));
        }
        [UnityTest] public IEnumerator ConcurrentRequestsStopIndependently()
        {
            var clock=new Clock(); var s=Service(clock); var d=Definition(Delay());
            var a=s.Play(new FeedbackRequest(d,FeedbackContext.At(Vector3.zero))); var b=s.Play(new FeedbackRequest(d,FeedbackContext.At(Vector3.zero)));
            a.Stop(); Assert.That(clock.Plays[0].Status,Is.EqualTo(FeedbackStatus.Stopped)); Assert.That(b.IsRunning,Is.True);
            clock.Plays[1].Finish(); yield return null; Assert.That(b.Status,Is.EqualTo(FeedbackStatus.Completed));
        }
        [Test] public void CompleteRunsPendingOneShotsExactlyOnce()
        {
            var clock=new Clock(); var haptic=new Haptic(); var s=Service(clock,haptic);
            var d=Definition(FeedbackNode.Group(FeedbackNodeType.Sequence,Delay(),new FeedbackNode {Type=FeedbackNodeType.Haptic}));
            var h=s.Play(new FeedbackRequest(d,FeedbackContext.At(Vector3.zero)));
            h.Complete(); h.Complete(); h.Stop(); Assert.That(h.Status,Is.EqualTo(FeedbackStatus.Completed)); Assert.That(haptic.Count,Is.EqualTo(1));
        }
        [Test] public void StopDoesNotStartPendingChildren()
        {
            var clock=new Clock(); var haptic=new Haptic(); var s=Service(clock,haptic);
            var h=s.Play(new FeedbackRequest(Definition(FeedbackNode.Group(FeedbackNodeType.Sequence,Delay(),new FeedbackNode {Type=FeedbackNodeType.Haptic})),FeedbackContext.At(Vector3.zero)));
            h.Stop(); h.Complete(); Assert.That(haptic.Count,Is.Zero); Assert.That(h.Status,Is.EqualTo(FeedbackStatus.Stopped));
        }
        [UnityTest] public IEnumerator RequiredFailureStopsParallelSibling()
        {
            var clock=new Clock(); var s=Service(clock);
            var h=s.Play(new FeedbackRequest(Definition(FeedbackNode.Group(FeedbackNodeType.Parallel,Delay(),new FeedbackNode {Type=FeedbackNodeType.UiPunch,Required=true})),FeedbackContext.At(Vector3.zero)));
            yield return null; Assert.That(h.Status,Is.EqualTo(FeedbackStatus.Faulted)); Assert.That(clock.Plays[0].Status,Is.EqualTo(FeedbackStatus.Stopped));
        }
        [UnityTest] public IEnumerator OwnerDestroyedStopsOnlyItsRequest()
        {
            var clock=new Clock(); var s=Service(clock); var owner=new GameObject("Owner");objects.Add(owner);
            var d=Definition(Delay()); var a=s.Play(new FeedbackRequest(d,new FeedbackContext(Vector3.zero,owner)));
            var b=s.Play(new FeedbackRequest(d,FeedbackContext.At(Vector3.zero)));Object.Destroy(owner);
            yield return null; yield return null; Assert.That(a.Status,Is.EqualTo(FeedbackStatus.Stopped)); Assert.That(b.IsRunning,Is.True);
        }
        [Test] public void BudgetRejectsExcessRequest()
        {
            var clock=new Clock();var s=Service(clock);s.MaximumActive=1;var d=Definition(Delay());
            var a=s.Play(new FeedbackRequest(d,FeedbackContext.At(Vector3.zero)));
            Assert.That(s.Play(new FeedbackRequest(d,FeedbackContext.At(Vector3.zero))).IsValid,Is.False);
            a.Stop();Assert.That(s.Play(new FeedbackRequest(d,FeedbackContext.At(Vector3.zero))).IsValid,Is.True);
        }
        [Test] public void GraphRejectsCycleAndUnboundedVfx()
        {
            var root=FeedbackNode.Group(FeedbackNodeType.Sequence,Delay());root.Children.Add(root);
            Assert.That(FeedbackGraphValidation.Validate(Definition(root)),Does.Contain("cycle"));
            Assert.That(FeedbackGraphValidation.Validate(Definition(new FeedbackNode{Type=FeedbackNodeType.Vfx,Id="loop"})),Does.Contain("maximum duration"));
        }
        [Test] public void BuilderPreservesAmountAndIntensityWithoutGraphMutation()
        {
            var d=Definition(Delay());var target=new GameObject("Target");objects.Add(target);
            var r=FeedbackRequestBuilder.For(d).To(target.transform).WithAmount(long.MaxValue).WithIntensity(2).At(Vector3.one).Build();
            Assert.That(r.Context.Amount,Is.EqualTo(long.MaxValue));Assert.That(r.Context.Intensity,Is.EqualTo(2));Assert.That(r.Context.Target,Is.EqualTo(target.transform));Assert.That(r.Definition.Root.Duration,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator RequestIconOverrideDoesNotMutateDefinition()
        {
            var iconRoot = new GameObject("IconRoot", typeof(RectTransform)); objects.Add(iconRoot);
            var target = new GameObject("Target"); objects.Add(target);
            var source = new GameObject("Source"); objects.Add(source);
            var texture = new Texture2D(4,4); objects.Add(texture);
            var authored = Sprite.Create(texture,new Rect(0,0,2,2),Vector2.zero); objects.Add(authored);
            var overrideIcon = Sprite.Create(texture,new Rect(2,0,2,2),Vector2.zero); objects.Add(overrideIcon);
            var node = new FeedbackNode { Type = FeedbackNodeType.IconFly, Required = true, IconFly = IconFlyOptions.Create(authored,Vector3.zero,Vector3.one) };
            var definition = Definition(node);
            var icons = new IconFlyService(); icons.Initialize(iconRoot.transform);
            var facade = new FeedbackService(new FeedbackServices { IconFly = icons },new UnityFeedbackAnimationBackend());
            var handle = facade.Play(FeedbackRequestBuilder.For(definition).IconsFrom(source.transform).To(target.transform).WithIcon(overrideIcon).Build());
            yield return null;
            Assert.That(iconRoot.GetComponentInChildren<UnityEngine.UI.Image>().sprite, Is.SameAs(overrideIcon));
            Assert.That(definition.Root.IconFly.Icon, Is.SameAs(authored));
            handle.Complete(); facade.Dispose(); icons.Dispose();
        }
        [UnityTest] public IEnumerator IconCompleteAndStaleHandleDoNotStopReusedPool()
        {
            var root=new GameObject("Icons",typeof(RectTransform));objects.Add(root);
            var icons=new IconFlyService();icons.Initialize(root.transform);
            var options=new IconFlyOptions{Count=3,Duration=2,StartPosition=Vector3.zero,EndPosition=Vector3.one};
            var old=icons.Fly(options);old.Complete();Assert.That(old.Status,Is.EqualTo(FeedbackStatus.Completed));
            var current=icons.Fly(options);old.Stop();yield return null;
            Assert.That(current.IsRunning,Is.True);Assert.That(root.transform.childCount,Is.EqualTo(3));
            current.Stop();Assert.That(current.Status,Is.EqualTo(FeedbackStatus.Stopped));icons.Dispose();
        }
        [UnityTest] public IEnumerator UnscaledAnimationFinishesAtTimeScaleZero()
        {
            Time.timeScale=0;var p=new UnityFeedbackAnimationBackend().Animate(.02f,true,FeedbackEase.Linear,_=>{});
            yield return new WaitForSecondsRealtime(.08f);Assert.That(p.Status,Is.EqualTo(FeedbackStatus.Completed));
        }
        private sealed class Haptic : IHapticService { public bool Enabled {get;set;}=true;public int Count;public void Play(HapticType t)=>Count++; }
        private sealed class Clock : IFeedbackAnimationBackend
        {
            public readonly List<FeedbackPlayback> Plays=new List<FeedbackPlayback>();
            public FeedbackHandle Animate(float duration,bool unscaled,FeedbackEase ease,Action<float> update)
            {var p=new FeedbackPlayback();p.Bind(null,()=>update(1));Plays.Add(p);return p.Handle;}
        }
    }
}
