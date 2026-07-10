using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Feedback
{
    [CreateAssetMenu(menuName = "Dreamy/Feedback/VFX Database", fileName = "VfxDatabase")]
    public sealed class VfxDatabase : ScriptableObject
    {
        [SerializeField] private List<VfxEntry> entries = new List<VfxEntry>();

        public IReadOnlyList<VfxEntry> Entries => entries;

        public bool TryGet(string id, out VfxEntry entry)
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
