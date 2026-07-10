using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Feedback
{
    [CreateAssetMenu(menuName = "Dreamy/Feedback/Floating Text Database", fileName = "FloatingTextDatabase")]
    public sealed class FloatingTextDatabase : ScriptableObject
    {
        [SerializeField] private List<FloatingTextEntry> entries = new List<FloatingTextEntry>();

        public IReadOnlyList<FloatingTextEntry> Entries => entries;

        public bool TryGet(string id, out FloatingTextEntry entry)
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
