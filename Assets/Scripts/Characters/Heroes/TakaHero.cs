using UnityEngine;
using VaingloryMoba.Combat;

namespace VaingloryMoba.Characters
{
    /// <summary>
    /// Taka - Assassin hero with stealth and burst damage.
    /// Passive: House Kamuigahara - Landing abilities grants Ki stacks (move speed).
    /// </summary>
    public class TakaHero : MonoBehaviour
    {
        [Header("House Kamuigahara Passive")]
        [SerializeField] private int maxKiStacks = 5;
        [SerializeField] private float moveSpeedPerStack = 0.04f; // 4% per stack
        [SerializeField] private float kiDuration = 5f;

        [Header("Visual")]
        [SerializeField] private Color stealthColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        private int kiStacks = 0;
        private float kiExpireTime = 0f;
        private bool isStealthed = false;
        private float stealthEndTime = 0f;

        private CharacterStats ownerStats;
        private HeroController heroController;
        private Renderer[] renderers;
        private Color[] originalColors;

        public int KiStacks => kiStacks;
        public bool IsStealthed => isStealthed && Time.time < stealthEndTime;

        private void Awake()
        {
            ownerStats = GetComponent<CharacterStats>();
            heroController = GetComponent<HeroController>();
        }

        private void Start()
        {
            // Cache renderers for stealth visual
            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalColors[i] = renderers[i].material.color;
            }
        }

        private void Update()
        {
            // Check Ki expiration
            if (kiStacks > 0 && Time.time >= kiExpireTime)
            {
                ClearKi();
            }

            // Check stealth expiration
            if (isStealthed && Time.time >= stealthEndTime)
            {
                EndStealth();
            }
        }

        /// <summary>
        /// Called when Taka lands an ability - grants Ki stack
        /// </summary>
        public void OnAbilityHit()
        {
            AddKiStack();
        }

        private void AddKiStack()
        {
            if (kiStacks < maxKiStacks)
            {
                kiStacks++;
                UpdateMoveSpeedModifier();
            }

            kiExpireTime = Time.time + kiDuration;
            Debug.Log($"Ki stack gained! Stacks: {kiStacks}/{maxKiStacks}");
        }

        private void ClearKi()
        {
            kiStacks = 0;
            ownerStats.RemoveModifier("taka_ki");
            Debug.Log("Ki stacks expired");
        }

        private void UpdateMoveSpeedModifier()
        {
            float moveBonus = 1f + (kiStacks * moveSpeedPerStack);
            ownerStats.ApplyModifier("taka_ki", CharacterStats.StatType.MoveSpeed, moveBonus, kiDuration);
        }

        /// <summary>
        /// Enter stealth mode
        /// </summary>
        public void EnterStealth(float duration, float healPerSecond)
        {
            isStealthed = true;
            stealthEndTime = Time.time + duration;

            // Visual effect - make semi-transparent
            foreach (var r in renderers)
            {
                var color = r.material.color;
                r.material.color = new Color(color.r, color.g, color.b, 0.3f);
            }

            // Start healing coroutine
            StartCoroutine(StealthHeal(duration, healPerSecond));

            Debug.Log($"Taka entered stealth for {duration}s");
        }

        private System.Collections.IEnumerator StealthHeal(float duration, float healPerSecond)
        {
            float elapsed = 0f;
            while (elapsed < duration && isStealthed)
            {
                ownerStats.Heal(healPerSecond * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Break stealth (from attacking or taking damage)
        /// </summary>
        public void BreakStealth()
        {
            if (isStealthed)
            {
                EndStealth();
            }
        }

        private void EndStealth()
        {
            isStealthed = false;

            // Restore original colors
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].material.color = originalColors[i];
                }
            }

            Debug.Log("Taka stealth ended");
        }

        /// <summary>
        /// Modify damage - bonus from stealth attacks
        /// </summary>
        public float ModifyAttackDamage(float baseDamage)
        {
            if (IsStealthed)
            {
                BreakStealth();
                return baseDamage * 1.25f; // 25% bonus from stealth
            }
            return baseDamage;
        }

        /// <summary>
        /// Reset for respawn
        /// </summary>
        public void Reset()
        {
            kiStacks = 0;
            isStealthed = false;
            EndStealth();
        }
    }
}
