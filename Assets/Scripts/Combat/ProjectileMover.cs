using UnityEngine;

namespace VaingloryMoba.Combat
{
    public class ProjectileMover : MonoBehaviour
    {
        private Vector3 targetPosition;
        private float speed;
        private bool initialized;

        public void Initialize(Vector3 target, float moveSpeed)
        {
            targetPosition = target;
            speed = moveSpeed;
            initialized = true;
        }

        private void Update()
        {
            if (!initialized) return;

            // Move toward target
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // Check if reached target
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                Destroy(gameObject);
            }

            // Safety: destroy after 3 seconds if somehow stuck
            Destroy(gameObject, 3f);
        }
    }
}
