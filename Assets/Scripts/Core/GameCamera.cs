using UnityEngine;

namespace VaingloryMoba.Core
{
    /// <summary>
    /// Vainglory-style fixed isometric camera.
    /// Fixed angle (60 degrees), smooth pan to keep hero in view.
    /// </summary>
    public class GameCamera : MonoBehaviour
    {
        public static GameCamera Instance { get; private set; }

        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Camera Settings - Top Down")]
        [SerializeField] private float cameraHeight = 90f;
        [SerializeField] private float cameraAngle = 90f;   // Straight down
        [SerializeField] private float cameraYaw = 0f;
        [SerializeField] private float panSpeed = 8f;
        [SerializeField] private float edgePanMargin = 0.05f;

        [Header("Bounds")]
        [SerializeField] private float minX = 0f;    // Map Left
        [SerializeField] private float maxX = 160f;  // Map Right
        [SerializeField] private float minZ = 0f;    // Map Bottom
        [SerializeField] private float maxZ = 40f;   // Map Top

        [Header("Zoom Settings")]
        [SerializeField] private float minHeight = 8f;
        [SerializeField] private float maxHeight = 120f;
        [SerializeField] private float zoomSpeed = 5f;

        [Header("Shake Settings")]
        [SerializeField] private float shakeDecay = 5f;

        // State
        private Vector3 cameraFocusPoint;
        private float shakeIntensity = 0f;
        private Vector3 shakeOffset = Vector3.zero;
        private float targetHeight;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            transform.rotation = Quaternion.Euler(cameraAngle, cameraYaw, 0f);
            targetHeight = cameraHeight;

            // Center on map (160x40)
            cameraFocusPoint = new Vector3(80f, 0f, 20f);
            transform.position = new Vector3(80f, cameraHeight, 20f);
        }

        private void Update()
        {
            // Scroll wheel zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                targetHeight -= scroll * zoomSpeed * 10f;
                targetHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight);
            }

            // Smooth zoom
            cameraHeight = Mathf.Lerp(cameraHeight, targetHeight, Time.deltaTime * 10f);

            // Update camera position for zoom (top-down view)
            transform.position = new Vector3(transform.position.x, cameraHeight, transform.position.z);
        }

        /// <summary>
        /// Instantly snap camera to target position (no smoothing)
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null) return;

            cameraFocusPoint = target.position;
            cameraFocusPoint.y = 0;
            UpdateCameraPosition(true);
        }

        private void LateUpdate()
        {
            // Disabled - keep camera fixed on map center
            return;
        }

        private void UpdateFocusPoint()
        {
            if (target == null) return;

            // Camera directly follows hero - hero stays centered, map moves around them
            Vector3 targetFocus = target.position;
            targetFocus.y = 0;

            // Clamp to map bounds
            targetFocus.x = Mathf.Clamp(targetFocus.x, minX, maxX);
            targetFocus.z = Mathf.Clamp(targetFocus.z, minZ, maxZ);

            // Smooth follow - higher panSpeed = more responsive
            cameraFocusPoint = Vector3.Lerp(cameraFocusPoint, targetFocus, panSpeed * Time.deltaTime);
        }

        private void UpdateShake()
        {
            if (shakeIntensity > 0)
            {
                shakeOffset = Random.insideUnitSphere * shakeIntensity;
                shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, shakeDecay * Time.deltaTime);

                if (shakeIntensity < 0.01f)
                {
                    shakeIntensity = 0f;
                    shakeOffset = Vector3.zero;
                }
            }
        }

        private void UpdateCameraPosition(bool instant)
        {
            // Calculate camera position based on angle and height
            // Camera looks diagonally, so offset is in both X and Z
            float pitchRad = cameraAngle * Mathf.Deg2Rad;
            float yawRad = cameraYaw * Mathf.Deg2Rad;
            float horizontalDist = cameraHeight / Mathf.Tan(pitchRad);

            // Offset in the direction opposite to where camera is looking
            Vector3 targetPosition = new Vector3(
                cameraFocusPoint.x - Mathf.Sin(yawRad) * horizontalDist,
                cameraHeight,
                cameraFocusPoint.z - Mathf.Cos(yawRad) * horizontalDist
            ) + shakeOffset;

            if (instant)
            {
                transform.position = targetPosition;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, panSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Set the camera's follow target
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                SnapToTarget();
            }
        }

        /// <summary>
        /// Trigger camera shake effect
        /// </summary>
        public void Shake(float intensity)
        {
            shakeIntensity = Mathf.Max(shakeIntensity, intensity);
        }

        /// <summary>
        /// Adjust camera height (for pinch zoom on mobile)
        /// </summary>
        public void SetHeight(float height)
        {
            cameraHeight = Mathf.Clamp(height, 15f, 30f);
        }

        public Transform Target => target;
        public float CameraHeight => cameraHeight;
    }
}
