using UnityEngine;

namespace Dreamy.Feedback
{
    /// <summary>Inspector-friendly entry point. The definition owns the effects; this component owns each request.</summary>
    [AddComponentMenu("Dreamy/Feedback/Feedback Player")]
    public sealed class FeedbackPlayer : MonoBehaviour
    {
        [SerializeField] private FeedbackHost host;
        [SerializeField] private FeedbackDefinition definition;
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
            if (!isActiveAndEnabled || !host || !definition)
            {
                Debug.LogWarning("FeedbackPlayer needs an enabled owner, Host and Definition.", this);
                return default;
            }
            host.Initialize();
            LastHandle = host.Feedback.Play(new FeedbackRequest(definition,
                new FeedbackContext(position, this, to, value, iconSource: from, icon: sprite)));
            return LastHandle;
        }
        public void StopLast() => LastHandle.Stop();
        public void CompleteLast() => LastHandle.Complete();
    }
}
