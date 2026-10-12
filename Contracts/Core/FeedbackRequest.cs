using System.Threading;
using UnityEngine;
namespace Dreamy.Feedback
{
    [System.Serializable]
    public sealed class FeedbackStepBinding
    {
        public string Path;
        public FeedbackEffectOverrides Overrides;
        [System.NonSerialized] public FeedbackContext? Context;
    }

    public readonly struct FeedbackRequest
    {
        public FeedbackRequest(FeedbackDefinition definition, FeedbackContext context, CancellationToken cancellation = default,
            FeedbackEffectOverrides overrides = default, FeedbackStepBinding[] bindings = null)
        { Bindings = bindings; Definition = definition; Effect = null; Context = context; Cancellation = cancellation; Overrides = overrides; UnscaledTime = definition ? definition.UnscaledTime : true; }
        public FeedbackRequest(FeedbackEffectAsset effect, FeedbackContext context, CancellationToken cancellation = default,
            FeedbackEffectOverrides overrides = default, bool unscaledTime = true, FeedbackStepBinding[] bindings = null)
        { Bindings = bindings; Definition = null; Effect = effect; Context = context; Cancellation = cancellation; Overrides = overrides; UnscaledTime = unscaledTime; }
        public FeedbackStepBinding[] Bindings { get; }
        public FeedbackDefinition Definition { get; }
        public FeedbackEffectAsset Effect { get; }
        public FeedbackEffectOverrides Overrides { get; }
        public bool UnscaledTime { get; }
        public string Id => Definition ? Definition.Id : Effect ? Effect.Id : "";
        public FeedbackContext Context { get; }
        public CancellationToken Cancellation { get; }
    }
    public sealed class FeedbackRequestBuilder
    {
        private readonly FeedbackDefinition definition;
        private readonly FeedbackEffectAsset effect;
        private FeedbackEffectOverrides overrides;
        private FeedbackStepBinding[] bindings;
        private int? iconCount;
        private string text;
        private bool unscaledTime = true;
        private Vector3 position;
        private Object source;
        private Sprite icon;
        private Transform target, iconSource;
        private RectTransform uiAnchor;
        private long amount;
        private float intensity = 1;
        private CancellationToken cancellation;
        private FeedbackRequestBuilder(FeedbackDefinition definition) { this.definition = definition; }
        private FeedbackRequestBuilder(FeedbackEffectAsset effect) { this.effect = effect; }
        public static FeedbackRequestBuilder For(FeedbackDefinition definition) => new FeedbackRequestBuilder(definition);
        public static FeedbackRequestBuilder For(FeedbackEffectAsset effect) => new FeedbackRequestBuilder(effect);
        public FeedbackRequestBuilder From(Object value) { source = value; return this; }
        public FeedbackRequestBuilder To(Transform value) { target = value; return this; }
        public FeedbackRequestBuilder IconsFrom(Transform value) { iconSource = value; return this; }
        public FeedbackRequestBuilder At(Vector3 value) { position = value; return this; }
        public FeedbackRequestBuilder AtUi(RectTransform anchor) { uiAnchor = anchor; return this; }
        public FeedbackRequestBuilder WithIcon(Sprite value) { icon = value; return this; }
        public FeedbackRequestBuilder WithIconCount(int value) { iconCount = value; return this; }
        public FeedbackRequestBuilder WithText(string value) { text = value; return this; }
        public FeedbackRequestBuilder WithBindings(params FeedbackStepBinding[] value) { bindings = value; return this; }
        public FeedbackRequestBuilder WithAmount(long value) { amount = value; return this; }
        public FeedbackRequestBuilder WithIntensity(float value) { intensity = value; return this; }
        public FeedbackRequestBuilder CancelWith(CancellationToken value) { cancellation = value; return this; }
        public FeedbackRequestBuilder WithOverrides(FeedbackEffectOverrides value) { overrides = value; return this; }
        public FeedbackRequestBuilder UseUnscaledTime(bool value) { unscaledTime = value; return this; }
        public FeedbackRequest Build()
        {
            var context = new FeedbackContext(position, source, target, amount, intensity, iconSource: iconSource, icon: icon, iconCount: iconCount, text: text, uiAnchor: uiAnchor);
            return effect ? new FeedbackRequest(effect, context, cancellation, overrides, unscaledTime, bindings)
                : new FeedbackRequest(definition, context, cancellation, overrides, bindings);
        }
    }
}
