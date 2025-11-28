using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace VaingloryMoba.Core
{
    /// <summary>
    /// Handles all touch input and converts to game actions.
    /// Supports tap-to-move, ability targeting, and drag gestures.
    /// </summary>
    public class TouchInputManager : MonoBehaviour
    {
        public static TouchInputManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float tapThreshold = 0.2f; // Max time for a tap
        [SerializeField] private float dragThreshold = 10f; // Min pixels to count as drag
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask unitLayer;

        [Header("Debug")]
        [SerializeField] private bool debugMode = true;

        // Events
        public UnityEvent<Vector3> OnMoveCommand = new UnityEvent<Vector3>();
        public UnityEvent<Vector3> OnMoveHold = new UnityEvent<Vector3>();
        public UnityEvent<GameObject> OnUnitTapped = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> OnUnitHeld = new UnityEvent<GameObject>();
        public UnityEvent<Vector2> OnTouchBegan = new UnityEvent<Vector2>();
        public UnityEvent<Vector2> OnTouchEnded = new UnityEvent<Vector2>();
        public UnityEvent<Vector2, Vector2> OnDrag = new UnityEvent<Vector2, Vector2>(); // start, current

        // State
        private Camera mainCamera;
        private Dictionary<int, TouchData> activeTouches = new Dictionary<int, TouchData>();
        private bool isAbilityTargeting = false;
        private int abilityTargetingIndex = -1;

        private class TouchData
        {
            public Vector2 startPosition;
            public Vector2 currentPosition;
            public float startTime;
            public bool isDragging;
            public bool isOverUI;
        }

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

            mainCamera = Camera.main;

            // Set up layer masks if not configured
            if (groundLayer == 0)
            {
                // Try to find Ground layer, fallback to Default
                int groundLayerIndex = LayerMask.NameToLayer("Ground");
                if (groundLayerIndex >= 0)
                {
                    groundLayer = 1 << groundLayerIndex;
                }
                else
                {
                    groundLayer = 1 << LayerMask.NameToLayer("Default");
                }
            }

            if (unitLayer == 0)
            {
                // Try to find Unit layer, fallback to Default
                int unitLayerIndex = LayerMask.NameToLayer("Unit");
                if (unitLayerIndex >= 0)
                {
                    unitLayer = 1 << unitLayerIndex;
                }
                else
                {
                    unitLayer = 1 << LayerMask.NameToLayer("Default");
                }
            }
        }

        private void Update()
        {
            // Ensure we have a camera reference
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            // Handle touch input on mobile
            if (Input.touchCount > 0)
            {
                ProcessTouches();
            }
            // Handle mouse input for editor testing
            else if (Application.isEditor)
            {
                ProcessMouseInput();
            }
        }

        private void ProcessTouches()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        HandleTouchBegan(touch.fingerId, touch.position);
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        HandleTouchMoved(touch.fingerId, touch.position);
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        HandleTouchEnded(touch.fingerId, touch.position);
                        break;
                }
            }
        }

        private void ProcessMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleTouchBegan(0, Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                HandleTouchMoved(0, Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                HandleTouchEnded(0, Input.mousePosition);
            }
        }

        private void HandleTouchBegan(int fingerId, Vector2 position)
        {
            // Check if touch is over UI
            // For mouse input (fingerId 0 from ProcessMouseInput), use IsPointerOverGameObject() without argument
            // For touch input, use the actual fingerId
            bool overUI = false;
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                // Mouse uses no argument, touch uses fingerId
                if (Input.touchCount == 0)
                    overUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
                else
                    overUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(fingerId);
            }

            var touchData = new TouchData
            {
                startPosition = position,
                currentPosition = position,
                startTime = Time.time,
                isDragging = false,
                isOverUI = overUI
            };

            activeTouches[fingerId] = touchData;

            if (!overUI)
            {
                OnTouchBegan.Invoke(position);
            }

            if (debugMode)
            {
                Debug.Log($"Touch began: {fingerId} at {position}, overUI: {overUI}");
            }
        }

        private void HandleTouchMoved(int fingerId, Vector2 position)
        {
            if (!activeTouches.TryGetValue(fingerId, out TouchData touchData))
                return;

            if (touchData.isOverUI)
                return;

            touchData.currentPosition = position;

            // Check if we've started dragging
            float distance = Vector2.Distance(touchData.startPosition, position);
            if (!touchData.isDragging && distance > dragThreshold)
            {
                touchData.isDragging = true;
            }

            if (touchData.isDragging)
            {
                OnDrag.Invoke(touchData.startPosition, position);
            }

            // Continuous move command while holding
            float holdTime = Time.time - touchData.startTime;
            if (holdTime > tapThreshold && !isAbilityTargeting)
            {
                Vector3? worldPos = ScreenToGroundPosition(position);
                if (worldPos.HasValue)
                {
                    OnMoveHold.Invoke(worldPos.Value);
                }
            }
        }

        private void HandleTouchEnded(int fingerId, Vector2 position)
        {
            if (!activeTouches.TryGetValue(fingerId, out TouchData touchData))
                return;

            activeTouches.Remove(fingerId);

            if (touchData.isOverUI)
                return;

            OnTouchEnded.Invoke(position);

            float duration = Time.time - touchData.startTime;
            float distance = Vector2.Distance(touchData.startPosition, position);

            // Was this a tap?
            if (duration < tapThreshold && distance < dragThreshold)
            {
                HandleTap(position);
            }

            if (debugMode)
            {
                Debug.Log($"Touch ended: {fingerId}, duration: {duration:F2}s, distance: {distance:F1}px");
            }
        }

        private void HandleTap(Vector2 screenPosition)
        {
            Debug.Log($"HandleTap at screen position: {screenPosition}");

            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            // Check if we tapped a targetable unit (not ground)
            if (Physics.Raycast(ray, out hit, 100f))
            {
                Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");

                // Check the object and its parent for Targetable
                var targetable = hit.collider.GetComponent<Combat.Targetable>();
                if (targetable == null)
                {
                    targetable = hit.collider.GetComponentInParent<Combat.Targetable>();
                }

                if (targetable != null)
                {
                    OnUnitTapped.Invoke(targetable.gameObject);
                    Debug.Log($"Unit tapped: {targetable.gameObject.name}, Team: {targetable.Team}");
                    return;
                }

                // Hit something that's not a unit - use hit point as move target
                Debug.Log($"Move command issued to hit point: {hit.point}");
                OnMoveCommand.Invoke(hit.point);
                return;
            }

            // Fallback: Move to ground position using plane intersection
            Vector3? worldPos = ScreenToGroundPosition(screenPosition);
            if (worldPos.HasValue)
            {
                OnMoveCommand.Invoke(worldPos.Value);
                Debug.Log($"Move command issued to: {worldPos.Value}");
            }
            else
            {
                Debug.LogWarning("Could not find ground position for tap");
            }
        }

        /// <summary>
        /// Convert screen position to world position on ground plane
        /// </summary>
        public Vector3? ScreenToGroundPosition(Vector2 screenPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, groundLayer))
            {
                return hit.point;
            }

            // Fallback: intersect with Y=0 plane
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            if (groundPlane.Raycast(ray, out distance))
            {
                return ray.GetPoint(distance);
            }

            return null;
        }

        /// <summary>
        /// Enter ability targeting mode
        /// </summary>
        public void StartAbilityTargeting(int abilityIndex)
        {
            isAbilityTargeting = true;
            abilityTargetingIndex = abilityIndex;
        }

        /// <summary>
        /// Exit ability targeting mode
        /// </summary>
        public void CancelAbilityTargeting()
        {
            isAbilityTargeting = false;
            abilityTargetingIndex = -1;
        }

        public bool IsAbilityTargeting => isAbilityTargeting;
        public int CurrentAbilityIndex => abilityTargetingIndex;
    }
}
