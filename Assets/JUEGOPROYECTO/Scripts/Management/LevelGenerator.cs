using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.CorgiEngine;

namespace JUEGOPROYECTO.Management
{
    /// <summary>
    /// Generates a level: infinite jetpack for player, moving platform pairs, restart on platform hit,
    /// and spawns a VictoryZone object after a timer.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Platform prefab (must have Collider2D, Rigidbody2D as kinematic)")]
        public GameObject platformPrefab;
        [Tooltip("Victory zone prefab (must have Collider2D set as Trigger)")]
        public GameObject victoryZonePrefab;
        [Tooltip("Player character instance")] 
        public Character playerCharacter;

        [Header("Level Settings")]
        public int platformPairs = 5;
        public float horizontalSpacing = 3f;
        public float minHeight = -1f;
        public float maxHeight = 3f;
        public float levelDuration = 20f;
        public string nextSceneName = "nivel3";

        private void Start()
        {
            Debug.Log("[LevelGenerator] Level generation started.");
            if (playerCharacter != null)
            {
                Debug.Log("[LevelGenerator] Enabling jetpack ability for player.");
                var jetpack = playerCharacter.FindAbility<CharacterJetpack>();
                if (jetpack != null)
                {
                    jetpack.PermitAbility(true);
                    Debug.Log("[LevelGenerator] Jetpack ability enabled.");
                }
            }
            // Generate platform pairs
            for (int i = 0; i < platformPairs; i++)
            {
                Debug.Log($"[LevelGenerator] Generating platform pair {i+1}/{platformPairs}.");
                float y = Random.Range(minHeight, maxHeight);
                Vector3 basePos = new Vector3(i * horizontalSpacing, y, 0f);

                // Left platform
                var left = Instantiate(platformPrefab, basePos, Quaternion.identity);
                Debug.Log($"[LevelGenerator] Left platform at {basePos} created.");
                left.AddComponent<PlatformMover>();
                left.AddComponent<PlatformCollisionReset>();

                // Right platform, leaving a gap for the player
                float gap = platformPrefab.transform.localScale.x + 1f;
                var rightPos = basePos + new Vector3(gap, 0f, 0f);
                Debug.Log($"[LevelGenerator] Right platform at {rightPos} created.");
                var right = Instantiate(platformPrefab, rightPos, Quaternion.identity);
                right.AddComponent<PlatformMover>();
                right.AddComponent<PlatformCollisionReset>();
            }
            // Start timer for victory
            StartCoroutine(VictoryTimer());
        }

        private IEnumerator VictoryTimer()
        {
            Debug.Log($"[LevelGenerator] Starting level timer for {levelDuration} seconds.");
            yield return new WaitForSeconds(levelDuration);
            Debug.Log("[LevelGenerator] Level duration reached, spawning victory zone.");
            // spawn VictoryZone
            Vector3 winPos = new Vector3(platformPairs * horizontalSpacing + 2f, 0f, 0f);
            var vz = Instantiate(victoryZonePrefab, winPos, Quaternion.identity);
            Debug.Log($"[LevelGenerator] Victory zone spawned at {winPos}.");
            var zone = vz.AddComponent<VictoryZone>();
            zone.NextSceneName = nextSceneName;
        }
    }
}
