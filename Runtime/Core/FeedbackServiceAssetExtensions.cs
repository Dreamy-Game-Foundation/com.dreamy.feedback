using System;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace Dreamy.Feedback
{
    public static class FeedbackServiceAssetExtensions
    {
        private static T Lookup<T>(object service, string id) where T : FeedbackAsset
        {
            var asset = (service as IFeedbackAssetLookup)?.FindAsset(id);
            if (asset is T typed) return typed;
            Debug.LogWarning("Feedback ID missing or wrong type: " + id); return null;
        }
        private static FeedbackHandle Fault(string error) { Debug.LogWarning(error); var state = new FeedbackPlayback(); state.Fail(); return state.Handle; }
        private static string Resolve(FeedbackEffectAsset asset, out FeedbackNode node)
        {
            node = null;
            try { node = asset.Resolve(); return FeedbackGraphValidation.ValidateNode(node, false); }
            catch (Exception error) { return error.Message; }
        }
        public static FeedbackHandle Play(this IVfxService service, VfxFeedbackAsset asset, VfxPlayOptions options)
        {
            if (!asset || !(service is IVfxPresetService direct)) return Fault("VFX direct-preset capability/asset missing.");
            var error = Resolve(asset, out var node);
            return error == null ? direct.Play(node.VfxSettings, options) : Fault(error);
        }
        public static FeedbackHandle Play(this IIconFlyService service, string id, IconFlyPlayArgs args) => service.Play(Lookup<IconFlyFeedbackAsset>(service, id), args);
        public static FeedbackHandle Play(this IIconFlyService service, IconFlyFeedbackAsset asset, IconFlyPlayArgs args)
        {
            if (!asset || service == null) return Fault("Icon preset/service missing.");
            if (args.Cancellation.IsCancellationRequested || !OwnerAvailable(args.Owner)) return Stopped();
            var resolveError = Resolve(asset, out var snapshot); if (resolveError != null) return Fault(resolveError);
            var options = snapshot.IconFly;
            if (args.Icon) options.Icon = args.Icon;
            if (args.Count.HasValue) options.Count = args.Count.Value;
            var error = FeedbackGraphValidation.ValidateNode(new FeedbackNode { Type = FeedbackNodeType.IconFly, IconFly = options });
            if (error != null) return Fault(error);
            if (options.Count == 0) return FeedbackHandle.Completed;
            if (!args.Source || !args.Target) return Fault("Icon source and target are required.");
            options.StartPosition = args.Source.position; options.EndPosition = args.Target.position; options.Target = args.Target; options.UnscaledTime = args.UnscaledTime ?? true;
            var handle = service.Fly(options);
            Watch(handle, args.Owner, args.Source, args.Target, args.Cancellation).Forget();
            return handle;
        }
        private static FeedbackHandle Stopped() { var state = new FeedbackPlayback(); state.Stop(); return state.Handle; }
        private static bool OwnerAvailable(UnityEngine.Object owner) => ReferenceEquals(owner, null) || owner && (!(owner is Behaviour b) || b.isActiveAndEnabled) && (!(owner is GameObject g) || g.activeInHierarchy);
        private static async UniTaskVoid Watch(FeedbackHandle handle, UnityEngine.Object owner, Transform source, Transform target, CancellationToken cancellation)
        {
            while (handle.IsRunning)
            {
                if (cancellation.IsCancellationRequested || !OwnerAvailable(owner) || !source || !source.gameObject.activeInHierarchy || !target || !target.gameObject.activeInHierarchy) { handle.Stop(); return; }
                await UniTask.Yield();
            }
        }
        public static FeedbackHandle Play(this IFloatingTextService service, string id, FeedbackContext context, bool unscaledTime = true)
        {
            var asset = Lookup<FloatingTextFeedbackAsset>(service, id);
            if (!asset || !(service is IFloatingTextPresetService direct)) return Fault("Text preset/service missing.");
            var error = Resolve(asset, out var node);
            return error == null ? direct.Play(node.TextSettings, (context.Text ?? node.Text).Replace("{amount}", context.Amount.ToString(CultureInfo.InvariantCulture)), context.WorldPosition, unscaledTime) : Fault(error);
        }
        public static FeedbackHandle Play(this IUiFeedbackService service, string id, Transform target)
        { var asset = Lookup<UiPunchFeedbackAsset>(service, id); if (!asset) return Fault("UI punch preset missing."); var error = Resolve(asset, out var node); return error == null ? service.Punch(target, node.Punch) : Fault(error); }
        public static FeedbackHandle Play(this ICameraShakeService service, string id)
        { var asset = Lookup<CameraShakeFeedbackAsset>(service, id); if (!asset) return Fault("Shake preset missing."); var error = Resolve(asset, out var node); return error == null ? service.Shake(node.Shake) : Fault(error); }
        public static FeedbackHandle Play(this IScreenFeedbackService service, string id)
        { var asset = Lookup<ScreenFlashFeedbackAsset>(service, id); if (!asset) return Fault("Flash preset missing."); var error = Resolve(asset, out var node); return error == null ? service.Flash(node.Flash) : Fault(error); }
        public static FeedbackHandle Play(this IHapticService service, string id)
        { var asset = Lookup<HapticFeedbackAsset>(service, id); if (!asset) return Fault("Haptic preset missing."); var error = Resolve(asset, out var node); if (error != null) return Fault(error); service.Play(node.Haptic); return FeedbackHandle.Completed; }
        public static FeedbackHandle Play(this IFeedbackService service, string id, FeedbackContext context, CancellationToken cancellation = default)
        { var asset = Lookup<FeedbackDefinition>(service, id); return asset ? service.Play(new FeedbackRequest(asset, context, cancellation)) : Fault("Definition preset missing."); }
    }
}
