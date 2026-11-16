using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TreeClimb
{
    public enum PlatformerControlMode { Keyboard, DeviceTilt, TouchButtons }

    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Min(0.1f)] public float moveSpeed = 6f;
        [Min(0.1f)] public float acceleration = 40f;
        [Min(0.1f)] public float deceleration = 48f;
        [InspectorName("Normal Bounce Strength"), Min(0.1f)] public float bounceSpeed = 12f;
        [InspectorName("Boosted Bounce Strength"), Min(0.1f)] public float boostedBounceSpeed = 18f;
        [Min(0.1f)] public float gravityScale = 3f;
        [Min(0.5f)] public float horizontalLimit = 4.7f;
        public bool screenWrapping = true;
        [Tooltip("Optional wind velocity added to horizontal steering.")]
        public float windStrength;
        public PlatformerControlMode controlMode = PlatformerControlMode.Keyboard;
        [Min(0.1f)] public float tiltSensitivity = 3f;
        [Range(0f, 0.9f)] public float tiltDeadZone = 0.08f;
        public bool invertTilt;
        [Min(0f), Tooltip("Response per second; 0 disables smoothing.")]
        public float tiltSmoothing = 10f;

        public Rigidbody2D Body { get; private set; }
        public int BounceCount { get; private set; }
        public bool IsAlive { get; private set; }
        public bool HasBoost { get; private set; }
        public bool IsPaused { get; private set; }
        Vector2 pausedVelocity;
        float pausedAngularVelocity, pausedAt;
        public float HighestY { get; private set; }
        public float LastBounceSpeed { get; private set; }
        public float JumpHeight => bounceSpeed * bounceSpeed / (2f * Gravity);
        public float Gravity => Mathf.Max(0.1f, Mathf.Abs(Physics2D.gravity.y) * gravityScale);
        public bool TiltAvailable => GravitySensor.current != null || Accelerometer.current != null;
        float horizontalInput, smoothedTilt, neutralTilt;
        float verticalSpeedBeforePhysics, knockbackUntil, enemyKnockbackCooldownUntil;
        readonly HashSet<Collider2D> bouncedContacts = new HashSet<Collider2D>();
        BoxCollider2D playerShape;
        float feetBeforePhysics;
        float lastBounceFixedTime = float.NegativeInfinity;
        float lastStompFixedTime = float.NegativeInfinity;
        bool boostBeforeBounce;
        public Bounds StepStartBounds { get; private set; }
        public Vector2 StepVelocity { get; private set; }
        public float StepTime { get; private set; } = float.NegativeInfinity;
        bool touchLeft, touchRight;
        Sensor ownedSensor;

        void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            playerShape = GetComponent<BoxCollider2D>();
            HighestY = Body.position.y;
        }

        void Update()
        {
            if (!IsAlive || IsPaused) return;
            HighestY = Mathf.Max(HighestY, Body.position.y);
            switch (controlMode)
            {
                case PlatformerControlMode.Keyboard:
                    var keyboard = Keyboard.current;
                    horizontalInput = keyboard == null ? 0f :
                        ((keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) ? 1f : 0f) -
                        ((keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) ? 1f : 0f);
                    break;
                case PlatformerControlMode.TouchButtons:
                    horizontalInput = (touchRight ? 1f : 0f) - (touchLeft ? 1f : 0f);
                    break;
                default:
                    float tilt = TiltAvailable ? ReadTilt() - neutralTilt : 0f;
                    float amount = Mathf.Max(0f, Mathf.Abs(tilt) - tiltDeadZone) / (1f - tiltDeadZone);
                    float target = Mathf.Clamp(amount * tiltSensitivity, 0f, 1f) * Mathf.Sign(tilt) * (invertTilt ? -1f : 1f);
                    smoothedTilt = Mathf.Lerp(smoothedTilt, target,
                        tiltSmoothing <= 0f ? 1f : 1f - Mathf.Exp(-tiltSmoothing * Time.deltaTime));
                    horizontalInput = smoothedTilt;
                    break;
            }
        }

        float ReadTilt()
        {
            Sensor sensor = GravitySensor.current != null ? (Sensor)GravitySensor.current : Accelerometer.current;
            if (sensor == null) return 0f;
            if (ownedSensor != null && ownedSensor != sensor) ReleaseSensor();
            if (!sensor.enabled)
            {
                InputSystem.EnableDevice(sensor);
                ownedSensor = sensor;
            }
            return sensor is GravitySensor gravity ? gravity.gravity.ReadValue().x :
                ((Accelerometer)sensor).acceleration.ReadValue().x;
        }

        public void CalibrateTilt() { neutralTilt = ReadTilt(); ResetInput(); }
        public void SetControlMode(PlatformerControlMode mode)
        {
            controlMode = mode;
            ResetInput();
            if (mode == PlatformerControlMode.DeviceTilt) ReadTilt();
            else ReleaseSensor();
        }
        public void SetTouchLeft(bool held) => touchLeft = held;
        public void SetTouchRight(bool held) => touchRight = held;
        public void ResetInput() { horizontalInput = smoothedTilt = 0f; touchLeft = touchRight = false; }
        void ReleaseSensor()
        {
            if (ownedSensor != null && ownedSensor.added) InputSystem.DisableDevice(ownedSensor);
            ownedSensor = null;
        }
        void OnDisable() { ResetInput(); ReleaseSensor(); }
        void OnApplicationFocus(bool focused) { if (!focused) ResetInput(); }
        void OnApplicationPause(bool paused) { if (paused) ResetInput(); }

        public void SetPaused(bool paused)
        {
            if (IsPaused == paused) return;
            IsPaused = paused;
            ResetInput();
            if (paused)
            {
                pausedVelocity = Body.linearVelocity;
                pausedAngularVelocity = Body.angularVelocity;
                pausedAt = Time.time;
                Body.simulated = false;
            }
            else
            {
                float duration = Time.time - pausedAt;
                knockbackUntil += duration;
                enemyKnockbackCooldownUntil += duration;
                Body.linearVelocity = pausedVelocity;
                Body.angularVelocity = pausedAngularVelocity;
                Body.simulated = IsAlive;
            }
        }

        public void ResetRun(Vector2 position, bool playing)
        {
            IsPaused = false;
            if (Body == null) Body = GetComponent<Rigidbody2D>();
            Body.simulated = false;
            Teleport(position);
            Body.linearVelocity = Vector2.zero;
            Body.angularVelocity = 0f;
            HighestY = position.y;
            BounceCount = 0;
            LastBounceSpeed = verticalSpeedBeforePhysics = knockbackUntil = enemyKnockbackCooldownUntil = 0f;
            HasBoost = false;
            lastBounceFixedTime = lastStompFixedTime = StepTime = float.NegativeInfinity;
            bouncedContacts.Clear();
            ResetInput();
            IsAlive = playing;
            Body.gravityScale = gravityScale;
            Body.simulated = playing;
        }

        void Teleport(Vector2 position)
        {
            // Reset interpolation history; never sweep a MovePosition across the level.
            var interpolation = Body.interpolation;
            Body.interpolation = RigidbodyInterpolation2D.None;
            Body.position = position;
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            Body.interpolation = interpolation;
        }

        void FixedUpdate()
        {
            if (!IsAlive || IsPaused) return;
            HighestY = Mathf.Max(HighestY, Body.position.y);
            Body.gravityScale = gravityScale;
            var velocity = Body.linearVelocity;
            if (Time.time >= knockbackUntil)
                velocity.x = Mathf.MoveTowards(velocity.x, horizontalInput * moveSpeed + windStrength,
                    (Mathf.Abs(horizontalInput) < 0.001f ? deceleration : acceleration) * Time.fixedDeltaTime);
            if (Mathf.Abs(Body.position.x) > horizontalLimit)
            {
                if (screenWrapping)
                    Teleport(new Vector2(Mathf.Repeat(Body.position.x + horizontalLimit, horizontalLimit * 2f) - horizontalLimit, Body.position.y));
                else
                {
                    Teleport(new Vector2(Mathf.Clamp(Body.position.x, -horizontalLimit, horizontalLimit), Body.position.y));
                    if (Mathf.Sign(velocity.x) == Mathf.Sign(Body.position.x)) velocity.x = 0f;
                }
            }
            Body.linearVelocity = velocity;
            verticalSpeedBeforePhysics = velocity.y;
            if (playerShape == null) playerShape = GetComponent<BoxCollider2D>();
            feetBeforePhysics = Body.position.y + playerShape.offset.y * transform.lossyScale.y -
                playerShape.size.y * Mathf.Abs(transform.lossyScale.y) * 0.5f;
            StepStartBounds = new Bounds(Body.position + Vector2.Scale(playerShape.offset, transform.lossyScale),
                Vector2.Scale(playerShape.size, new Vector2(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y))));
            StepVelocity = velocity + Vector2.down * (Gravity * Time.fixedDeltaTime);
            StepTime = Time.fixedTime;
        }

        void OnCollisionEnter2D(Collision2D collision) => TryBounce(collision);
        void OnCollisionStay2D(Collision2D collision) => TryBounce(collision);
        void OnCollisionExit2D(Collision2D collision) => bouncedContacts.Remove(collision.collider);
        void TryBounce(Collision2D collision)
        {
            if (!IsAlive || IsPaused || lastBounceFixedTime == Time.fixedTime ||
                collision.collider.GetComponent<PlatformEffector2D>() == null) return;
            foreach (var enemy in collision.collider.GetComponentsInChildren<PlatformEnemy>())
                enemy.ResolvePlatformContact(this);
            if (!IsAlive || lastBounceFixedTime == Time.fixedTime || bouncedContacts.Contains(collision.collider) || verticalSpeedBeforePhysics > 0f) return;
            var mover = collision.collider.GetComponent<PlatformerMovingPlatform>();
            float platformVelocityY = mover != null ? mover.StepVelocity.y :
                collision.rigidbody != null ? collision.rigidbody.linearVelocity.y : 0f;
            float incomingVelocityY = verticalSpeedBeforePhysics - Gravity * Time.fixedDeltaTime;
            if (incomingVelocityY - platformVelocityY > 0f) return;
            // Use incoming geometry and motion, not velocity already changed by the solver.
            if (mover != null && feetBeforePhysics < mover.StepStartTop - Physics2D.defaultContactOffset * 2f) return;
            var breakable = collision.collider.GetComponent<PlatformerBreakablePlatform>();
            if (breakable != null && breakable.IsBreaking) return;
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y < 0.5f) continue;
                ApplyBounce(HasBoost ? boostedBounceSpeed : bounceSpeed, false);
                bouncedContacts.Add(collision.collider);
                if (mover != null) mover.ReleaseBouncingPlayer();
                if (breakable != null) breakable.OnPlayerBounce();
                break;
            }
        }
        // One bounce writer, with enemy stomps taking priority over a same-step platform landing.
        void ApplyBounce(float strength, bool stomp)
        {
            if (lastBounceFixedTime != Time.fixedTime)
            {
                boostBeforeBounce = HasBoost;
                BounceCount++;
            }
            if (stomp) HasBoost = boostBeforeBounce; else HasBoost = false;
            LastBounceSpeed = Mathf.Max(0.1f, strength);
            Body.linearVelocity = new Vector2(Body.linearVelocity.x, LastBounceSpeed);
            lastBounceFixedTime = Time.fixedTime;
            if (stomp) lastStompFixedTime = Time.fixedTime;
        }
        internal bool BounceFromEnemy(float strength)
        {
            if (!IsAlive || IsPaused) return false;
            if (lastStompFixedTime == Time.fixedTime) return true;
            ApplyBounce(strength, true);
            return true;
        }
        public void CollectBoost() { if (IsAlive && !IsPaused) HasBoost = true; }
        public bool TryApplyEnemyKnockback(float enemyX, float horizontal, float vertical, float cooldown, float steeringSuppression = 0.2f)
        {
            horizontal = Mathf.Max(0f, horizontal);
            vertical = Mathf.Max(0f, vertical);
            if (!IsAlive || IsPaused || !Body.simulated || Time.time < enemyKnockbackCooldownUntil ||
                (horizontal == 0f && vertical == 0f)) return false;
            float direction = Body.position.x < enemyX ? -1f : 1f;
            // An opposing incoming velocity must not cancel the outward hit. Keep
            // faster outward motion, and preserve zero-axis/no-hit behavior.
            var velocity = Body.linearVelocity;
            if (horizontal > 0f) velocity.x = direction * Mathf.Max(horizontal, direction * velocity.x);
            if (vertical > 0f) velocity.y -= vertical;
            Body.linearVelocity = velocity;
            if (horizontal > 0f) knockbackUntil = Time.time + Mathf.Max(0f, steeringSuppression);
            enemyKnockbackCooldownUntil = Time.time + Mathf.Max(0f, cooldown);
            return true;
        }
        public void Stop()
        {
            IsAlive = false;
            ResetInput();
            Body.linearVelocity = Vector2.zero;
            Body.simulated = false;
        }
    }
}
