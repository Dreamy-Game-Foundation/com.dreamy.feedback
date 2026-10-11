namespace Dreamy.Feedback
{
    public abstract class FeedbackEffectAsset : FeedbackAsset
    {
        public abstract FeedbackNodeType Type { get; }
        protected abstract FeedbackNode CreateNode();
        public override FeedbackNode Resolve() { ValidateIdentity(); var node = CreateNode(); node.Id = Id; return node; }
    }
}
