using UnityEngine;
namespace Dreamy.Feedback
{
    [CreateAssetMenu(menuName = "Dreamy/Feedback/Effects/Icon Fly")]
    public sealed class IconFlyFeedbackAsset : FeedbackEffectAsset
    {
        [SerializeField] private IconFlyFeedbackSettings settings = IconFlyFeedbackSettings.Default;
        
        public IconFlyFeedbackSettings Settings => settings;
        public override FeedbackNodeType Type => FeedbackNodeType.IconFly;
        public void Configure(string id, IconFlyOptions value)
        { ConfigureIdentity(id); settings = IconFlyFeedbackSettings.From(value);  }
        protected override FeedbackNode CreateNode() => new FeedbackNode { Type = Type, DirectAsset = true, IconFly = settings.ToOptions() };
    }
}

