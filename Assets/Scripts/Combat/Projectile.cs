using UnityEngine;
using VaingloryMoba.Characters;
using VaingloryMoba.Core;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Handles projectile movement and collision for abilities and attacks.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float maxLifetime = 5f;
        [SerializeField] private bool destroyOnHit = true;
        [SerializeField] private bool canBeBlocked = false; // For Hellfire Brew

        [Header("Visual")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private TrailRenderer trail;

        // State
        private Transform target;
        private Vector3 direction;
        private float damage;
        private CharacterStats.DamageType damageType;
        private GameObject owner;
        private GameManager.Team ownerTeam;
        private System.Action<GameObject> onHitCallback;
        private float spawnTime;
        private bool isHoming;
        private bool heroesOnly;

        public void Initialize(Vector3 direction, float speed, float damage,
            CharacterStats.DamageType damageType, GameObject owner,
            System.Action<GameObject> onHit = null, bool heroesOnly = false)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.damage = damage;
            this.damageType = damageType;
            this.owner = owner;
            this.onHitCallback = onHit;
            this.isHoming = false;
            this.heroesOnly = heroesOnly;

            var heroController = owner.GetComponent<HeroController>();
            if (heroController != null)
            {
                ownerTeam = heroController.Team;
            }

            spawnTime = Time.time;

            // Face direction
            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        public void Initialize(Transform target, float speed, System.Action<GameObject> onHit = null)
        {
            this.target = target;
            this.speed = speed;
            this.onHitCallback = onHit;
            this.isHoming = true;
            spawnTime = Time.time;
        }

        public void InitializeHoming(Transform target, float speed, float damage,
            CharacterStats.DamageType damageType, GameObject owner, bool canBeBlocked,
            System.Action<GameObject> onHit = null)
        {
            this.target = target;
            this.speed = speed;
            this.damage = damage;
            this.damageType = damageType;
            this.owner = owner;
            this.canBeBlocked = canBeBlocked;
            this.onHitCallback = onHit;
            this.isHoming = true;

            var heroController = owner.GetComponent<HeroController>();
            if (heroController != null)
            {
                ownerTeam = heroController.Team;
            }

            spawnTime = Time.time;
        }

        private void Update()
        {
            // Check lifetime
            if (Time.time > spawnTime + maxLifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (isHoming)
            {
                UpdateHoming();
            }
            else
            {
                UpdateLinear();
            }
        }

        private void UpdateLinear()
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        private void UpdateHoming()
        {
            if (target == null)
            {
                // Target lost, continue in last direction
                transform.position += transform.forward * speed * Time.deltaTime;
                return;
            }

            Vector3 targetPos = target.position + Vector3.up; // Aim at center mass
            Vector3 toTarget = targetPos - transform.position;

            // Check if reached target
            if (toTarget.magnitude < 0.5f)
            {
                OnHit(target.gameObject);
                return;
            }

            // Move towards target
            direction = toTarget.normalized;
            transform.position += direction * speed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Ignore owner
            if (other.gameObject == owner) return;
            if (other.transform.IsChildOf(owner.transform)) return;

            // Check if it's a valid target
            var targetStats = other.GetComponent<CharacterStats>();
            if (targetStats == null)
            {
                targetStats = other.GetComponentInParent<CharacterStats>();
            }

            if (targetStats == null) return;

            // Check team
            var targetHero = other.GetComponent<HeroController>();
            if (targetHero == null)
            {
                targetHero = other.GetComponentInParent<HeroController>();
            }

            if (targetHero != null && targetHero.Team == ownerTeam) return;

            // If heroesOnly, skip non-heroes
            if (heroesOnly && targetHero == null) return;

            // Check if can be blocked (for Hellfire Brew)
            if (canBeBlocked && isHoming && target != null)
            {
                // If we hit something other than the target, it was blocked
                if (other.gameObject != target.gameObject &&
                    !other.transform.IsChildOf(target))
                {
                    OnHit(other.gameObject);
                    return;
                }
            }

            OnHit(other.gameObject);
        }

        private void OnHit(GameObject hitObject)
        {
            // Apply damage
            var targetStats = hitObject.GetComponent<CharacterStats>();
            if (targetStats == null)
            {
                targetStats = hitObject.GetComponentInParent<CharacterStats>();
            }

            if (targetStats != null && damage > 0)
            {
                targetStats.TakeDamage(damage, damageType, owner);
            }

            // Callback
            onHitCallback?.Invoke(hitObject);

            // Spawn hit effect
            if (hitEffectPrefab != null)
            {
                var effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // Camera shake
            if (GameCamera.Instance != null)
            {
                GameCamera.Instance.Shake(0.1f);
            }

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}
