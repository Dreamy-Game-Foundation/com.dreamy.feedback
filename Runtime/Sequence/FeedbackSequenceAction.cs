using System;
using UnityEngine;

namespace Dreamy.Feedback
{
    [Serializable]
    public sealed class FeedbackSequenceAction
    {
        [SerializeField] private FeedbackSequenceActionType type;
        [SerializeField] private float delay;
        [SerializeField] private string id;
        [SerializeField] private string text;
        [SerializeField] private HapticType hapticType = HapticType.Light;
        [SerializeField] private ScreenFlashOptions screenFlashOptions = ScreenFlashOptions.WhiteFlash();
        [SerializeField] private CameraShakeOptions cameraShakeOptions = CameraShakeOptions.Small;
        [SerializeField] private IconFlyOptions iconFlyOptions;

        public FeedbackSequenceActionType Type => type;
        public float Delay => delay;
        public string Id => id;
        public string Text => text;
        public HapticType HapticType => hapticType;
        public ScreenFlashOptions ScreenFlashOptions => screenFlashOptions;
        public CameraShakeOptions CameraShakeOptions => cameraShakeOptions;
        public IconFlyOptions IconFlyOptions => iconFlyOptions;
    }
}
