using System;
using System.Collections.Generic;
using UnityEngine;
namespace Dreamy.Feedback
{
    public abstract class FeedbackDatabaseAsset : ScriptableObject { public abstract void Rebuild(); }
    public abstract class FeedbackAssetDatabase<T> : FeedbackDatabaseAsset where T : FeedbackAsset
    {
        [SerializeField] private List<T> assets = new List<T>();
        [NonSerialized] private Dictionary<string, T> index;
        public IReadOnlyList<T> Assets => assets;
        public void Configure(IEnumerable<T> values) { assets = new List<T>(values); Rebuild(); }
        public override void Rebuild()
        {
            index = null;
            var next = new Dictionary<string,T>(StringComparer.Ordinal);
            foreach (var asset in assets)
            {
                if (!asset || string.IsNullOrWhiteSpace(asset.Id)) throw new InvalidOperationException(name + ": missing asset/ID.");
                if (next.ContainsKey(asset.Id)) throw new InvalidOperationException(name + ": duplicate ID " + asset.Id);
                next.Add(asset.Id, asset);
            }
            index = next;
        }
        protected virtual void OnValidate() { index = null; }
        public bool TryGetAsset(string id, out T asset)
        {
            if (index == null) Rebuild();
            asset = null; return id != null && index.TryGetValue(id, out asset) && asset;
        }
    }
}
