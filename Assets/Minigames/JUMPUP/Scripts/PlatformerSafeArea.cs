using UnityEngine;
namespace TreeClimb
{
    [RequireComponent(typeof(RectTransform))]
    public class PlatformerSafeArea : MonoBehaviour
    {
        Rect previous;
        int width, height;
        void OnEnable() => Apply();
        void Update() { if (Screen.safeArea != previous || width != Screen.width || height != Screen.height) Apply(); }
        void Apply()
        {
            width = Screen.width; height = Screen.height; previous = Screen.safeArea;
            if (width <= 0 || height <= 0) return;
            var rect = (RectTransform)transform;
            rect.anchorMin = previous.min / new Vector2(width, height);
            rect.anchorMax = previous.max / new Vector2(width, height);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
