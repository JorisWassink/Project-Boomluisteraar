using UnityEngine;

namespace TreeClimb
{
    [RequireComponent(typeof(BoxCollider2D), typeof(PlatformEffector2D))]
    public class PlatformerBreakablePlatform : MonoBehaviour
    {
        [Min(0f), Tooltip("Seconds the consumed platform remains visible after the successful bounce.")]
        public float breakDelay = 0.12f;
        public bool IsBreaking { get; private set; }
        float remaining;
        PlatformGenerator generator;
        void Awake() => generator = GetComponentInParent<PlatformGenerator>();

        // Called only by PlayerController AFTER it has applied a valid top-contact bounce.
        // Enemy/seed triggers and underside crossings never call this method.
        public void OnPlayerBounce()
        {
            if (IsBreaking) return;
            IsBreaking = true;
            remaining = Mathf.Max(0f, breakDelay);
            GetComponent<BoxCollider2D>().enabled = false;
            if (remaining <= 0f) Break();
        }
        void Update()
        {
            if (!IsBreaking || (generator != null && !generator.IsRunning)) return;
            remaining -= Time.deltaTime;
            if (remaining <= 0f) Break();
        }
        void Break()
        {
            gameObject.SetActive(false);
            Destroy(gameObject); // Attached enemies and seeds share this lifetime.
        }
    }
}
