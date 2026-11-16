using UnityEngine;

namespace TreeClimb
{
    public class LeafPowerup : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player == null || !player.IsAlive) return;
            player.CollectBoost();
            Destroy(gameObject);
        }
    }
}
