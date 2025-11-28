using UnityEngine;
using VaingloryMoba.Core;

namespace VaingloryMoba.Map
{
    /// <summary>
    /// Manages all jungle camps and their spawning.
    /// </summary>
    public class JungleManager : MonoBehaviour
    {
        [Header("Camp Positions")]
        [SerializeField] private float mapWidth = 100f;   // Match MapGenerator
        [SerializeField] private float laneZ = 40f;       // Match MapGenerator

        private void Start()
        {
            CreateJungleCamps();
        }

        private void CreateJungleCamps()
        {
            // Jungle is BELOW the lane (lower Z values)
            // Kraken at bottom center: X=50, Z=10
            float jungleZ = 25f;

            // Blue side camps (left side)
            CreateCamp(new Vector3(30f, 0, jungleZ), JungleCamp.CampType.Small, "Jungle_BlueSide");
            CreateCamp(new Vector3(38f, 0, jungleZ - 5f), JungleCamp.CampType.Large, "Jungle_BlueTreent",
                JungleCamp.BuffType.AttackSpeed);

            // Central contested objective (Gold Mine near Kraken)
            CreateCamp(new Vector3(50f, 0, 18f), JungleCamp.CampType.Medium, "GoldMine");

            // Red side camps (right side)
            CreateCamp(new Vector3(62f, 0, jungleZ - 5f), JungleCamp.CampType.Large, "Jungle_RedTreent",
                JungleCamp.BuffType.AttackSpeed);
            CreateCamp(new Vector3(70f, 0, jungleZ), JungleCamp.CampType.Small, "Jungle_RedSide");
        }

        private void CreateCamp(Vector3 position, JungleCamp.CampType type, string name,
            JungleCamp.BuffType buff = JungleCamp.BuffType.None)
        {
            var campObj = new GameObject(name);
            campObj.transform.SetParent(transform);
            campObj.transform.position = position;

            var camp = campObj.AddComponent<JungleCamp>();
            camp.Configure(type, buff);
        }
    }
}
