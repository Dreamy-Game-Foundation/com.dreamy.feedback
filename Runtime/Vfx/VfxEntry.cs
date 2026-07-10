using System;
using UnityEngine;

namespace Dreamy.Feedback
{
    [Serializable]
    public sealed class VfxEntry
    {
        [SerializeField] private string id;
        [SerializeField] private GameObject prefab;
        [SerializeField] private int prewarmCount;
        [SerializeField] private VfxDespawnMode despawnMode = VfxDespawnMode.ParticleDuration;
        [SerializeField] private float fixedLifetime = 1f;

        public string Id => id;
        public GameObject Prefab => prefab;
        public int PrewarmCount => prewarmCount;
        public VfxDespawnMode DespawnMode => despawnMode;
        public float FixedLifetime => fixedLifetime;
    }
}
