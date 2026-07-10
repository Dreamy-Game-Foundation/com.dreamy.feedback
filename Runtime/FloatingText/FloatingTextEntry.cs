using System;
using UnityEngine;

namespace Dreamy.Feedback
{
    [Serializable]
    public sealed class FloatingTextEntry
    {
        [SerializeField] private string id;
        [SerializeField] private FloatingTextInstance prefab;
        [SerializeField] private Color color = Color.white;
        [SerializeField] private float duration = 0.8f;
        [SerializeField] private Vector3 moveOffset = new Vector3(0f, 80f, 0f);
        [SerializeField] private float startScale = 1f;
        [SerializeField] private float endScale = 1.15f;
        [SerializeField] private int prewarmCount;

        public string Id => id;
        public FloatingTextInstance Prefab => prefab;
        public Color Color => color;
        public float Duration => duration;
        public Vector3 MoveOffset => moveOffset;
        public float StartScale => startScale;
        public float EndScale => endScale;
        public int PrewarmCount => prewarmCount;
    }
}
