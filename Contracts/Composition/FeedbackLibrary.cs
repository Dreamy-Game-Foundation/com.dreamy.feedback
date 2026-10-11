using System.Collections.Generic;
using UnityEngine;
namespace Dreamy.Feedback
{
    [CreateAssetMenu(menuName = "Dreamy/Feedback/Library")]
    public sealed class FeedbackLibrary : ScriptableObject
    {
        [SerializeField] private List<FeedbackEffectAsset> effects = new List<FeedbackEffectAsset>();
        [SerializeField] private List<FeedbackDefinition> definitions = new List<FeedbackDefinition>();
        public IReadOnlyList<FeedbackEffectAsset> Effects => effects;
        public IReadOnlyList<FeedbackDefinition> Definitions => definitions;
        public void Configure(IEnumerable<FeedbackEffectAsset> effectAssets, IEnumerable<FeedbackDefinition> definitionAssets)
        { effects = new List<FeedbackEffectAsset>(effectAssets); definitions = new List<FeedbackDefinition>(definitionAssets); }
        public FeedbackEffectAsset FindEffect(string id) => effects.Find(e => e && e.Id == id);
        public FeedbackDefinition FindDefinition(string id) => definitions.Find(d => d && d.Id == id);
        public List<string> Validate()
        {
            var errors = new List<string>(); var ids = new HashSet<string>();
            foreach (var effect in effects)
            {
                if (!effect) { errors.Add("Missing effect reference."); continue; }
                if (!ids.Add(effect.Id)) errors.Add("Duplicate effect ID: " + effect.Id);
                try { var error = FeedbackGraphValidation.ValidateNode(effect.Resolve(), false); if (error != null) errors.Add(effect.name + ": " + error); }
                catch (System.Exception e) { errors.Add(effect.name + ": " + e.Message); }
            }
            ids.Clear();
            foreach (var definition in definitions)
            {
                if (!definition) { errors.Add("Missing definition reference."); continue; }
                if (!ids.Add(definition.Id)) errors.Add("Duplicate definition ID: " + definition.Id);
                var error = FeedbackGraphValidation.Validate(definition); if (error != null) errors.Add(definition.name + ": " + error);
            }
            return errors;
        }
    }
}
