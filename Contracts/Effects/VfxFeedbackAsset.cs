using UnityEngine;
namespace Dreamy.Feedback
{
    [CreateAssetMenu(menuName = "Dreamy/Feedback/Effects/VFX")]
    public sealed class VfxFeedbackAsset : FeedbackEffectAsset
    {
        [SerializeField] private VfxFeedbackSettings settings = new VfxFeedbackSettings { MaximumDuration = 1, Scale = 1 };
        
        public VfxFeedbackSettings Settings => settings;
        public override FeedbackNodeType Type => FeedbackNodeType.Vfx;
        public void Configure(string id, VfxFeedbackSettings value)
        { ConfigureIdentity(id); settings = value;  }
        protected override FeedbackNode CreateNode() => new FeedbackNode { Type = Type, DirectAsset = true, VfxSettings = settings, Duration = settings.MaximumDuration };
    }
}

