using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Feedback
{
    [CreateAssetMenu(menuName = "Dreamy/Feedback/Feedback Sequence Database", fileName = "FeedbackSequenceDatabase")]
    public sealed class FeedbackSequenceDatabase : ScriptableObject
    {
        [SerializeField] private List<FeedbackSequenceEntry> entries = new List<FeedbackSequenceEntry>();

        public IReadOnlyList<FeedbackSequenceEntry> Entries => entries;

        public bool TryGet(string id, out FeedbackSequenceEntry entry)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                var candidate = entries[i];
                if (candidate != null && candidate.Id == id)
                {
                    entry = candidate;
                    return true;
                }
            }

            entry = null;
            return false;
        }
    }
}
