using System.Collections.Generic;
using UnityEngine;

namespace TreeClimb
{
    public enum PlatformEnemyType { Static, Moving, Shooting }

    [DefaultExecutionOrder(-40)]
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider2D))]
    public class PlatformEnemy : MonoBehaviour
    {
        public PlatformEnemyType enemyType;
        [Min(0f)] public float horizontalKnockback = 6f;
        [Min(0f)] public float verticalKnockback = 4f;
        [Min(0f)] public float knockbackCooldown = 0.8f;
        [Min(0f)] public float steeringSuppression = 0.2f;
        [Header("Moving enemy")]
        public PlatformerMovementAxis movementAxis;
        [Min(0f), Tooltip("Independent movement relative to the supporting platform, in world units/second.")]
        public float movementSpeed = 0.6f;
        [Min(0f), Tooltip("Total horizontal endpoint-to-endpoint distance, fitted inside the platform.")]
        public float horizontalTravelRange = 0.6f;
        [Min(0f), Tooltip("Total rise above the initial position. The lower endpoint stays above the supporting surface.")]
        public float verticalTravelRange = 0.6f;
        [Min(0.1f)] public float stompBounceStrength = 12f;
        [Min(0)] public int killScoreReward = 50;
        public PlatformEnemyType Type => enemyType;
        public bool IsLethal => enemyType != PlatformEnemyType.Static;
        public bool CanBeStomped => IsLethal;
        public bool IsAlive => alive && isActiveAndEnabled;
        public Vector2 StepVelocity { get; private set; }
        public Bounds StepStartBounds { get; private set; }
        public float StepTime { get; private set; } = float.NegativeInfinity;
        public float ActualTravelRange { get; private set; }
        readonly HashSet<PlayerController> touchingPlayers = new HashSet<PlayerController>();
        BoxCollider2D shape;
        PlatformGenerator generator;
        PlatformerMovingPlatform platformMover;
        Vector3 originLocal;
        float phase, allocatedTravel = float.PositiveInfinity;
        bool initialized, alive = true;

        void Awake() { shape = GetComponent<BoxCollider2D>(); shape.isTrigger = true; }
        void Start() { if (!initialized) CaptureOrigin(); }
        void OnDisable() { touchingPlayers.Clear(); StepVelocity = Vector2.zero; }
        void CaptureOrigin()
        {
            generator = GetComponentInParent<PlatformGenerator>();
            platformMover = transform.parent != null ? transform.parent.GetComponent<PlatformerMovingPlatform>() : null;
            originLocal = transform.localPosition;
            var support = transform.parent != null ? transform.parent.GetComponent<BoxCollider2D>() : null;
            if (IsLethal && support != null)
            {
                float enemyHalfHeight = GetComponent<BoxCollider2D>().size.y * Mathf.Abs(transform.lossyScale.y) * 0.5f;
                float minimumY = support.offset.y + support.size.y * 0.5f + enemyHalfHeight / Mathf.Max(0.001f, Mathf.Abs(transform.parent.lossyScale.y));
                originLocal.y = Mathf.Max(originLocal.y, minimumY);
                transform.localPosition = originLocal;
            }
            phase = 0f; initialized = true;
        }
        public void Configure(float horizontal, float vertical, float cooldown, float suppression = 0.2f)
        {
            enemyType = PlatformEnemyType.Static;
            horizontalKnockback = Mathf.Max(0f, horizontal);
            verticalKnockback = Mathf.Max(0f, vertical);
            knockbackCooldown = Mathf.Max(0f, cooldown);
            steeringSuppression = Mathf.Max(0f, suppression);
            CaptureOrigin();
        }
        public void ConfigureMoving(PlatformerMovementAxis axis, float speed, float horizontal, float vertical, float bounce, int reward)
        {
            enemyType = PlatformEnemyType.Moving;
            movementAxis = axis; movementSpeed = Mathf.Max(0f, speed);
            horizontalTravelRange = Mathf.Max(0f, horizontal); verticalTravelRange = Mathf.Max(0f, vertical);
            stompBounceStrength = Mathf.Max(0.1f, bounce); killScoreReward = Mathf.Max(0, reward);
            allocatedTravel = axis == PlatformerMovementAxis.Horizontal ? horizontalTravelRange : verticalTravelRange;
            CaptureOrigin();
        }
        public void ConfigureShooting(float bounce, int reward)
        {
            enemyType = PlatformEnemyType.Shooting;
            stompBounceStrength = Mathf.Max(0.1f, bounce); killScoreReward = Mathf.Max(0, reward);
            CaptureOrigin();
        }
        internal Bounds CurrentPhysicsBounds => PhysicsBounds();
        Bounds PhysicsBounds()
        {
            Vector3 center = transform.TransformPoint(shape.offset);
            if (platformMover != null) center += (Vector3)platformMover.Body.position - platformMover.transform.position;
            return new Bounds(center, Vector2.Scale(shape.size, new Vector2(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y))));
        }
        void FixedUpdate()
        {
            if (!initialized) CaptureOrigin();
            StepStartBounds = PhysicsBounds(); StepTime = Time.fixedTime; StepVelocity = Vector2.zero;
            if (!IsAlive || (generator != null && !generator.IsRunning)) return;
            Vector3 previous = transform.localPosition;
            ActualTravelRange = 0f;
            if (Type == PlatformEnemyType.Moving && transform.parent != null)
            {
                float range = Mathf.Min(allocatedTravel, movementAxis == PlatformerMovementAxis.Horizontal ? horizontalTravelRange : verticalTravelRange);
                var support = transform.parent.GetComponent<BoxCollider2D>();
                if (support != null && movementAxis == PlatformerMovementAxis.Horizontal)
                {
                    float centerX = transform.parent.TransformPoint(support.offset).x;
                    float originX = transform.parent.TransformPoint(originLocal).x;
                    float spare = support.size.x * Mathf.Abs(transform.parent.lossyScale.x) * 0.5f - StepStartBounds.extents.x - Mathf.Abs(originX - centerX);
                    range = Mathf.Min(range, Mathf.Max(0f, spare * 2f));
                }
                ActualTravelRange = Mathf.Max(0f, range);
                if (range > 0f)
                {
                    // Bound per-step motion for trigger overlap at the supported gameplay speeds.
                    float speed = Mathf.Min(movementSpeed, Mathf.Min(StepStartBounds.size.x, StepStartBounds.size.y) * 0.5f / Time.fixedDeltaTime);
                    phase = Mathf.Repeat(phase + speed * Time.fixedDeltaTime, range * 2f);
                    float offset = movementAxis == PlatformerMovementAxis.Horizontal ? Mathf.PingPong(phase + range * 0.5f, range) - range * 0.5f : Mathf.PingPong(phase, range);
                    Vector3 worldOffset = (movementAxis == PlatformerMovementAxis.Horizontal ? Vector3.right : Vector3.up) * offset;
                    transform.localPosition = originLocal + transform.parent.InverseTransformVector(worldOffset);
                }
            }
            Vector2 ownVelocity = transform.parent != null ? (Vector2)transform.parent.TransformVector(transform.localPosition - previous) / Time.fixedDeltaTime : Vector2.zero;
            StepVelocity = ownVelocity + (platformMover != null ? platformMover.StepVelocity : Vector2.zero);
        }
        void OnTriggerEnter2D(Collider2D other) => HandleContact(GetPlayer(other));
        void OnTriggerStay2D(Collider2D other) { if (IsLethal) HandleContact(GetPlayer(other)); }
        void OnTriggerExit2D(Collider2D other)
        {
            var player = GetPlayer(other);
            if (player != null && !player.IsPaused) touchingPlayers.Remove(player);
        }
        // Platform callbacks delegate here too, so stomp priority cannot depend on callback order.
        internal void ResolvePlatformContact(PlayerController player)
        {
            if (IsLethal && IsAlive && shape.isActiveAndEnabled && shape.bounds.Intersects(player.GetComponent<BoxCollider2D>().bounds)) HandleContact(player);
        }
        void HandleContact(PlayerController player)
        {
            if (!IsAlive || player == null || !player.IsAlive || player.IsPaused || !player.Body.simulated) return;
            if (generator == null) generator = GetComponentInParent<PlatformGenerator>();
            var manager = generator != null ? generator.gameManager : null;
            if (manager == null || manager.State != PlatformerRunState.Playing || manager.IsPaused || !generator.IsRunning) return;
            if (!IsLethal)
            {
                if (touchingPlayers.Add(player)) player.TryApplyEnemyKnockback(transform.position.x, horizontalKnockback, verticalKnockback, knockbackCooldown, steeringSuppression);
                return;
            }
            if (ValidStomp(player))
            {
                // Defeat first: no later trigger/collision callback can damage or reward twice.
                alive = false; shape.enabled = false;
                var shooter = GetComponent<PlatformEnemyShooter>();
                if (shooter != null) shooter.StopAttacking();
                player.BounceFromEnemy(stompBounceStrength);
                manager.AwardEnemyBonus(killScoreReward);
                Destroy(gameObject);
                return;
            }
            manager.GameOver();
        }
        bool ValidStomp(PlayerController player)
        {
            if (StepTime != Time.fixedTime || player.StepTime != Time.fixedTime) return false;
            float relativeY = player.StepVelocity.y - StepVelocity.y;
            if (relativeY >= -0.001f) return false;
            float separation = player.StepStartBounds.min.y - StepStartBounds.max.y;
            float tolerance = Physics2D.defaultContactOffset * 2f;
            if (separation < -tolerance) return false;
            float crossing = separation / (-relativeY * Time.fixedDeltaTime);
            if (crossing < -0.01f || crossing > 1.01f) return false;
            float relativeX = (player.StepVelocity.x - StepVelocity.x) * Time.fixedDeltaTime * Mathf.Clamp01(crossing);
            float footInset = player.StepStartBounds.size.x * 0.1f;
            float left = player.StepStartBounds.min.x + footInset + relativeX;
            float right = player.StepStartBounds.max.x - footInset + relativeX;
            // Horizontal overlap must exist when the feet cross the top, not only later from a side.
            return Mathf.Min(right, StepStartBounds.max.x) - Mathf.Max(left, StepStartBounds.min.x) > tolerance;
        }
        static PlayerController GetPlayer(Collider2D other) => other.attachedRigidbody != null ?
            other.attachedRigidbody.GetComponent<PlayerController>() : other.GetComponent<PlayerController>();
    }
}
