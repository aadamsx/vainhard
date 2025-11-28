using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using VaingloryMoba.Combat;
using VaingloryMoba.Characters;

namespace VaingloryMoba.UI
{
    /// <summary>
    /// UI button for activating abilities.
    /// Handles tap, hold, and drag for different targeting modes.
    /// </summary>
    public class AbilityButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private Text cooldownText;
        [SerializeField] private Image borderImage;

        [Header("Colors")]
        [SerializeField] private Color readyColor = Color.white;
        [SerializeField] private Color onCooldownColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color noEnergyColor = new Color(0.2f, 0.2f, 0.5f);
        [SerializeField] private Color activeColor = new Color(1f, 0.8f, 0f);

        // State
        private AbilityBase ability;
        private int abilityIndex;
        private bool isPressed;
        private bool isDragging;
        private Vector2 pressStartPosition;
        private float pressStartTime;

        private const float DRAG_THRESHOLD = 20f;
        private const float HOLD_THRESHOLD = 0.2f;

        public void Setup(AbilityBase ability, int index)
        {
            this.ability = ability;
            this.abilityIndex = index;

            if (iconImage != null && ability.Icon != null)
            {
                iconImage.sprite = ability.Icon;
            }

            UpdateState();
        }

        public void UpdateState()
        {
            if (ability == null) return;

            bool isReady = ability.IsReady;
            bool canAfford = ability.CanAfford;

            // Update cooldown overlay
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = ability.CooldownPercent;
                cooldownOverlay.gameObject.SetActive(!isReady);
            }

            // Update cooldown text
            if (cooldownText != null)
            {
                if (!isReady)
                {
                    float cd = ability.CurrentCooldown;
                    cooldownText.text = cd >= 1f ? Mathf.CeilToInt(cd).ToString() : cd.ToString("F1");
                    cooldownText.gameObject.SetActive(true);
                }
                else
                {
                    cooldownText.gameObject.SetActive(false);
                }
            }

            // Update icon color
            if (iconImage != null)
            {
                if (!isReady)
                {
                    iconImage.color = onCooldownColor;
                }
                else if (!canAfford)
                {
                    iconImage.color = noEnergyColor;
                }
                else
                {
                    iconImage.color = readyColor;
                }
            }

            // Update border for active state
            if (borderImage != null && ability is TwirlingSilver twirling)
            {
                borderImage.color = twirling.IsActive ? activeColor : readyColor;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (ability == null || !ability.IsReady || !ability.CanAfford)
                return;

            isPressed = true;
            isDragging = false;
            pressStartPosition = eventData.position;
            pressStartTime = Time.time;

            // Instant cast abilities activate immediately
            if (ability.Targeting == AbilityBase.TargetingType.Instant)
            {
                ActivateAbility(null, null);
                isPressed = false;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;

            float pressDuration = Time.time - pressStartTime;
            float dragDistance = Vector2.Distance(pressStartPosition, eventData.position);

            if (isDragging)
            {
                // Dragged and released - confirm target at release position
                Vector3? worldPos = GetWorldPositionFromScreen(eventData.position);
                if (worldPos.HasValue)
                {
                    ActivateAbility(worldPos, null);
                }
                GameHUD.Instance?.CancelAbilityTargeting();
            }
            else if (dragDistance < DRAG_THRESHOLD)
            {
                // Tap - start targeting mode for non-instant abilities
                if (ability.Targeting != AbilityBase.TargetingType.Instant)
                {
                    GameHUD.Instance?.StartAbilityTargeting(abilityIndex, ability);
                }
            }

            isPressed = false;
            isDragging = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isPressed) return;

            float dragDistance = Vector2.Distance(pressStartPosition, eventData.position);

            if (dragDistance > DRAG_THRESHOLD)
            {
                if (!isDragging)
                {
                    isDragging = true;
                    GameHUD.Instance?.StartAbilityTargeting(abilityIndex, ability);
                }

                // Update targeting indicator
                // The HUD handles this based on touch position
            }
        }

        private void ActivateAbility(Vector3? position, GameObject target)
        {
            if (ability == null) return;

            var hero = FindObjectOfType<HeroController>();
            if (hero != null && hero.IsPlayerControlled)
            {
                hero.UseAbility(abilityIndex, position, target);
            }
        }

        private Vector3? GetWorldPositionFromScreen(Vector2 screenPos)
        {
            if (Camera.main == null) return null;

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return null;
        }
    }
}
