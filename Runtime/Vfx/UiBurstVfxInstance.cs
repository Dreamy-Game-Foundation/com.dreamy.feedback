using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Feedback
{
    /// <summary>Pooled Canvas-native sparkles/confetti. Works with Overlay, Camera and World Space canvases.</summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Dreamy/Feedback/UI Burst VFX")]
    public sealed class UiBurstVfxInstance : VfxInstance
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color color = new Color(1, .8f, .2f, 1);
        [SerializeField, Range(1, 64)] private int count = 12;
        [SerializeField, Min(0)] private float radius = 100;
        [SerializeField, Min(1)] private float size = 20;
        [SerializeField] private float spin = 180;
        private Image[] pieces;
        private float elapsed, duration = .6f;
        private bool unscaledTime = true;

        public override void ConfigurePlayback(VfxPlayOptions options, float duration)
        {
            unscaledTime = options.UnscaledTime;
            this.duration = Mathf.Max(.01f, duration);
        }

        public override void Play()
        {
            elapsed = 0;
            if (pieces == null)
            {
                pieces = new Image[Mathf.Clamp(count, 1, 64)];
                for (int i = 0; i < pieces.Length; i++)
                {
                    var go = new GameObject("Spark " + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    go.transform.SetParent(transform, false);
                    pieces[i] = go.GetComponent<Image>();
                    pieces[i].sprite = sprite;
                    pieces[i].raycastTarget = false;
                    pieces[i].preserveAspect = true;
                    pieces[i].rectTransform.sizeDelta = Vector2.one * size;
                }
            }
            Render(0);
            base.Play();
        }

        private void Update()
        {
            elapsed += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Render(t);
            if (t >= 1) Stop();
        }

        private void Render(float t)
        {
            if (pieces == null) return;
            float travel = 1 - Mathf.Pow(1 - t, 3);
            for (int i = 0; i < pieces.Length; i++)
            {
                float angle = i * Mathf.PI * 2 / pieces.Length;
                var piece = pieces[i];
                piece.rectTransform.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * travel;
                piece.rectTransform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg + spin * t);
                piece.rectTransform.localScale = Vector3.one * (1 - .7f * t);
                var tint = color;
                tint.a *= 1 - t * t;
                piece.color = tint;
            }
        }
    }
}
