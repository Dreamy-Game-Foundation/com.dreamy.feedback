using UnityEngine;

namespace Dreamy.Feedback
{
    public interface IVfxService
    {
        void Initialize(VfxDatabase database, Transform root);
        VfxHandle Play(string id, Vector3 worldPosition);
        VfxHandle Play(string id, VfxPlayOptions options);
        void Prewarm();
        void Clear();
    }
}
