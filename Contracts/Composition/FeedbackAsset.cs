using UnityEngine;
namespace Dreamy.Feedback
{
    public abstract class FeedbackAsset : ScriptableObject
    {
        [SerializeField] private string id;
        public string Id => id;
        protected virtual void OnValidate() { if (string.IsNullOrWhiteSpace(id)) id = "feedback." + System.Guid.NewGuid().ToString("N"); }
        public void ConfigureIdentity(string stableId) => id = stableId;
        public abstract FeedbackNode Resolve();
        protected void ValidateIdentity() { if (string.IsNullOrWhiteSpace(id)) throw new System.InvalidOperationException(name + " requires a stable ID."); }
    }
}
