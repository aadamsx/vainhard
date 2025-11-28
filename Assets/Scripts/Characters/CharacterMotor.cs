using UnityEngine;
using UnityEngine.AI;

namespace VaingloryMoba.Characters
{
    /// <summary>
    /// Handles character movement with smooth interpolation.
    /// Uses NavMesh for pathfinding with custom smoothing on top.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class CharacterMotor : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float baseMoveSpeed = 3.3f;
        [SerializeField] private float acceleration = 15f;
        [SerializeField] private float deceleration = 20f;
        [SerializeField] private float rotationSpeed = 720f;

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.05f;
        [SerializeField] private float stoppingDistance = 0.1f;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject moveIndicatorPrefab;

        // Components
        private NavMeshAgent agent;
        private CharacterStats stats;

        // State
        private Vector3 currentVelocity;
        private Vector3 smoothVelocity;
        private Vector3? targetPosition;
        private Transform followTarget;
        private bool isMoving;
        private float currentSpeed;

        // Move indicator
        private GameObject moveIndicator;

        public bool IsMoving => isMoving;
        public float CurrentSpeed => currentSpeed;
        public Vector3? Destination => targetPosition;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            stats = GetComponent<CharacterStats>();

            // Configure NavMeshAgent for smooth movement
            agent.speed = baseMoveSpeed;
            agent.acceleration = 100f; // High acceleration, we handle smoothing ourselves
            agent.angularSpeed = 0f; // We handle rotation ourselves
            agent.stoppingDistance = stoppingDistance;
            agent.autoBraking = true;
            agent.updateRotation = false;
        }

        private void Start()
        {
            // Create move indicator
            if (moveIndicatorPrefab != null)
            {
                moveIndicator = Instantiate(moveIndicatorPrefab);
                moveIndicator.SetActive(false);
            }
        }

        private void Update()
        {
            UpdateSpeed();
            UpdateMovement();
            UpdateRotation();
            UpdateMoveIndicator();
        }

        private void UpdateSpeed()
        {
            // Get speed modifier from stats if available
            float speedMultiplier = stats != null ? stats.MoveSpeedMultiplier : 1f;
            float targetSpeed = baseMoveSpeed * speedMultiplier;

            // Update NavMeshAgent speed
            agent.speed = targetSpeed;
        }

        private void UpdateMovement()
        {
            // Handle direct movement (fallback when NavMesh fails)
            if (useDirectMovement)
            {
                Vector3 toTarget = directMoveTarget - transform.position;
                toTarget.y = 0;

                float distance = toTarget.magnitude;
                if (distance <= stoppingDistance)
                {
                    // Reached destination
                    useDirectMovement = false;
                    StopMoving();
                    return;
                }

                // Move directly toward target
                float speedMultiplier = stats != null ? stats.MoveSpeedMultiplier : 1f;
                float moveSpeed = baseMoveSpeed * speedMultiplier;
                Vector3 moveDirection = toTarget.normalized;

                transform.position += moveDirection * moveSpeed * Time.deltaTime;
                currentSpeed = moveSpeed;
                isMoving = true;
                currentVelocity = moveDirection * moveSpeed;
                return;
            }

            // Handle follow target
            if (followTarget != null)
            {
                agent.SetDestination(followTarget.position);
            }

            // Check if NavMesh path is stuck pending - fallback to direct movement
            if (targetPosition.HasValue && agent.pathPending)
            {
                pathPendingTime += Time.deltaTime;
                if (pathPendingTime > MaxPathPendingTime)
                {
                    Debug.Log($"NavMesh path pending too long ({pathPendingTime}s), switching to direct movement");
                    useDirectMovement = true;
                    directMoveTarget = new Vector3(targetPosition.Value.x, transform.position.y, targetPosition.Value.z);
                    pathPendingTime = 0f;
                    return;
                }
            }
            else
            {
                pathPendingTime = 0f;
            }

            // Check if we've reached destination
            if (agent.hasPath && !agent.pathPending)
            {
                if (agent.remainingDistance <= stoppingDistance)
                {
                    StopMoving();
                }
            }

            // Calculate current speed for animations
            currentSpeed = agent.velocity.magnitude;
            isMoving = currentSpeed > 0.1f;

            // Apply smooth velocity for visual smoothness
            currentVelocity = Vector3.SmoothDamp(currentVelocity, agent.velocity, ref smoothVelocity, positionSmoothTime);
        }

        private void UpdateRotation()
        {
            // Rotate towards movement direction
            Vector3 moveDirection = useDirectMovement ? currentVelocity : agent.velocity;
            moveDirection.y = 0;

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        private void UpdateMoveIndicator()
        {
            if (moveIndicator == null)
                return;

            if (targetPosition.HasValue && isMoving)
            {
                moveIndicator.SetActive(true);
                moveIndicator.transform.position = targetPosition.Value + Vector3.up * 0.1f;
            }
            else
            {
                moveIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// Move to a specific position
        /// </summary>
        public void MoveTo(Vector3 position)
        {
            Vector3 newTarget = new Vector3(position.x, transform.position.y, position.z);

            // If already moving to same destination, don't reset
            if (targetPosition.HasValue && Vector3.Distance(targetPosition.Value, newTarget) < 0.5f)
            {
                return;
            }

            followTarget = null;
            targetPosition = newTarget;
            pathPendingTime = 0f; // Reset timeout for new destination

            // If already using direct movement, update target
            if (useDirectMovement)
            {
                directMoveTarget = newTarget;
                return;
            }

            // Try NavMesh first
            if (agent.isOnNavMesh && NavMesh.SamplePosition(position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                if (agent.SetDestination(hit.position))
                {
                    Debug.Log($"NavMesh path set to {hit.position}");
                    return;
                }
            }

            // Fallback: use direct movement immediately
            Debug.Log($"Using direct movement to {position}");
            useDirectMovement = true;
            directMoveTarget = newTarget;
        }

        private bool useDirectMovement = false;
        private Vector3 directMoveTarget;
        private float pathPendingTime = 0f;
        private const float MaxPathPendingTime = 0.5f; // Fallback after 0.5 seconds

        /// <summary>
        /// Follow a target transform
        /// </summary>
        public void FollowTarget(Transform target)
        {
            followTarget = target;
            targetPosition = null;
        }

        /// <summary>
        /// Stop all movement
        /// </summary>
        public void StopMoving()
        {
            followTarget = null;
            targetPosition = null;
            useDirectMovement = false;
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
            }
            isMoving = false;
        }

        /// <summary>
        /// Check if a position is reachable
        /// </summary>
        public bool CanReach(Vector3 position)
        {
            NavMeshPath path = new NavMeshPath();
            return agent.CalculatePath(position, path) && path.status == NavMeshPathStatus.PathComplete;
        }

        /// <summary>
        /// Get distance to a position via navmesh
        /// </summary>
        public float GetPathDistance(Vector3 position)
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(position, path))
            {
                float distance = 0f;
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                }
                return distance;
            }
            return float.MaxValue;
        }

        /// <summary>
        /// Set base move speed
        /// </summary>
        public void SetBaseMoveSpeed(float speed)
        {
            baseMoveSpeed = speed;
        }

        /// <summary>
        /// Apply a temporary slow effect
        /// </summary>
        public void ApplySlow(float slowPercent, float duration)
        {
            if (stats != null)
            {
                stats.ApplyMoveSpeedModifier(1f - slowPercent, duration);
            }
        }

        private void OnDestroy()
        {
            if (moveIndicator != null)
            {
                Destroy(moveIndicator);
            }
        }
    }
}
