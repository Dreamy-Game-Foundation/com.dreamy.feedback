using System;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace Dreamy.Feedback
{
    public sealed class FeedbackService : IFeedbackService, IDisposable
    {
        private readonly FeedbackServices services;
        private readonly IFeedbackAnimationBackend backend;
        private readonly List<FeedbackHandle> active = new List<FeedbackHandle>();
        public int MaximumActive { get; set; } = 32;
        private bool disposed;
        public FeedbackService(FeedbackServices services, IFeedbackAnimationBackend backend)
        { this.services = services ?? throw new ArgumentNullException(nameof(services)); this.backend = backend ?? throw new ArgumentNullException(nameof(backend)); }
        public FeedbackHandle Play(FeedbackRequest request)
        {
            active.RemoveAll(h => !h.IsRunning);
            if (disposed || !request.Definition || request.Cancellation.IsCancellationRequested || active.Count >= MaximumActive) return FeedbackHandle.Invalid;
            if (!ReferenceEquals(request.Context.Source,null) && !request.Context.Source) return default;
            string error = FeedbackGraphValidation.Validate(request.Definition);
            if (error != null) { Debug.LogWarning(error, request.Definition); return FeedbackHandle.Invalid; }
            var execution = new NodeExecution(request.Definition.Root, request, services, backend);
            var handle = execution.State.Handle;
            active.Add(handle);
            execution.Start();
            ObserveOwner(handle, request).Forget();
            return handle;
        }
        private static async UniTaskVoid ObserveOwner(FeedbackHandle handle, FeedbackRequest request)
        {
            var source = request.Context.Source;
            bool hasSource = source;
            while (handle.IsRunning)
            {
                if (request.Cancellation.IsCancellationRequested || (hasSource && (!source || source is Behaviour b && !b.isActiveAndEnabled || source is GameObject g && !g.activeInHierarchy)))
                { handle.Stop(); return; }
                await UniTask.Yield();
            }
        }
        public void StopAll() { foreach (var handle in active.ToArray()) handle.Stop(); active.Clear(); }
        public void Dispose() { disposed = true; StopAll(); }

        private sealed class NodeExecution
        {
            public readonly FeedbackPlayback State = new FeedbackPlayback();
            private readonly FeedbackNode node;
            private readonly FeedbackRequest request;
            private readonly FeedbackServices services;
            private readonly IFeedbackAnimationBackend backend;
            private readonly List<NodeExecution> children = new List<NodeExecution>();
            private FeedbackHandle primitive;
            private bool started;
            public NodeExecution(FeedbackNode node, FeedbackRequest request, FeedbackServices services, IFeedbackAnimationBackend backend)
            {
                this.node = node; this.request = request; this.services = services; this.backend = backend;
                if (node.Type == FeedbackNodeType.Sequence || node.Type == FeedbackNodeType.Parallel)
                    foreach (var child in node.Children) children.Add(new NodeExecution(child, request, services, backend));
                State.Bind(StopChildren, CompleteChildren);
            }
            public void Start()
            {
                if (started || State.Status != FeedbackStatus.Running) return;
                started = true;
                if (node.Type == FeedbackNodeType.Sequence) RunSequence().Forget();
                else if (node.Type == FeedbackNodeType.Parallel) RunParallel().Forget();
                else
                {
                    try
                    {
                        primitive = Dispatch();
                        if (!primitive.IsValid)
                        {
                            if (node.Required) { State.Fail(); return; }
                            Debug.LogWarning($"Feedback '{request.Definition.Id}' skipped optional {node.Type} '{node.Id}'.");
                            State.Finish(); return;
                        }
                        ObservePrimitive().Forget();
                    }
                    catch (Exception e) { State.Fail(); Debug.LogException(e); }
                }
            }
            private async UniTaskVoid ObservePrimitive()
            {
                var status = await primitive.Completion.AsUniTask();
                Propagate(status);
            }
            private void Propagate(FeedbackStatus status)
            {
                if (status == FeedbackStatus.Completed) State.Finish();
                else if (status == FeedbackStatus.Faulted) State.Fail();
                else State.Stop();
            }
            private async UniTaskVoid RunSequence()
            {
                foreach (var child in children)
                {
                    if (State.Status != FeedbackStatus.Running) return;
                    child.Start();
                    var result = await child.State.Completion.AsUniTask();
                    if (result != FeedbackStatus.Completed) { Propagate(result); return; }
                }
                State.Finish();
            }
            private async UniTaskVoid RunParallel()
            {
                foreach (var child in children) { child.Start(); WatchSibling(child).Forget(); }
                foreach (var child in children)
                {
                    var result = await child.State.Completion.AsUniTask();
                    if (State.Status != FeedbackStatus.Running) return;
                    if (result != FeedbackStatus.Completed) { Propagate(result); return; }
                }
                State.Finish();
            }
            private async UniTaskVoid WatchSibling(NodeExecution child)
            {
                var result = await child.State.Completion.AsUniTask();
                if (result != FeedbackStatus.Completed) Propagate(result);
            }
            private void StopChildren() { primitive.Stop(); foreach (var child in children) child.State.Stop(); }
            private void CompleteChildren()
            {
                if (children.Count > 0)
                    foreach (var child in children) { child.Start(); child.State.Complete(); }
                else primitive.Complete();
            }
            private FeedbackHandle Dispatch()
            {
                var c = request.Context;
                switch (node.Type)
                {
                    case FeedbackNodeType.Delay: return backend.Animate(node.Duration, request.Definition.UnscaledTime, FeedbackEase.Linear, _ => { });
                    case FeedbackNodeType.Vfx:
                        if(c.Intensity <= 0) return FeedbackHandle.Completed;
                        var vfx = services.Vfx?.Play(node.Id, new VfxPlayOptions { Position = c.WorldPosition, Parent = c.Parent, FollowTarget = c.FollowTarget, Scale = c.Intensity, UnscaledTime = request.Definition.UnscaledTime }) ?? default;
                        if (vfx.IsValid && node.Duration > 0) return Limit(vfx, node.Duration);
                        return vfx;
                    case FeedbackNodeType.Haptic:
                        if (services.Haptic == null) return default;
                        if (c.Intensity > 0) services.Haptic.Play(node.Haptic);
                        return FeedbackHandle.Completed;
                    case FeedbackNodeType.FloatingText:
                        return services.FloatingText?.Play((node.Text ?? "").Replace("{amount}", c.Amount.ToString(CultureInfo.InvariantCulture)), c.WorldPosition,
                            new FloatingTextOptions { StyleId = node.Id, UnscaledTime = request.Definition.UnscaledTime }) ?? default;
                    case FeedbackNodeType.IconFly:
                        if (!c.Target || !c.IconSource || services.IconFly == null) return default;
                        var icons = node.IconFly; icons.StartPosition = c.IconSource.position; icons.EndPosition = c.Target.position;
                        if (c.Icon) icons.Icon = c.Icon;
                        icons.Target = c.Target; icons.UnscaledTime = request.Definition.UnscaledTime;
                        return services.IconFly.Fly(icons);
                    case FeedbackNodeType.UiPunch:
                        var punch = node.Punch; punch.Strength *= c.Intensity; punch.UnscaledTime = request.Definition.UnscaledTime;
                        return services.Ui?.Punch(c.Target, punch) ?? default;
                    case FeedbackNodeType.CameraShake:
                        var shake = node.Shake; shake.Amplitude *= c.Intensity; shake.UnscaledTime = request.Definition.UnscaledTime;
                        return services.CameraShake?.Shake(shake) ?? default;
                    case FeedbackNodeType.ScreenFlash:
                        var flash = node.Flash; flash.UnscaledTime = request.Definition.UnscaledTime;
                        return services.Screen?.Flash(flash) ?? default;
                    default: return default;
                }
            }
            private FeedbackHandle Limit(FeedbackHandle effect, float duration)
            {
                var p = new FeedbackPlayback();
                var timer = backend.Animate(duration, request.Definition.UnscaledTime, FeedbackEase.Linear, _ => { });
                p.Bind(() => { timer.Stop(); effect.Stop(); }, () => { timer.Complete(); effect.Complete(); });
                EndLimit(p, effect, timer).Forget(); return p.Handle;
            }
            private static async UniTaskVoid EndLimit(FeedbackPlayback p, FeedbackHandle effect, FeedbackHandle timer)
            {
                while (p.Status == FeedbackStatus.Running && effect.IsRunning && timer.IsRunning) await UniTask.Yield();
                if (p.Status != FeedbackStatus.Running) return;
                if (effect.Status == FeedbackStatus.Stopped || effect.Status == FeedbackStatus.Faulted) { p.Stop(); return; }
                effect.Complete(); timer.Stop(); p.Finish();
            }
        }
    }
}
