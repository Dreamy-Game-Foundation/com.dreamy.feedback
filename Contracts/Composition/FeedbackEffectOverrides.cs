using System;
using UnityEngine;

namespace Dreamy.Feedback
{
    [Flags]
    public enum FeedbackOverrideFields { None = 0, Duration = 1, Color = 2, Count = 4, Strength = 8, Text = 16, Icon = 32, Prefab = 64, IconOptions = 128, Haptic = 256, FlashOptions = 512, PunchOptions = 1024, ShakeOptions = 2048 }

    /// <summary>Explicit field mask: zero/black/null can be intentional values, not inheritance sentinels.</summary>
    [Serializable]
    public struct FeedbackEffectOverrides
    {
        public FeedbackOverrideFields Fields;
        [Min(0)] public float Duration;
        public Color Color;
        [Range(1,64)] public int Count;
        public float Strength;
        public string Text;
        public Sprite Icon;
        public GameObject Prefab;
        public IconFlyOptions IconOptions;
        public HapticType Haptic;
        public ScreenFlashOptions FlashOptions;
        public UiPunchOptions PunchOptions;
        public CameraShakeOptions ShakeOptions;

        public void Apply(FeedbackNode node)
        {
            var fields = Fields;
            bool Has(FeedbackOverrideFields field) => (fields & field) != 0;
            if (Has(FeedbackOverrideFields.IconOptions) && node.Type == FeedbackNodeType.IconFly) node.IconFly = IconOptions;
            if (Has(FeedbackOverrideFields.Haptic) && node.Type == FeedbackNodeType.Haptic) node.Haptic = Haptic;
            if (Has(FeedbackOverrideFields.FlashOptions) && node.Type == FeedbackNodeType.ScreenFlash) node.Flash = FlashOptions;
            if (Has(FeedbackOverrideFields.PunchOptions) && node.Type == FeedbackNodeType.UiPunch) node.Punch = PunchOptions;
            if (Has(FeedbackOverrideFields.ShakeOptions) && node.Type == FeedbackNodeType.CameraShake) node.Shake = ShakeOptions;
            if (Has(FeedbackOverrideFields.Duration))
                switch (node.Type)
                {
                    case FeedbackNodeType.IconFly: node.IconFly.Duration = Duration; break;
                    case FeedbackNodeType.UiPunch: node.Punch.Duration = Duration; break;
                    case FeedbackNodeType.CameraShake: node.Shake.Duration = Duration; break;
                    case FeedbackNodeType.FloatingText: node.TextSettings.Duration = Duration; break;
                    case FeedbackNodeType.Vfx: node.Duration = Duration; node.VfxSettings.MaximumDuration = Duration; break;
                    case FeedbackNodeType.ScreenFlash:
                        node.Flash.FadeInDuration = Duration * .25f; node.Flash.HoldDuration = 0; node.Flash.FadeOutDuration = Duration * .75f; break;
                    default: node.Duration = Duration; break;
                }
            if (Has(FeedbackOverrideFields.Color))
            {
                if (node.Type == FeedbackNodeType.FloatingText) node.TextSettings.Color = Color;
                if (node.Type == FeedbackNodeType.ScreenFlash) node.Flash.Color = Color;
            }
            if (Has(FeedbackOverrideFields.Count) && node.Type == FeedbackNodeType.IconFly) node.IconFly.Count = Count;
            if (Has(FeedbackOverrideFields.Strength))
            {
                if (node.Type == FeedbackNodeType.UiPunch) node.Punch.Strength = Strength;
                if (node.Type == FeedbackNodeType.CameraShake) node.Shake.Amplitude = Strength;
                if (node.Type == FeedbackNodeType.Vfx) node.VfxSettings.Scale = Strength;
            }
            if (Has(FeedbackOverrideFields.Text) && node.Type == FeedbackNodeType.FloatingText) node.Text = Text;
            if (Has(FeedbackOverrideFields.Icon) && node.Type == FeedbackNodeType.IconFly) node.IconFly.Icon = Icon;
            if (Has(FeedbackOverrideFields.Prefab))
            {
                if (node.Type == FeedbackNodeType.Vfx) node.VfxSettings.Prefab = Prefab;
                if (node.Type == FeedbackNodeType.FloatingText) node.TextSettings.Prefab = Prefab;
            }
        }
    }

    [Serializable]
    public struct VfxFeedbackSettings
    {
        public GameObject Prefab;
        [Min(.01f)] public float MaximumDuration;
        [Min(0)] public int PrewarmCount;
        [Min(0)] public float Scale;
    }

    [Serializable]
    public struct FloatingTextFeedbackSettings
    {
        public GameObject Prefab;
        public Color Color;
        [Min(.01f)] public float Duration;
        public Vector3 MoveOffset;
        public float StartScale, EndScale;
        [Min(0)] public int PrewarmCount;
        public static FloatingTextFeedbackSettings Default => new FloatingTextFeedbackSettings
        { Color = Color.white, Duration = .9f, MoveOffset = new Vector3(0,90,0), StartScale = 1, EndScale = 1.15f };
    }
}
