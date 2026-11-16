using System.Collections.Generic;
using UnityEngine;

namespace TreeClimb
{
    [DefaultExecutionOrder(-30), DisallowMultipleComponent, RequireComponent(typeof(PlatformEnemy))]
    public class PlatformEnemyShooter : MonoBehaviour
    {
        public EnemyProjectile projectilePrefab;
        [Min(0.1f), Tooltip("Seconds of eligible visible play between shots. Pauses without resetting or catch-up bursts.")]
        public float fireInterval = 5f;
        [Min(0.1f)] public float projectileSpeed = 3f;
        [Min(0.1f)] public float projectileLifetime = 6f;
        [Min(0.05f), Tooltip("Projectile square width/height in world units.")]
        public float projectileSize = 0.18f;
        [Min(0f), Tooltip("Maximum world distance to the player; both shooter and player must also be on screen.")]
        public float maximumShootingDistance = 7f;
        public float TimeUntilNextShot => remaining;
        public int ShotsFired { get; private set; }
        public int ActiveProjectileCount => projectiles.Count;
        readonly HashSet<EnemyProjectile> projectiles = new HashSet<EnemyProjectile>();
        PlatformEnemy enemy;
        PlatformGenerator generator;
        float remaining;
        bool initialized, stopped;

        void Awake() => enemy = GetComponent<PlatformEnemy>();
        void Start() { if (!initialized) Initialize(); }
        void Initialize()
        {
            if (enemy == null) enemy = GetComponent<PlatformEnemy>();
            generator = GetComponentInParent<PlatformGenerator>();
            remaining = Mathf.Max(0.1f, fireInterval); initialized = true;
        }
        public void Configure(EnemyProjectile prefab, float interval, float speed, float lifetime, float size, float distance)
        {
            projectilePrefab = prefab; fireInterval = Mathf.Max(0.1f, interval);
            projectileSpeed = Mathf.Max(0.1f, speed); projectileLifetime = Mathf.Max(0.1f, lifetime);
            projectileSize = Mathf.Max(0.05f, size); maximumShootingDistance = Mathf.Max(0f, distance);
            stopped = false; ShotsFired = 0;
            Initialize();
        }
        void FixedUpdate()
        {
            if (!initialized) Initialize();
            if (stopped || !enemy.IsAlive || enemy.Type != PlatformEnemyType.Shooting || generator == null || !generator.IsRunning) return;
            var manager = generator.gameManager;
            if (manager == null || manager.State != PlatformerRunState.Playing || manager.IsPaused) return;
            var target = manager.player;
            if (target == null || !target.IsAlive || !target.gameObject.activeInHierarchy || projectilePrefab == null) return;
            Vector2 origin = enemy.CurrentPhysicsBounds.center;
            Vector2 aim = target.Body.position - origin;
            if (aim.sqrMagnitude < 0.0001f || aim.sqrMagnitude > maximumShootingDistance * maximumShootingDistance ||
                !Visible(manager.viewCamera, origin) || !Visible(manager.viewCamera, target.Body.position)) return;
            remaining -= Time.fixedDeltaTime;
            if (remaining > 0f) return;
            remaining = Mathf.Max(0.1f, fireInterval);
            var direction = aim.normalized;
            float muzzleOffset = Mathf.Min(enemy.CurrentPhysicsBounds.extents.magnitude + projectileSize * 0.5f + 0.025f, aim.magnitude * 0.5f);
            var projectile = Instantiate(projectilePrefab, origin + direction * muzzleOffset, Quaternion.identity, generator.transform);
            projectile.Configure(this, enemy, generator, direction * projectileSpeed, projectileLifetime, projectileSize);
            ShotsFired++;
        }
        static bool Visible(Camera camera, Vector3 point)
        {
            if (camera == null) return false;
            Vector3 viewport = camera.WorldToViewportPoint(point);
            return viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
        }
        internal void Track(EnemyProjectile projectile) => projectiles.Add(projectile);
        internal void Forget(EnemyProjectile projectile) => projectiles.Remove(projectile);
        public void StopAttacking() { stopped = true; ClearProjectiles(); }
        void ClearProjectiles()
        {
            foreach (var projectile in new List<EnemyProjectile>(projectiles)) if (projectile != null) projectile.Despawn();
            projectiles.Clear();
        }
        void OnDisable() => ClearProjectiles();
    }
}
