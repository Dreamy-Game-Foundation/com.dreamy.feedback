namespace Dreamy.Feedback
{
    public readonly struct VfxHandle
    {
        private readonly VfxInstance instance;

        public VfxHandle(VfxInstance instance)
        {
            this.instance = instance;
        }

        public bool IsValid => instance;

        public void Stop()
        {
            if (instance)
            {
                instance.Stop();
            }
        }
    }
}
