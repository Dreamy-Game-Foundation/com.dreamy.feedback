namespace Dreamy.Feedback
{
    public interface IHapticService
    {
        bool Enabled { get; set; }
        void Play(HapticType type);
    }
}
