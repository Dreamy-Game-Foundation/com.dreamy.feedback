using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class ParticleVfxInstance : VfxInstance
    {
        [SerializeField] private ParticleSystem[] particleSystems;

        public override void Play()
        {
            base.Play();
            CacheParticles();
            for (var i = 0; i < particleSystems.Length; i++)
            {
                if (particleSystems[i])
                {
                    particleSystems[i].Play(true);
                }
            }
        }

        public override void Stop()
        {
            CacheParticles();
            for (var i = 0; i < particleSystems.Length; i++)
            {
                if (particleSystems[i])
                {
                    particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            base.Stop();
        }

        public float GetDuration()
        {
            CacheParticles();
            var maxDuration = 0f;
            for (var i = 0; i < particleSystems.Length; i++)
            {
                var particle = particleSystems[i];
                if (!particle)
                {
                    continue;
                }

                var main = particle.main;
                var duration = main.duration + main.startLifetime.constantMax;
                if (duration > maxDuration)
                {
                    maxDuration = duration;
                }
            }

            return maxDuration;
        }

        private void CacheParticles()
        {
            if (particleSystems == null || particleSystems.Length == 0)
            {
                particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            }
        }
    }
}
