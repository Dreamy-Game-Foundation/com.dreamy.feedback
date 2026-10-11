using System;
using System.Collections.Generic;
using UnityEngine;
namespace Dreamy.Feedback
{
    public enum FeedbackPlayMode { Sequence, Parallel, Staggered }
    [Serializable]
    public sealed class FeedbackStep
    {
        public FeedbackAsset Asset;
        [Min(0)] public float DelayBefore;
        public bool Required = true;
        public FeedbackStep() { }
        public FeedbackStep(FeedbackAsset asset, float delayBefore = 0, bool required = true) { Asset = asset; DelayBefore = delayBefore; Required = required; }
    }
    [CreateAssetMenu(menuName = "Dreamy/Feedback/Definition")]
    public sealed class FeedbackDefinition : FeedbackAsset
    {
        [SerializeField] private FeedbackPlayMode playMode;
        [SerializeField] private List<FeedbackStep> effects = new List<FeedbackStep>();
        [SerializeField, Min(0)] private float staggerInterval = .1f;
        [SerializeField] private bool unscaledTime = true;
        // Compatibility storage only. New authoring does not serialize graph overrides.
        [SerializeReference, HideInInspector] private FeedbackNode root;
        public bool IsLegacy => root != null;
        public FeedbackNode Root => root ?? Resolve();
        public IReadOnlyList<FeedbackStep> Effects => effects;
        public FeedbackPlayMode PlayMode => playMode;
        public float StaggerInterval => staggerInterval;
        public bool UnscaledTime => unscaledTime;
        public void Configure(string id, FeedbackNode root, bool unscaledTime = true) { ConfigureIdentity(id); this.root = root; effects.Clear(); this.unscaledTime = unscaledTime; }
        public void Configure(string id, FeedbackPlayMode mode, IEnumerable<FeedbackStep> steps, bool unscaledTime = true, float staggerInterval = .1f)
        { ConfigureIdentity(id); root = null; playMode = mode; effects = new List<FeedbackStep>(steps); this.unscaledTime = unscaledTime; this.staggerInterval = staggerInterval; }
        public override FeedbackNode Resolve() => Resolve(new HashSet<FeedbackDefinition>(), 0, "", new Budget());
        private FeedbackNode Resolve(HashSet<FeedbackDefinition> path, int depth, string prefix, Budget budget)
        {
            ValidateIdentity();
            if (depth > 16 || !path.Add(this)) throw new InvalidOperationException("Definition cycle or depth exceeds 16.");
            try
            {
                if (root != null) return FeedbackGraphResolver.Resolve(root);
                if (!Enum.IsDefined(typeof(FeedbackPlayMode), playMode) || !Finite(staggerInterval) || staggerInterval < 0 || effects.Count == 0 || effects.Count > 256)
                    throw new InvalidOperationException("Definition requires valid mode, interval and 1..256 steps.");
                var result = FeedbackNode.Group(playMode == FeedbackPlayMode.Sequence ? FeedbackNodeType.Sequence : FeedbackNodeType.Parallel);
                for (int i = 0; i < effects.Count; i++)
                {
                    var step = effects[i];
                    if (step == null || !step.Asset || !Finite(step.DelayBefore) || step.DelayBefore < 0) throw new InvalidOperationException("Step requires asset and finite non-negative delay.");
                    if (++budget.Count > 256) throw new InvalidOperationException("Expanded definition exceeds 256 steps.");
                    string stepPath = prefix.Length == 0 ? i.ToString() : prefix + "/" + i;
                    var node = step.Asset is FeedbackDefinition definition ? definition.Resolve(path, depth + 1, stepPath, budget) : step.Asset.Resolve();
                    node.StepPath = stepPath;
                    node.Required = step.Required;
                    float delay = step.DelayBefore + (playMode == FeedbackPlayMode.Staggered ? i * staggerInterval : 0);
                    if (!Finite(delay)) throw new InvalidOperationException("Step delay overflow.");
                    if (delay > 0) node = FeedbackNode.Group(FeedbackNodeType.Sequence, new FeedbackNode { Type = FeedbackNodeType.Delay, Duration = delay }, node);
                    result.Children.Add(node);
                }
                var error = FeedbackGraphValidation.ValidateNode(result, false);
                if (error != null) throw new InvalidOperationException(error);
                return result;
            }
            finally { path.Remove(this); }
        }
        private sealed class Budget { public int Count; }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
