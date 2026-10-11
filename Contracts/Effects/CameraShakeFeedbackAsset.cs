using UnityEngine;
namespace Dreamy.Feedback
{
    [CreateAssetMenu(menuName = "Dreamy/Feedback/Effects/Camera Shake")]
    public sealed class CameraShakeFeedbackAsset : FeedbackEffectAsset
    {
        [SerializeField] private CameraShakeOptions settings = CameraShakeOptions.Small;
        
        public CameraShakeOptions Settings => settings;
        public override FeedbackNodeType Type => FeedbackNodeType.CameraShake;
        public void Configure(string id, CameraShakeOptions value)
        { ConfigureIdentity(id); settings = value;  }
        protected override FeedbackNode CreateNode() => new FeedbackNode { Type = Type, DirectAsset = true, Shake = settings };
    }
}

