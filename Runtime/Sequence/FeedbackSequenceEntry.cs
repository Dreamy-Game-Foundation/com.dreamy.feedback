using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Feedback
{
    [Serializable]
    public sealed class FeedbackSequenceEntry
    {
        [SerializeField] private string id;
        [SerializeField] private List<FeedbackSequenceAction> actions = new List<FeedbackSequenceAction>();

        public string Id => id;
        public IReadOnlyList<FeedbackSequenceAction> Actions => actions;
    }
}
