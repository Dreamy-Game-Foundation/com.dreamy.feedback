using UnityEngine;

namespace Dreamy.Feedback
{
    /// <summary>Inspector-friendly entry point. The definition owns the effects; this component owns each request.</summary>
    [AddComponentMenu("Dreamy/Feedback/Feedback Player")]
    public sealed class FeedbackPlayer : MonoBehaviour
    {
        [SerializeField] private FeedbackHost host;
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("definition")] private FeedbackAsset asset;
        [SerializeField] private bool customize;
        [SerializeField] private FeedbackEffectOverrides runtimeSettings;
        [SerializeField] private FeedbackStepBinding[] bindings;
        [SerializeField] private bool useRuntimeCount;
        [SerializeField, Range(0,64)] private int iconCount = 8;
        [SerializeField] private Transform iconSource;
        [SerializeField] private Transform target;
        [SerializeField] private Sprite icon;
        [SerializeField] private long amount = 1;
        public FeedbackHandle LastHandle { get; private set; }

        // UnityEvent entry point: wire a Button.onClick to this method.
        public void Play() => PlayAt(transform.position);
        public FeedbackHandle PlayAt(Vector3 position) => PlayBetween(iconSource, target, amount, icon, position);
        public FeedbackHandle PlayBetween(Transform from, Transform to, long value, Sprite sprite = null)
            => PlayBetween(from, to, value, sprite, transform.position);

        private FeedbackHandle PlayBetween(Transform from, Transform to, long value, Sprite sprite, Vector3 position)
        {
            if (!isActiveAndEnabled || !host || !asset)
            {
                Debug.LogWarning("FeedbackPlayer needs an enabled owner, Host and Asset.", this);
                return default;
            }
            host.Initialize();
            var builder = asset is FeedbackDefinition definition ? FeedbackRequestBuilder.For(definition) : FeedbackRequestBuilder.For((FeedbackEffectAsset)asset);
            builder.From(this).At(position).To(to).IconsFrom(from).WithAmount(value).WithIcon(sprite).WithBindings(bindings);
            if (customize) builder.WithOverrides(runtimeSettings);
            if (useRuntimeCount) builder.WithIconCount(iconCount);
            LastHandle = host.Feedback.Play(builder.Build());
            return LastHandle;
        }
        public void StopLast() => LastHandle.Stop();
        public void CompleteLast() => LastHandle.Complete();
    }
}
