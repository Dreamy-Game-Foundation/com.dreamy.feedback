using System;
using System.Collections.Generic;
using UnityEngine;
namespace Dreamy.Feedback
{
    public enum FeedbackNodeType { Sequence, Parallel, Delay, Vfx, Haptic, IconFly, UiPunch, CameraShake, FloatingText, ScreenFlash }
    [Serializable]
    public sealed class FeedbackNode
    {
        [NonSerialized] public string StepPath;
        [NonSerialized] public FeedbackContext? RuntimeContext;
        public FeedbackNodeType Type;
        public bool Required;
        public FeedbackEffectAsset Effect;
        public FeedbackEffectOverrides Overrides;
        [NonSerialized] public bool DirectAsset;
        [NonSerialized] public VfxFeedbackSettings VfxSettings;
        [NonSerialized] public FloatingTextFeedbackSettings TextSettings;
        public string Id;
        [Min(0)] public float Duration;
        public string Text = "+{amount}";
        public HapticType Haptic = HapticType.Light;
        public IconFlyOptions IconFly;
        public UiPunchOptions Punch = UiPunchOptions.Default;
        public CameraShakeOptions Shake = CameraShakeOptions.Small;
        public ScreenFlashOptions Flash = ScreenFlashOptions.WhiteFlash();
        [SerializeReference] public List<FeedbackNode> Children = new List<FeedbackNode>();
        public static FeedbackNode Group(FeedbackNodeType type, params FeedbackNode[] children)
            => new FeedbackNode { Type = type, Children = new List<FeedbackNode>(children) };
        public static FeedbackNode From(FeedbackEffectAsset asset, bool required = true)
            => new FeedbackNode { Type = asset ? asset.Type : FeedbackNodeType.Vfx, Effect = asset, Required = required };
    }
}
