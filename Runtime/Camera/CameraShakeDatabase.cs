using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Feedback
{
    [CreateAssetMenu(menuName = "Dreamy/Feedback/Camera Shake Database", fileName = "CameraShakeDatabase")]
    public sealed class CameraShakeDatabase : ScriptableObject
    {
        [SerializeField] private List<CameraShakePreset> presets = new List<CameraShakePreset>();

        public IReadOnlyList<CameraShakePreset> Presets => presets;

        public bool TryGet(string id, out CameraShakePreset preset)
        {
            for (var i = 0; i < presets.Count; i++)
            {
                var candidate = presets[i];
                if (candidate != null && candidate.Id == id)
                {
                    preset = candidate;
                    return true;
                }
            }

            preset = null;
            return false;
        }
    }
}
