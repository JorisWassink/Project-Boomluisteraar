using UnityEngine;

namespace TreeClimb
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public PlayerController player;
        [Min(0f)] public float lookAhead = 2f;
        [Min(0.01f)] public float smoothTime = 0.2f;
        [Min(0.01f)] public float downwardSmoothTime = 0.28f;
        [Min(0f)] public float verticalDeadZone = 0.6f;
        [Min(1f)] public float minimumHalfHeight = 7f;
        [Min(1f)] public float minimumHalfWidth = 5.5f;
        float velocity;
        Camera viewCamera;

        Vector3 startingPosition;
        bool capturedStart;
        public void CaptureStart()
        {
            if (capturedStart) return;
            capturedStart = true;
            startingPosition = transform.position;
            viewCamera = GetComponent<Camera>();
        }
        public void ResetRun()
        {
            CaptureStart();
            velocity = 0f;
            transform.position = startingPosition;
            viewCamera.orthographicSize = Mathf.Max(minimumHalfHeight, minimumHalfWidth / viewCamera.aspect);
        }

        void Awake()
        {
            viewCamera = GetComponent<Camera>();
        }

        void LateUpdate()
        {
            viewCamera.orthographicSize = Mathf.Max(minimumHalfHeight, minimumHalfWidth / viewCamera.aspect);
            if (!player.IsAlive || player.IsPaused) return;
            float currentY = transform.position.y;
            float desiredY = player.transform.position.y + lookAhead;
            float targetY = currentY;
            if (desiredY > currentY + verticalDeadZone) targetY = desiredY - verticalDeadZone;
            else if (desiredY < currentY - verticalDeadZone) targetY = desiredY + verticalDeadZone;
            float y = Mathf.SmoothDamp(currentY, targetY, ref velocity,
                targetY < currentY ? downwardSmoothTime : smoothTime);
            transform.position = new Vector3(0f, y, -10f);
        }
    }
}
