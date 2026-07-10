using System;
using UnityEngine;

namespace Dreamy.Feedback
{
    [Serializable]
    public sealed class CameraShakePreset
    {
        [SerializeField] private string id;
        [SerializeField] private CameraShakeOptions options = CameraShakeOptions.Small;

        public string Id => id;
        public CameraShakeOptions Options => options;
    }
}
