using UnityEngine;
namespace Dreamy.Feedback
{
    [CreateAssetMenu(menuName = "Dreamy/Feedback/Effects/Screen Flash")]
    public sealed class ScreenFlashFeedbackAsset : FeedbackEffectAsset
    {
        [SerializeField] private ScreenFlashOptions settings = ScreenFlashOptions.WhiteFlash(.2f);
        
        public ScreenFlashOptions Settings => settings;
        public override FeedbackNodeType Type => FeedbackNodeType.ScreenFlash;
        public void Configure(string id, ScreenFlashOptions value)
        { ConfigureIdentity(id); settings = value;  }
        protected override FeedbackNode CreateNode() => new FeedbackNode { Type = Type, DirectAsset = true, Flash = settings };
    }
}

