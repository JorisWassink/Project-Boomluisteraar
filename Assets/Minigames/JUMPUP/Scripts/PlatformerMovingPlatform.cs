using UnityEngine;

namespace TreeClimb
{
    public enum PlatformerMovementAxis { Horizontal, Vertical }

    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(PlatformEffector2D))]
    public class PlatformerMovingPlatform : MonoBehaviour
    {
        public PlatformerMovementAxis axis;
        [Min(0f)] public float movementSpeed = 0.7f;
        [Min(0f), Tooltip("Maximum displacement either side of the spawn position (half the full travel range).")]
        public float movementDistance = 0.35f;
        [Min(0.5f), Tooltip("World X boundary, including the platform's full width.")]
        public float horizontalBoundary = 4.7f;
        public Vector2 Origin { get; private set; }
        public Rigidbody2D Body { get; private set; }
        // MovePosition velocities are reset by the physics step; retain the incoming step.
        public Vector2 StepVelocity { get; private set; }
        public float StepStartTop { get; private set; }
        public float TravelRange { get => movementDistance * 2f; set => movementDistance = Mathf.Max(0f, value) * 0.5f; }
        public float ActualTravelRange => TravelDistance * 2f;
        public float TravelDistance
        {
            get
            {
                float distance = Mathf.Min(Mathf.Max(0f, movementDistance), generationTravelLimit);
                if (axis == PlatformerMovementAxis.Horizontal)
                    distance = Mathf.Min(distance, Mathf.Max(0f, horizontalBoundary - HalfWidth - Mathf.Abs(Origin.x)));
                return distance;
            }
        }
        public float UpperY => Origin.y + (axis == PlatformerMovementAxis.Vertical ? TravelDistance : 0f) + HalfHeight;
        public float HalfWidth => Shape.size.x * Mathf.Abs(transform.lossyScale.x) * 0.5f;
        public float HalfHeight => Shape.size.y * Mathf.Abs(transform.lossyScale.y) * 0.5f;
        BoxCollider2D shape;
        BoxCollider2D Shape => shape != null ? shape : (shape = GetComponent<BoxCollider2D>());
        BoxCollider2D playerShape;
        PlayerController player;
        PlatformGenerator generator;
        PhysicsMaterial2D movementMaterial;
        float phase;
        float generationTravelLimit = float.PositiveInfinity;

        void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            shape = GetComponent<BoxCollider2D>();
            generator = GetComponentInParent<PlatformGenerator>();
            Origin = Body.position;
            player = generator != null ? generator.player : null;
            playerShape = player != null ? player.GetComponent<BoxCollider2D>() : null;
            Body.bodyType = RigidbodyType2D.Kinematic;
            Body.gravityScale = 0f;
            Body.freezeRotation = true;
            Body.interpolation = RigidbodyInterpolation2D.Interpolate;
            Body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            // Keep horizontal steering independent of friction from a moving surface.
            movementMaterial = new PhysicsMaterial2D("TreeClimb moving surface") { friction = 0f, bounciness = 0f };
            shape.sharedMaterial = movementMaterial;
        }

        public void Configure(PlatformerMovementAxis movementAxis, float speed, float distance, float boundary)
        {
            axis = movementAxis;
            movementSpeed = Mathf.Max(0f, speed);
            movementDistance = generationTravelLimit = Mathf.Max(0f, distance);
            horizontalBoundary = boundary;
            Origin = Body.position;
            phase = 0f;
        }

        void FixedUpdate()
        {
            StepStartTop = Body.position.y + Shape.offset.y * transform.lossyScale.y + HalfHeight;
            StepVelocity = Vector2.zero;
            if (generator != null && !generator.IsRunning) { Body.linearVelocity = Vector2.zero; return; }
            float distance = TravelDistance;
            Vector2 target = Body.position;
            if (distance > 0f && movementSpeed > 0f)
            {
                float speed = Mathf.Min(movementSpeed, Mathf.Max(0.01f, HalfHeight) / Time.fixedDeltaTime);
                phase = Mathf.Repeat(phase + speed * Time.fixedDeltaTime, distance * 4f);
                float offset = Mathf.PingPong(phase + distance, distance * 2f) - distance;
                target = Origin + (axis == PlatformerMovementAxis.Horizontal ? Vector2.right : Vector2.up) * offset;
                StepVelocity = (target - Body.position) / Time.fixedDeltaTime;
            }
            UpdatePlayerCollision();
            Body.MovePosition(target);
        }

        void UpdatePlayerCollision()
        {
            if (player == null || playerShape == null || !player.IsAlive || player.IsPaused ||
                !Shape.isActiveAndEnabled || !playerShape.isActiveAndEnabled || !player.Body.simulated) return;
            float feet = player.Body.position.y + playerShape.offset.y * player.transform.lossyScale.y -
                playerShape.size.y * Mathf.Abs(player.transform.lossyScale.y) * 0.5f;
            // Pair-specific one-way filtering BEFORE simulation prevents a moving surface
            // from catching an ascending player or capping their bounce from underneath.
            bool passThrough = player.Body.linearVelocity.y > 0f || feet < StepStartTop - Physics2D.defaultContactOffset * 2f;
            if (Physics2D.GetIgnoreCollision(Shape, playerShape) != passThrough)
                Physics2D.IgnoreCollision(Shape, playerShape, passThrough);
        }

        public void ReleaseBouncingPlayer()
        {
            if (playerShape != null) Physics2D.IgnoreCollision(Shape, playerShape, true);
        }
        void RestorePlayerCollision()
        {
            if (playerShape != null && shape != null && playerShape.isActiveAndEnabled && shape.isActiveAndEnabled && Physics2D.GetIgnoreCollision(shape, playerShape))
                Physics2D.IgnoreCollision(shape, playerShape, false);
        }
        void OnDisable()
        {
            RestorePlayerCollision();
            StepVelocity = Vector2.zero;
            if (Body != null) Body.linearVelocity = Vector2.zero;
        }
        void OnDestroy() { if (movementMaterial != null) Destroy(movementMaterial); }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Vector2 center = Application.isPlaying && Body != null ? Origin : (Vector2)transform.position;
            float distance = Application.isPlaying && Body != null ? TravelDistance : Mathf.Max(0f, movementDistance);
            Vector2 direction = axis == PlatformerMovementAxis.Horizontal ? Vector2.right : Vector2.up;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(center - direction * distance, center + direction * distance);
            Vector3 size = new Vector3(HalfWidth * 2f, HalfHeight * 2f, 0f);
            Gizmos.DrawWireCube(center - direction * distance, size);
            Gizmos.DrawWireCube(center + direction * distance, size);
        }
#endif
    }
}
