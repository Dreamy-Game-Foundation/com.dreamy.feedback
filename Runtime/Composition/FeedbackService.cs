using System;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace Dreamy.Feedback
{
    public sealed class FeedbackService : IFeedbackAssetLookup, IFeedbackService, IDisposable
    {
        private DefinitionDatabase assetDatabase;
        public void SetDatabase(DefinitionDatabase value) { if (value) value.Rebuild(); assetDatabase = value; }
        public FeedbackAsset FindAsset(string id) => assetDatabase && assetDatabase.TryGetAsset(id, out var asset) ? asset : null;

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
            if (disposed || (!request.Definition && !request.Effect) || request.Cancellation.IsCancellationRequested || active.Count >= MaximumActive) return FeedbackHandle.Invalid;
            if (!ReferenceEquals(request.Context.Source,null) && !request.Context.Source) return default;
            if (request.Context.Source is Behaviour owner && !owner.isActiveAndEnabled ||
                request.Context.Source is GameObject ownerObject && !ownerObject.activeInHierarchy) return default;
            FeedbackNode snapshot;
            try
            {
                snapshot = FeedbackGraphResolver.Resolve(request.Definition ? request.Definition.Resolve() : FeedbackNode.From(request.Effect), request.Overrides);
                ApplyBindings(snapshot, request.Bindings);
                ApplyRuntime(snapshot, request.Context);
                string error = FeedbackGraphValidation.ValidateNode(snapshot);
                if (error != null) { Debug.LogWarning(error, request.Definition ? (UnityEngine.Object)request.Definition : request.Effect); return default; }
            }
            catch (Exception e) { Debug.LogWarning(e.Message); return default; }
            var execution = new NodeExecution(snapshot, request, services, backend);
            var handle = execution.State.Handle;
            active.Add(handle);
            execution.Start();
            ObserveOwner(handle, request).Forget();
            return handle;
        }
        private static void ApplyBindings(FeedbackNode root, FeedbackStepBinding[] bindings)
        {
            if (bindings == null) return;
            var paths = new HashSet<string>();
            foreach (var binding in bindings)
            {
                if (binding == null || string.IsNullOrEmpty(binding.Path) || !paths.Add(binding.Path)) throw new InvalidOperationException("Invalid or duplicate step binding path.");
                var node = Find(root, binding.Path);
                if (node == null || node.Type == FeedbackNodeType.Sequence || node.Type == FeedbackNodeType.Parallel) throw new InvalidOperationException("Binding path must point to an effect step: " + binding.Path);
                binding.Overrides.Apply(node); node.RuntimeContext = binding.Context;
            }
        }
        private static FeedbackNode Find(FeedbackNode node, string path)
        {
            if (node.StepPath == path) return node;
            if (node.Children != null) foreach (var child in node.Children) { var found = Find(child, path); if (found != null) return found; }
            return null;
        }
        private static void ApplyRuntime(FeedbackNode node, FeedbackContext context)
        {
            var c = node.RuntimeContext ?? context;
            if (node.Type == FeedbackNodeType.IconFly) { if (c.Icon) node.IconFly.Icon = c.Icon; if (c.IconCount.HasValue) node.IconFly.Count = c.IconCount.Value; }
            if (node.Type == FeedbackNodeType.FloatingText && c.Text != null) node.Text = c.Text;
            if (node.Children != null) foreach (var child in node.Children) ApplyRuntime(child, c);
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
                            Debug.LogWarning($"Feedback '{request.Id}' skipped optional {node.Type} '{node.Id}'.");
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
                foreach (var child in children)
                {
                    if (State.Status != FeedbackStatus.Running) return;
                    child.Start();
                    if (child.State.Status == FeedbackStatus.Faulted || child.State.Status == FeedbackStatus.Stopped)
                    { Propagate(child.State.Status); return; }
                    WatchSibling(child).Forget();
                }
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
            private void StopChildren()
            {
                Exception error = null;
                try { primitive.Stop(); } catch (Exception e) { error = e; }
                foreach (var child in children) try { child.State.Stop(); } catch (Exception e) { error = e; }
                if (error != null) throw error;
            }
            private void CompleteChildren()
            {
                if (children.Count > 0)
                    foreach (var child in children) { child.Start(); child.State.Complete(); }
                else primitive.Complete();
            }
            private FeedbackHandle Dispatch()
            {
                var c = node.RuntimeContext ?? request.Context;
                switch (node.Type)
                {
                    case FeedbackNodeType.Delay: return backend.Animate(node.Duration, request.UnscaledTime, FeedbackEase.Linear, _ => { });
                    case FeedbackNodeType.Vfx:
                        if(c.Intensity <= 0) return FeedbackHandle.Completed;
                        var vfxOptions = new VfxPlayOptions { Position = c.WorldPosition, Parent = c.Parent, FollowTarget = c.FollowTarget, Scale = c.Intensity, UnscaledTime = request.UnscaledTime };
                        var vfx = node.DirectAsset
                            ? (services.Vfx as IVfxPresetService)?.Play(node.VfxSettings, vfxOptions) ?? default
                            : services.Vfx?.Play(node.Id, vfxOptions) ?? default;
                        if (vfx.IsValid && node.Duration > 0) return Limit(vfx, node.Duration);
                        return vfx;
                    case FeedbackNodeType.Haptic:
                        if (services.Haptic == null) return default;
                        if (c.Intensity > 0) services.Haptic.Play(node.Haptic);
                        return FeedbackHandle.Completed;
                    case FeedbackNodeType.FloatingText:
                        var text = (node.Text ?? "").Replace("{amount}", c.Amount.ToString(CultureInfo.InvariantCulture));
                        return node.DirectAsset
                            ? (services.FloatingText as IFloatingTextPresetService)?.Play(node.TextSettings, text, c.WorldPosition, request.UnscaledTime) ?? default
                            : services.FloatingText?.Play(text, c.WorldPosition, new FloatingTextOptions
                            { StyleId = node.Id, UnscaledTime = request.UnscaledTime,
                                Color = (node.Overrides.Fields & FeedbackOverrideFields.Color) != 0 ? node.TextSettings.Color : (Color?)null,
                                Duration = (node.Overrides.Fields & FeedbackOverrideFields.Duration) != 0 ? node.TextSettings.Duration : (float?)null }) ?? default;
                    case FeedbackNodeType.IconFly:
                        if (node.IconFly.Count == 0) return FeedbackHandle.Completed;
                        if (!c.Target || !c.IconSource || services.IconFly == null) return default;
                        var icons = node.IconFly; icons.StartPosition = c.IconSource.position; icons.EndPosition = c.Target.position;
                        if (c.Icon) icons.Icon = c.Icon;
                        icons.Target = c.Target; icons.UnscaledTime = request.UnscaledTime;
                        return services.IconFly.Fly(icons);
                    case FeedbackNodeType.UiPunch:
                        var punch = node.Punch; punch.Strength *= c.Intensity; punch.UnscaledTime = request.UnscaledTime;
                        return services.Ui?.Punch(c.Target, punch) ?? default;
                    case FeedbackNodeType.CameraShake:
                        var shake = node.Shake; shake.Amplitude *= c.Intensity; shake.UnscaledTime = request.UnscaledTime;
                        return services.CameraShake?.Shake(shake) ?? default;
                    case FeedbackNodeType.ScreenFlash:
                        var flash = node.Flash; flash.UnscaledTime = request.UnscaledTime;
                        return services.Screen?.Flash(flash) ?? default;
                    default: return default;
                }
            }
            private FeedbackHandle Limit(FeedbackHandle effect, float duration)
            {
                var p = new FeedbackPlayback();
                var timer = backend.Animate(duration, request.UnscaledTime, FeedbackEase.Linear, _ => { });
                p.Bind(() => { timer.Stop(); effect.Stop(); }, () => { timer.Complete(); effect.Complete(); });
                EndLimit(p, effect, timer).Forget(); return p.Handle;
            }
            private static async UniTaskVoid EndLimit(FeedbackPlayback p, FeedbackHandle effect, FeedbackHandle timer)
            {
                while (p.Status == FeedbackStatus.Running && effect.IsRunning && timer.IsRunning) await UniTask.Yield();
                if (p.Status != FeedbackStatus.Running) return;
                if (effect.Status == FeedbackStatus.Faulted) { p.Fail(); return; }
                if (effect.Status == FeedbackStatus.Stopped) { p.Stop(); return; }
                effect.Complete(); timer.Stop(); p.Finish();
            }
        }
    }
}
