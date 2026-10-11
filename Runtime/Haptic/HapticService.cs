using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class HapticService : IFeedbackAssetLookup, IHapticService
    {
        private HapticDatabase assetDatabase;
        public void SetDatabase(HapticDatabase value) { if (value) value.Rebuild(); assetDatabase = value; }
        public FeedbackAsset FindAsset(string id) => assetDatabase && assetDatabase.TryGetAsset(id, out var asset) ? asset : null;

        public bool Enabled { get; set; } = true;
        public float MinimumInterval { get; set; } = .1f;
        private float lastPlayTime = float.NegativeInfinity;

        public void Play(HapticType type)
        {
            if (!Enabled || type == HapticType.None || Time.unscaledTime - lastPlayTime < MinimumInterval)
            {
                return;
            }

            // Unity's built-in mobile fallback has one vibration intensity.
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            lastPlayTime = Time.unscaledTime;
            Handheld.Vibrate();
#endif
        }
    }
}
