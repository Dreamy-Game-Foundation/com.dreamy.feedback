using UnityEngine;
using UnityEngine.UI;
namespace Dreamy.Feedback
{
    public sealed class IconFlyInstance : MonoBehaviour
    {
        [SerializeField] private Image image;
        private CanvasGroup opacity;
        public void Prepare(IconFlyOptions options)
        {
            if (!image) image = GetComponent<Image>() ? GetComponent<Image>() : gameObject.AddComponent<Image>();
            if (!opacity) opacity = GetComponent<CanvasGroup>() ? GetComponent<CanvasGroup>() : gameObject.AddComponent<CanvasGroup>();
            image.sprite = options.Icon; image.enabled = options.Icon; image.raycastTarget = false;
            ((RectTransform)transform).sizeDelta = options.Size == Vector2.zero ? new Vector2(48,48) : options.Size;
            transform.localScale = Vector3.one; transform.localRotation = Quaternion.identity; transform.position = options.StartPosition;
            opacity.alpha = 0; gameObject.SetActive(true);
        }
        public CanvasGroup Opacity => opacity;
    }
}
