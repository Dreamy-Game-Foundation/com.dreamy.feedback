using UnityEditor;
using UnityEngine;

namespace Dreamy.Feedback.Editor
{
    public enum FeedbackPreset { ButtonConfirm, ButtonReject, RewardPop, ComboPopup, LevelComplete, SoftShake, SuccessFlash }

    /// <summary>Authoring templates only. Gameplay uses saved definitions, not this factory.</summary>
    public static class FeedbackPresetFactory
    {
        public static FeedbackNode CreateNode(FeedbackNodeType type)
        {
            var node = new FeedbackNode { Type = type };
            switch (type)
            {
                case FeedbackNodeType.Delay: node.Duration = .15f; break;
                case FeedbackNodeType.Vfx: node.Id = "reward"; node.Duration = 1.5f; break;
                case FeedbackNodeType.FloatingText: node.Id = "default"; break;
                case FeedbackNodeType.IconFly:
                    node.IconFly = IconFlyOptions.Create(null, Vector3.zero, Vector3.zero);
                    node.IconFly.Count = 8;
                    node.Required = true;
                    break;
            }
            return node;
        }

        public static FeedbackDefinition Create(FeedbackPreset preset)
        {
            var punch = CreateNode(FeedbackNodeType.UiPunch);
            var haptic = CreateNode(FeedbackNodeType.Haptic);
            var flash = CreateNode(FeedbackNodeType.ScreenFlash);
            flash.Flash.MaxAlpha = .18f;
            var vfx = CreateNode(FeedbackNodeType.Vfx);
            var text = CreateNode(FeedbackNodeType.FloatingText);
            FeedbackNode root;
            switch (preset)
            {
                case FeedbackPreset.ButtonConfirm:
                    haptic.Haptic = HapticType.Success;
                    root = FeedbackNode.Group(FeedbackNodeType.Parallel, punch, haptic); break;
                case FeedbackPreset.ButtonReject:
                    punch.Punch.Strength = .1f; punch.Punch.Duration = .2f;
                    haptic.Haptic = HapticType.Warning;
                    root = FeedbackNode.Group(FeedbackNodeType.Parallel, punch, haptic); break;
                case FeedbackPreset.RewardPop:
                    root = FeedbackNode.Group(FeedbackNodeType.Parallel, vfx, text, haptic); break;
                case FeedbackPreset.ComboPopup:
                    text.Text = "Combo x{amount}";
                    root = FeedbackNode.Group(FeedbackNodeType.Parallel, text, haptic); break;
                case FeedbackPreset.LevelComplete:
                    haptic.Haptic = HapticType.Success;
                    root = FeedbackNode.Group(FeedbackNodeType.Sequence,
                        FeedbackNode.Group(FeedbackNodeType.Parallel, vfx, flash, haptic),
                        CreateNode(FeedbackNodeType.Delay), punch); break;
                case FeedbackPreset.SoftShake:
                    var shake = CreateNode(FeedbackNodeType.CameraShake);
                    shake.Shake.Duration = .18f; shake.Shake.Amplitude = .06f;
                    root = shake; break;
                case FeedbackPreset.SuccessFlash:
                    flash.Flash.Color = new Color(.5f, 1f, .65f);
                    haptic.Haptic = HapticType.Success;
                    root = FeedbackNode.Group(FeedbackNodeType.Parallel, flash, haptic); break;
                default: throw new System.ArgumentOutOfRangeException(nameof(preset));
            }
            var definition = ScriptableObject.CreateInstance<FeedbackDefinition>();
            definition.name = preset.ToString();
            definition.Configure("common." + preset.ToString().ToLowerInvariant(), root);
            return definition;
        }

        [MenuItem("Assets/Create/Dreamy/Feedback/Common Presets/Button Confirm")]
        private static void Confirm() => Save(FeedbackPreset.ButtonConfirm);
        [MenuItem("Assets/Create/Dreamy/Feedback/Common Presets/Button Reject")]
        private static void Reject() => Save(FeedbackPreset.ButtonReject);
        [MenuItem("Assets/Create/Dreamy/Feedback/Common Presets/Reward Pop")]
        private static void Reward() => Save(FeedbackPreset.RewardPop);
        [MenuItem("Assets/Create/Dreamy/Feedback/Common Presets/Combo Popup")]
        private static void Combo() => Save(FeedbackPreset.ComboPopup);
        [MenuItem("Assets/Create/Dreamy/Feedback/Common Presets/Level Complete")]
        private static void Win() => Save(FeedbackPreset.LevelComplete);
        [MenuItem("Assets/Create/Dreamy/Feedback/Common Presets/Soft Shake")]
        private static void Shake() => Save(FeedbackPreset.SoftShake);
        [MenuItem("Assets/Create/Dreamy/Feedback/Common Presets/Success Flash")]
        private static void Flash() => Save(FeedbackPreset.SuccessFlash);

        private static void Save(FeedbackPreset preset)
        {
            ProjectWindowUtil.CreateAsset(Create(preset), preset + ".asset");
        }
    }
}
