namespace Dreamy.Feedback
{
    public interface IIconFlyService
    {
        void Initialize(UnityEngine.Transform root);
        void Fly(IconFlyOptions options);
        void Clear();
    }
}
