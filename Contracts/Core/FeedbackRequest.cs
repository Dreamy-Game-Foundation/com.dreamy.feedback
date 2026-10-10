using System.Threading;
using UnityEngine;
namespace Dreamy.Feedback
{
    public readonly struct FeedbackRequest
    {
        public FeedbackRequest(FeedbackDefinition definition, FeedbackContext context, CancellationToken cancellation = default)
        { Definition = definition; Context = context; Cancellation = cancellation; }
        public FeedbackDefinition Definition { get; }
        public FeedbackContext Context { get; }
        public CancellationToken Cancellation { get; }
    }
    public sealed class FeedbackRequestBuilder
    {
        private readonly FeedbackDefinition definition;
        private Vector3 position;
        private Object source;
        private Sprite icon;
        private Transform target, iconSource;
        private long amount;
        private float intensity = 1;
        private CancellationToken cancellation;
        private FeedbackRequestBuilder(FeedbackDefinition definition) { this.definition = definition; }
        public static FeedbackRequestBuilder For(FeedbackDefinition definition) => new FeedbackRequestBuilder(definition);
        public FeedbackRequestBuilder From(Object value) { source = value; return this; }
        public FeedbackRequestBuilder To(Transform value) { target = value; return this; }
        public FeedbackRequestBuilder IconsFrom(Transform value) { iconSource = value; return this; }
        public FeedbackRequestBuilder At(Vector3 value) { position = value; return this; }
        public FeedbackRequestBuilder WithIcon(Sprite value) { icon = value; return this; }
        public FeedbackRequestBuilder WithAmount(long value) { amount = value; return this; }
        public FeedbackRequestBuilder WithIntensity(float value) { intensity = value; return this; }
        public FeedbackRequestBuilder CancelWith(CancellationToken value) { cancellation = value; return this; }
        public FeedbackRequest Build() => new FeedbackRequest(definition,
            new FeedbackContext(position, source, target, amount, intensity, iconSource: iconSource, icon: icon), cancellation);
    }
}
