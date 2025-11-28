using UnityEngine;
using UnityEngine.AI;
using VaingloryMoba.Core;
using VaingloryMoba.Combat;

namespace VaingloryMoba.Characters
{
    /// <summary>
    /// AI controller for enemy heroes.
    /// Handles basic decision-making: farming, pushing, fighting.
    /// </summary>
    [RequireComponent(typeof(HeroController))]
    public class AIController : MonoBehaviour
    {
        [Header("Behavior Settings")]
        [SerializeField] private float decisionInterval = 0.5f;
        [SerializeField] private float aggroRange = 10f;
        [SerializeField] private float retreatHealthPercent = 0.25f;
        [SerializeField] private float engageHealthPercent = 0.6f;

        [Header("Ability Usage")]
        [SerializeField] private float abilityUseCooldown = 2f;
        [SerializeField] private float skillshotLeadFactor = 0.3f;

        // Components
        private HeroController heroController;
        private CharacterStats stats;
        private CharacterMotor motor;
        private NavMeshAgent agent;

        // State
        private float lastDecisionTime;
        private float lastAbilityTime;
        private AIState currentState = AIState.Idle;
        private GameObject currentTarget;
        private Vector3? moveTarget;

        private enum AIState
        {
            Idle,
            Farming,
            Pushing,
            Fighting,
            Retreating,
            Returning
        }

        private void Awake()
        {
            heroController = GetComponent<HeroController>();
            stats = GetComponent<CharacterStats>();
            motor = GetComponent<CharacterMotor>();
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            // Subscribe to death event
            if (stats != null)
            {
                stats.OnDeath.AddListener(OnDeath);
            }

            Debug.Log($"AIController started on {gameObject.name}, Team: {heroController.Team}");
        }

        private void Update()
        {
            if (!stats.IsAlive) return;

            // Make decisions periodically
            if (Time.time >= lastDecisionTime + decisionInterval)
            {
                MakeDecision();
                lastDecisionTime = Time.time;
            }

            // Execute current behavior
            ExecuteBehavior();
        }

        private void MakeDecision()
        {
            var prevState = currentState;

            // Check if should retreat
            if (stats.HealthPercent < retreatHealthPercent)
            {
                currentState = AIState.Retreating;
                currentTarget = null;
                if (prevState != currentState)
                    Debug.Log($"AI {gameObject.name}: State changed to {currentState}");
                return;
            }

            // Look for enemy heroes
            var enemyHero = FindEnemyHero();
            if (enemyHero != null)
            {
                float distance = Vector3.Distance(transform.position, enemyHero.transform.position);
                var enemyStats = enemyHero.GetComponent<CharacterStats>();

                // Engage if healthy and enemy is weak or close
                bool shouldEngage = stats.HealthPercent >= engageHealthPercent &&
                                   (enemyStats.HealthPercent < stats.HealthPercent || distance < aggroRange * 0.5f);

                if (distance < aggroRange && shouldEngage)
                {
                    currentState = AIState.Fighting;
                    currentTarget = enemyHero;
                    return;
                }
            }

            // Look for minions to farm
            var nearestMinion = FindNearestEnemy<Map.Minion>();
            if (nearestMinion != null)
            {
                float distance = Vector3.Distance(transform.position, nearestMinion.transform.position);
                if (distance < aggroRange)
                {
                    if (prevState != AIState.Farming)
                        Debug.Log($"AI {gameObject.name}: Found minion {nearestMinion.name} at distance {distance:F1}, switching to Farming");
                    currentState = AIState.Farming;
                    currentTarget = nearestMinion.gameObject;
                    return;
                }
            }

            // Default: push lane
            currentState = AIState.Pushing;
            currentTarget = null;

            if (prevState != currentState)
                Debug.Log($"AI {gameObject.name}: State changed to {currentState}");
        }

        private void ExecuteBehavior()
        {
            switch (currentState)
            {
                case AIState.Idle:
                    // Do nothing
                    break;

                case AIState.Farming:
                    ExecuteFarming();
                    break;

                case AIState.Pushing:
                    ExecutePushing();
                    break;

                case AIState.Fighting:
                    ExecuteFighting();
                    break;

                case AIState.Retreating:
                    ExecuteRetreating();
                    break;

                case AIState.Returning:
                    ExecuteReturning();
                    break;
            }
        }

        private void ExecuteFarming()
        {
            if (currentTarget == null)
            {
                currentState = AIState.Pushing;
                return;
            }

            var targetStats = currentTarget.GetComponent<CharacterStats>();
            if (targetStats == null || !targetStats.IsAlive)
            {
                currentTarget = null;
                return;
            }

            // Attack the minion
            heroController.SetAttackTarget(currentTarget);

            // Use abilities on low health minions for last hit
            if (targetStats.HealthPercent < 0.3f)
            {
                TryUseAbility(currentTarget);
            }
        }

        private void ExecutePushing()
        {
            // Move towards enemy base
            Vector3 pushTarget = heroController.Team == GameManager.Team.Blue ?
                new Vector3(40, 0, 50) : new Vector3(40, 0, 10);

            float distanceToTarget = Vector3.Distance(transform.position, pushTarget);

            if (distanceToTarget > 5f)
            {
                motor.MoveTo(pushTarget);
            }

            // Look for targets along the way
            var target = FindNearestEnemy<CharacterStats>();
            if (target != null)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < stats.AttackRange)
                {
                    heroController.SetAttackTarget(target.gameObject);
                }
            }
        }

        private void ExecuteFighting()
        {
            if (currentTarget == null)
            {
                currentState = AIState.Pushing;
                return;
            }

            var targetStats = currentTarget.GetComponent<CharacterStats>();
            if (targetStats == null || !targetStats.IsAlive)
            {
                currentTarget = null;
                currentState = AIState.Pushing;
                return;
            }

            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

            // Use abilities
            TryUseAbility(currentTarget);

            // Attack
            heroController.SetAttackTarget(currentTarget);

            // Kite if target is melee and close
            if (distance < 3f)
            {
                Vector3 awayDirection = (transform.position - currentTarget.transform.position).normalized;
                Vector3 kitePosition = transform.position + awayDirection * 2f;
                motor.MoveTo(kitePosition);
            }
        }

        private void ExecuteRetreating()
        {
            heroController.ClearTarget();

            // Move to base
            Vector3 basePosition = heroController.Team == GameManager.Team.Blue ?
                new Vector3(40, 0, 5) : new Vector3(40, 0, 55);

            motor.MoveTo(basePosition);

            float distanceToBase = Vector3.Distance(transform.position, basePosition);
            if (distanceToBase < 8f)
            {
                // In base - heal up
                if (stats.HealthPercent >= 0.9f)
                {
                    currentState = AIState.Pushing;
                }
            }
        }

        private void ExecuteReturning()
        {
            // Similar to retreating but without combat urgency
            Vector3 basePosition = heroController.Team == GameManager.Team.Blue ?
                new Vector3(40, 0, 5) : new Vector3(40, 0, 55);

            motor.MoveTo(basePosition);
        }

        private void TryUseAbility(GameObject target)
        {
            if (Time.time < lastAbilityTime + abilityUseCooldown) return;

            // Try each ability
            for (int i = 0; i < 4; i++)
            {
                var ability = heroController.GetAbility(i);
                if (ability == null || !ability.IsReady || !ability.CanAfford) continue;

                Vector3? targetPos = null;
                GameObject targetUnit = null;

                switch (ability.Targeting)
                {
                    case AbilityBase.TargetingType.Instant:
                        // Use self-buff if in combat
                        Debug.Log($"[AI] {gameObject.name} attempting to use INSTANT ability '{ability.AbilityName}'");
                        if (heroController.UseAbility(i, null, null))
                        {
                            Debug.Log($"[AI] {gameObject.name} successfully used '{ability.AbilityName}'");
                            lastAbilityTime = Time.time;
                            return;
                        }
                        break;

                    case AbilityBase.TargetingType.Skillshot:
                        // Lead the target
                        var targetMotor = target.GetComponent<CharacterMotor>();
                        if (targetMotor != null && targetMotor.CurrentSpeed > 0.5f)
                        {
                            // Predict position
                            float flightTime = Vector3.Distance(transform.position, target.transform.position) / 20f;
                            targetPos = target.transform.position + (target.transform.forward * targetMotor.CurrentSpeed * flightTime * skillshotLeadFactor);
                        }
                        else
                        {
                            targetPos = target.transform.position;
                        }

                        if (heroController.UseAbility(i, targetPos, null))
                        {
                            lastAbilityTime = Time.time;
                            return;
                        }
                        break;

                    case AbilityBase.TargetingType.UnitTarget:
                        targetUnit = target;
                        if (heroController.UseAbility(i, null, targetUnit))
                        {
                            lastAbilityTime = Time.time;
                            return;
                        }
                        break;

                    case AbilityBase.TargetingType.PointTarget:
                        targetPos = target.transform.position;
                        if (heroController.UseAbility(i, targetPos, null))
                        {
                            lastAbilityTime = Time.time;
                            return;
                        }
                        break;
                }
            }
        }

        private GameObject FindEnemyHero()
        {
            var heroes = FindObjectsOfType<HeroController>();
            GameObject closest = null;
            float closestDist = float.MaxValue;

            foreach (var hero in heroes)
            {
                if (hero.Team == heroController.Team) continue;
                if (!hero.Stats.IsAlive) continue;

                float dist = Vector3.Distance(transform.position, hero.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = hero.gameObject;
                }
            }

            return closest;
        }

        private T FindNearestEnemy<T>() where T : Component
        {
            var targets = FindObjectsOfType<T>();
            T closest = null;
            float closestDist = float.MaxValue;

            foreach (var target in targets)
            {
                // Check team
                var targetable = target.GetComponent<Targetable>();
                if (targetable != null && !targetable.CanBeTargetedBy(heroController.Team))
                {
                    continue;
                }

                var targetStats = target.GetComponent<CharacterStats>();
                if (targetStats != null && !targetStats.IsAlive)
                {
                    continue;
                }

                float dist = Vector3.Distance(transform.position, target.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = target;
                }
            }

            return closest;
        }

        private void OnDeath()
        {
            currentState = AIState.Idle;
            currentTarget = null;
        }
    }
}
