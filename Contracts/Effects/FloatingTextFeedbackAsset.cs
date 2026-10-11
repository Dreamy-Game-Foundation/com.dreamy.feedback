using UnityEngine;
namespace Dreamy.Feedback
{
    [CreateAssetMenu(menuName = "Dreamy/Feedback/Effects/Floating Text")]
    public sealed class FloatingTextFeedbackAsset : FeedbackEffectAsset
    {
        [SerializeField] private FloatingTextFeedbackSettings settings = FloatingTextFeedbackSettings.Default;
        [SerializeField] private string format = "+{amount}";
        public FloatingTextFeedbackSettings Settings => settings;
        public override FeedbackNodeType Type => FeedbackNodeType.FloatingText;
        public void Configure(string id, FloatingTextFeedbackSettings value, string textFormat = "+{amount}")
        { ConfigureIdentity(id); settings = value; format = textFormat; }
        protected override FeedbackNode CreateNode() => new FeedbackNode { Type = Type, DirectAsset = true, TextSettings = settings, Text = format };
    }
}

