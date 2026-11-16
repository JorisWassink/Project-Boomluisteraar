using UnityEngine;

namespace TreeClimb
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        public Vector2 Velocity { get; private set; }
        public float RemainingLifetime => remaining;
        public Rigidbody2D Body { get; private set; }
        public PlatformEnemy Owner => owner;
        PlatformEnemyShooter shooter;
        PlatformEnemy owner;
        PlatformGenerator generator;
        float remaining;
        bool initialized, spent;
        void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            Body.bodyType = RigidbodyType2D.Kinematic;
            Body.gravityScale = 0; Body.freezeRotation = true;
            Body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            GetComponent<BoxCollider2D>().isTrigger = true;
        }
        public void Configure(PlatformEnemyShooter source, PlatformEnemy enemy, PlatformGenerator run, Vector2 velocity, float lifetime, float size)
        {
            shooter = source; owner = enemy; generator = run; Velocity = velocity;
            remaining = Mathf.Max(0.1f, lifetime); spent = false; initialized = true;
            transform.localScale = Vector3.one * Mathf.Max(0.05f, size);
            Body.position = transform.position;
            if (shooter != null) shooter.Track(this);
        }
        void FixedUpdate()
        {
            if (!initialized || spent) return;
            var manager = generator != null ? generator.gameManager : null;
            if (owner == null || !owner.IsAlive || manager == null || manager.State != PlatformerRunState.Playing) { Despawn(); return; }
            if (manager.IsPaused || !generator.IsRunning) { Body.linearVelocity = Vector2.zero; return; }
            remaining -= Time.fixedDeltaTime;
            if (remaining <= 0f || OutsideUsefulArea(manager)) { Despawn(); return; }
            Body.MovePosition(Body.position + Velocity * Time.fixedDeltaTime);
        }
        bool OutsideUsefulArea(GameManager manager)
        {
            var camera = manager.viewCamera;
            if (camera == null) return true;
            const float margin = 2f;
            return Mathf.Abs(Body.position.x) > manager.player.horizontalLimit + margin ||
                Mathf.Abs(Body.position.y - camera.transform.position.y) > camera.orthographicSize + margin;
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!initialized || spent) return;
            var player = other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<PlayerController>() : other.GetComponent<PlayerController>();
            var manager = generator != null ? generator.gameManager : null;
            if (player == null || !player.IsAlive || !player.Body.simulated || player.IsPaused || manager == null || manager.IsPaused || manager.State != PlatformerRunState.Playing) return;
            // A simultaneous stomp of this shot's owner retains the shared enemy resolver's priority.
            if (owner != null) owner.ResolvePlatformContact(player);
            if (spent || owner == null || !owner.IsAlive || !player.IsAlive) { Despawn(); return; }
            Despawn();
            manager.GameOver();
        }
        public void Despawn()
        {
            if (spent) return;
            spent = true;
            if (shooter != null) shooter.Forget(this);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        void OnDestroy() { if (shooter != null) shooter.Forget(this); }
    }
}
