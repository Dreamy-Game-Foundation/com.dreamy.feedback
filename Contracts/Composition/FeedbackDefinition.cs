using UnityEngine;
namespace Dreamy.Feedback
{
    [CreateAssetMenu(menuName = "Dreamy/Feedback/Definition")]
    public sealed class FeedbackDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeReference] private FeedbackNode root = new FeedbackNode { Type = FeedbackNodeType.Sequence };
        [SerializeField] private bool unscaledTime = true;
        public string Id => id;
        public FeedbackNode Root => root;
        public bool UnscaledTime => unscaledTime;
        public void Configure(string id, FeedbackNode root, bool unscaledTime = true)
        { this.id = id; this.root = root; this.unscaledTime = unscaledTime; }
    }
}
