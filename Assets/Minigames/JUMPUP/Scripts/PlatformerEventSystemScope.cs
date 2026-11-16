using UnityEngine;
using UnityEngine.EventSystems;

namespace TreeClimb
{
    // The host's UI input remains authoritative when this scene is loaded additively.
    [DefaultExecutionOrder(-200), RequireComponent(typeof(EventSystem))]
    public class PlatformerEventSystemScope : MonoBehaviour
    {
        void Awake()
        {
            var local = GetComponent<EventSystem>();
            foreach (var other in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
            {
                if (other == local || !other.isActiveAndEnabled) continue;
                foreach (var module in GetComponents<BaseInputModule>()) module.enabled = false;
                local.enabled = false;
                break;
            }
        }
    }
}
