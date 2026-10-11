using UnityEngine;
namespace Dreamy.Feedback
{
    [CreateAssetMenu(menuName = "Dreamy/Feedback/Effects/Haptic")]
    public sealed class HapticFeedbackAsset : FeedbackEffectAsset
    {
        [SerializeField] private HapticType settings = HapticType.Light;
        
        public HapticType Settings => settings;
        public override FeedbackNodeType Type => FeedbackNodeType.Haptic;
        public void Configure(string id, HapticType value)
        { ConfigureIdentity(id); settings = value;  }
        protected override FeedbackNode CreateNode() => new FeedbackNode { Type = Type, DirectAsset = true, Haptic = settings };
    }
}

