using System;

namespace Dreamy.Feedback
{
    public readonly struct VfxHandle
    {
        private readonly VfxInstance instance;
        private readonly Func<bool> isCurrent;
        private readonly Action stop;

        public VfxHandle(VfxInstance instance)
        {
            this.instance = instance;
            isCurrent = null;
            stop = null;
        }

        internal VfxHandle(VfxInstance instance, Func<bool> isCurrent, Action stop)
        {
            this.instance = instance; this.isCurrent = isCurrent; this.stop = stop;
        }

        public bool IsValid => instance && (isCurrent == null || isCurrent());

        public void Stop()
        {
            if (stop != null) { stop(); return; }
            if (instance)
            {
                instance.Stop();
            }
        }
    }
}
