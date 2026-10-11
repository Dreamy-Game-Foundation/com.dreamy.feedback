using UnityEngine;
namespace Dreamy.Feedback
{
    [CreateAssetMenu(menuName = "Dreamy/Feedback/Effects/UI Punch")]
    public sealed class UiPunchFeedbackAsset : FeedbackEffectAsset
    {
        [SerializeField] private UiPunchOptions settings = UiPunchOptions.Default;
        
        public UiPunchOptions Settings => settings;
        public override FeedbackNodeType Type => FeedbackNodeType.UiPunch;
        public void Configure(string id, UiPunchOptions value)
        { ConfigureIdentity(id); settings = value;  }
        protected override FeedbackNode CreateNode() => new FeedbackNode { Type = Type, DirectAsset = true, Punch = settings };
    }
}

