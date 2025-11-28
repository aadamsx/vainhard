using UnityEngine;
using VaingloryMoba.Core;
using VaingloryMoba.Characters;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Makes an object targetable by abilities and attacks.
    /// Used for minions, jungle monsters, turrets, etc.
    /// </summary>
    public class Targetable : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameManager.Team team = GameManager.Team.Neutral;
        [SerializeField] private bool canBeTargetedByEnemies = true;
        [SerializeField] private bool canBeTargetedByAllies = false;
        [SerializeField] private TargetType targetType = TargetType.Minion;

        [Header("UI")]
        [SerializeField] private Transform healthBarAnchor;
        [SerializeField] private float selectionRadius = 1f;

        public enum TargetType
        {
            Hero,
            Minion,
            JungleMonster,
            Turret,
            Crystal,
            Other
        }

        public GameManager.Team Team => team;
        public TargetType Type => targetType;
        public Transform HealthBarAnchor => healthBarAnchor ?? transform;
        public float SelectionRadius => selectionRadius;

        public void SetTeam(GameManager.Team newTeam)
        {
            team = newTeam;
        }

        /// <summary>
        /// Check if this object can be targeted by a specific team
        /// </summary>
        public bool CanBeTargetedBy(GameManager.Team attackerTeam)
        {
            if (attackerTeam == team)
            {
                return canBeTargetedByAllies;
            }
            else
            {
                return canBeTargetedByEnemies;
            }
        }

        /// <summary>
        /// Check if this is an enemy of the specified team
        /// </summary>
        public bool IsEnemyOf(GameManager.Team otherTeam)
        {
            return team != otherTeam && team != GameManager.Team.Neutral;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, selectionRadius);
        }
    }
}
