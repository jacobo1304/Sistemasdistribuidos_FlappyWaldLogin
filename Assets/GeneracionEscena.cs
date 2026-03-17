using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.CorgiEngine;
using JUEGOPROYECTO.Management;
using TMPro;

namespace JUEGOPROYECTO.Management
{
    public class GeneracionEscena : MonoBehaviour
    {
        private class ScoreMarker
        {
            public GameObject markerObject;
            public bool counted;
        }

        [Header("Player")]
        [Tooltip("The player character instance from the scene.")]
        public Character playerCharacter; // Added serialized field for the player

        [Header("Prefabs")]
        public GameObject platformPrefab;
        // public GameObject victoryPrefab; // Replaced by victoryObjectInScene

        [Header("Generation Settings")]
        // public int platformPairs = 5; // Removed platformPairs
        public float horizontalSpacing = 3f;
        public float minHeight = -1f;
        public float maxHeight = 3f;

        [Header("Platform Movement")]  
        [Tooltip("Speed at which platforms scroll left.")]
        public float platformSpeed = 2f;
        [Tooltip("X position at which platforms are destroyed.")]
        public float leftBound = -10f;
        [Tooltip("X position at which platforms are spawned.")]
        public float spawnX = 10f;

        [Header("Pair Settings")]
        [Tooltip("Vertical gap between the two platforms of a pair.")]
        public float holeHeight = 2f;

        [Header("Scene Transition")]
        public string NextSceneName = "Leaderboard";

        [Header("UI Score")]
        [Tooltip("TextMeshPro UI element to display current score.")]
        public TMP_Text scoreText;

        [Header("Start Settings")]
        [Tooltip("Static platform prefab under the player before level starts.")]
        public GameObject staticPlatformPrefab;
        [Tooltip("Position to spawn the static platform.")]
        public Vector2 staticPlatformPosition = Vector2.zero;
        [Tooltip("Y position to place the player's feet on the static platform.")]
        public float startCharacterY = 1.326323f;
        [Tooltip("UI text prompting to press '2' to start the level.")]
        public TMP_Text startPromptText;
        [Tooltip("Message to display in the start prompt.")]
        public string startPromptMessage = "Jump or Fall to Start!"; // Serialized start prompt message

        private GameObject _staticPlatformInstance;
        private bool _levelStarted = false;
        private bool _isRestarting = false; // Flag to manage restart process
        private bool _playerConfirmedOnStaticPlatform = false; // New flag
        private int _score = 0;
        private readonly List<ScoreMarker> _scoreMarkers = new List<ScoreMarker>();

        // Cached components for the player
        private Health _playerHealth;
        private CorgiController _playerController;
        private CharacterJetpack _jetpackAbility;
        private CharacterHorizontalMovement _horizontalMovementAbility; // Cached horizontal movement
        private CharacterJump _jumpAbility; // Cached jump ability

        void Start()
        {
            if (playerCharacter != null)
            {
                Debug.Log($"[GeneracionEscena] Start: Using PlayerCharacter: {playerCharacter.name} (Instance ID: {playerCharacter.GetInstanceID()})", this);
                _playerHealth = playerCharacter.GetComponent<Health>();
                _playerController = playerCharacter.GetComponent<CorgiController>();
                _jetpackAbility = playerCharacter.FindAbility<CharacterJetpack>();
                _horizontalMovementAbility = playerCharacter.FindAbility<CharacterHorizontalMovement>(); // Cache it
                _jumpAbility = playerCharacter.FindAbility<CharacterJump>(); // Cache jump ability

                if (_playerHealth == null)
                    Debug.LogError("Player Health component not found on playerCharacter. Player death and revival might not work correctly.", this);
                if (_playerController == null)
                    Debug.LogError("Player CorgiController component not found on playerCharacter. Ground detection will not work.", this);
                if (_horizontalMovementAbility == null)
                    Debug.LogWarning("Player CharacterHorizontalMovement component not found on playerCharacter. Horizontal movement cannot be controlled on victory.", this);
                if (_jumpAbility == null)
                    Debug.LogWarning("Player CharacterJump component not found on playerCharacter. Unlimited jumps cannot be set.", this);
                // _jetpackAbility can be null if not intended for the character.
            }
            else
            {
                Debug.LogError("[GeneracionEscena] PlayerCharacter is not assigned in the Inspector! Disabling script.", this);
                this.enabled = false; 
                return;
            }

            SetupInitialState();
        }

        void SetupInitialState()
        {
            _isRestarting = true; // Indicate that setup/restart is in progress
            _playerConfirmedOnStaticPlatform = false; // Reset this flag
            Debug.Log("[GeneracionEscena] SetupInitialState: Starting setup.", this);

            _levelStarted = false;
            _score = 0;
            _scoreMarkers.Clear();
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetRunScore();
            }

            StopAllCoroutines(); // Stop any ongoing level logic (spawning, timers)
            ClearDynamicElements(); // Remove platforms, victory objects etc.

            // Recreate or ensure static platform exists
            if (_staticPlatformInstance != null) Destroy(_staticPlatformInstance);
            if (staticPlatformPrefab != null)
            {
                _staticPlatformInstance = Instantiate(staticPlatformPrefab, staticPlatformPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("StaticPlatformPrefab is not assigned. Cannot create starting platform.", this);
            }

            // Reset Player
            if (playerCharacter != null) 
            {
                // Position and orientation
                Vector3 newPlayerPos = new Vector3(staticPlatformPosition.x, startCharacterY, playerCharacter.transform.position.z);
                Debug.Log($"[GeneracionEscena] SetupInitialState: Positioning player {playerCharacter.name} to {newPlayerPos}", this);
                playerCharacter.transform.position = newPlayerPos; 
                playerCharacter.transform.rotation = Quaternion.identity;

                if (_playerController != null) _playerController.SetForce(Vector2.zero); // Reset any physics forces

                // Health, state, and input (rely on Revive to handle most of this)
                if (_playerHealth != null)
                {
                    _playerHealth.Revive(); // Resets health, sets condition to Normal, enables input
                }
                else // If no Health component, manually try to set states
                {
                    playerCharacter.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
                    // Corgi Engine's Character.EnableInput() is not a public method.
                    // Input is typically managed by the Health component or CharacterStates.
                }
                // Ensure movement state is idle
                playerCharacter.MovementState.ChangeState(CharacterStates.MovementStates.Idle);

                // Jetpack initially not allowed
                if (_jetpackAbility != null) _jetpackAbility.PermitAbility(false);
                // Horizontal movement initially not allowed (or reset to default state if applicable)
                if (_horizontalMovementAbility != null) _horizontalMovementAbility.PermitAbility(false); 

                // Set unlimited jumps
                if (_jumpAbility != null)
                {
                    _jumpAbility.NumberOfJumps = int.MaxValue; // Effectively unlimited jumps
                    Debug.Log($"[GeneracionEscena] SetupInitialState: Set NumberOfJumps to int.MaxValue for {playerCharacter.name}", this);
                    _jumpAbility.PermitAbility(false); // Initially disable jumping
                    Debug.Log($"[GeneracionEscena] SetupInitialState: Jump ability disabled for {playerCharacter.name}. Will re-enable on static platform.", this);
                }
            }

            // Reset UI
            if (startPromptText != null)
            {
                startPromptText.gameObject.SetActive(true);
                startPromptText.text = startPromptMessage; // Use serialized message
            }
            if (scoreText != null)
            {
                scoreText.gameObject.SetActive(false);
                UpdateScoreUI();
            }

            _isRestarting = false; // Setup/restart complete
            Debug.Log("[GeneracionEscena] SetupInitialState: Setup complete.", this);
        }

        void Update()
        {
            if (_isRestarting || playerCharacter == null || _playerController == null) return;

            // Handle Escape key to exit before level starts
            if (!_levelStarted && _staticPlatformInstance != null && _staticPlatformInstance.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Backspace)) // Changed from KeyCode.Escape to KeyCode.Backspace
                {
                    Debug.Log("[GeneracionEscena] Update: Backspace key pressed before level start. Loading Nivel3.", this);
                    SceneManager.LoadScene("Nivel3"); 
                    return; 
                }

                bool isPlayerCurrentlyGrounded = _playerController.State.IsGrounded;
                GameObject standingOnObject = _playerController.StandingOn;

                if (isPlayerCurrentlyGrounded && standingOnObject != null && standingOnObject == _staticPlatformInstance)
                {
                    _playerConfirmedOnStaticPlatform = true; // Player is verified on the static platform
                    // Debug.Log("Player confirmed on static platform.", this);

                    if (_jumpAbility != null && !_jumpAbility.AbilityPermitted)
                    {
                        _jumpAbility.PermitAbility(true);
                        Debug.Log("[GeneracionEscena] Update: Player on static platform. Re-enabled jump.", this);
                    }
                }

                // If player was confirmed on static platform and is now NOT grounded, start level
                if (_playerConfirmedOnStaticPlatform && !isPlayerCurrentlyGrounded)
                {
                    // Debug.Log("Player was on static platform and is now ungrounded. Starting level.", this);
                    StartLevel();
                }
            }

            // If level is active, update score based on passed gap markers
            if (_levelStarted)
            {
                UpdateScoreProgress();
            }

            // Check for player death if Health component is available
            if (_playerHealth != null && _playerHealth.CurrentHealth <= 0)
            {
                HandlePlayerDeath();
            }
        }

        private void StartLevel()
        {
            if (_levelStarted || _isRestarting) return; // Prevent multiple starts or start during restart

            _levelStarted = true;
            _playerConfirmedOnStaticPlatform = false; // Reset flag as level is starting

            // Remove static platform and prompt
            if (_staticPlatformInstance != null)
            {
                Destroy(_staticPlatformInstance);
                _staticPlatformInstance = null; // Clear reference
            }
            if (startPromptText != null) startPromptText.gameObject.SetActive(false);

            // Setup and show score
            if (scoreText != null)
            {
                scoreText.gameObject.SetActive(true);
                UpdateScoreUI();
            }

            // Permit jetpack usage
            if (_jetpackAbility != null) _jetpackAbility.PermitAbility(true);

            // Start level mechanics
            StartCoroutine(SpawnLoop());
        }

        private void HandlePlayerDeath()
        {
            if (_isRestarting) return; // Already handling a restart/death

            _isRestarting = true;
            _levelStarted = false;
            StopAllCoroutines();

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.SubmitCurrentScore();
            }

            GameSessionData.LastScore = _score;
            Debug.Log($"[GeneracionEscena] Player died. Final score: {_score}. Loading scene '{NextSceneName}'.", this);
            SceneManager.LoadScene(NextSceneName);
        }

        private void ClearDynamicElements()
        {
            // Destroy all scrolling platforms (that are not the static one, though static shouldn't have the script)
            ScrollingPlatform[] activePlatforms = FindObjectsByType<ScrollingPlatform>(FindObjectsSortMode.None);
            foreach (ScrollingPlatform platform in activePlatforms)
            {
                if (platform.gameObject != _staticPlatformInstance) // Check just in case
                {
                    Destroy(platform.gameObject);
                }
            }
        }

        /// <summary>Spawns platform pairs at intervals.</summary>
        private IEnumerator SpawnLoop()
        {
            Debug.Log("[GeneracionEscena] SpawnLoop started. Will spawn platforms indefinitely until level ends.", this);

            while (_levelStarted) // Loop indefinitely as long as the level is started
            {
                // Debug.Log($"[GeneracionEscena] SpawnLoop: Attempting to spawn a pair", this);

                if (platformPrefab == null)
                {
                    Debug.LogError("[GeneracionEscena] platformPrefab is null. Cannot spawn platforms. Stopping SpawnLoop.", this);
                    yield break; // Stop the coroutine
                }

                SpawnPair();
                // Debug.Log($"[GeneracionEscena] SpawnLoop: Pair spawned.", this);

                if (platformSpeed <= 0f)
                {
                    Debug.LogError($"[GeneracionEscena] platformSpeed ({platformSpeed}) is zero or negative. SpawnLoop cannot continue effectively. Stopping coroutine.", this);
                    yield break; // Stop the coroutine
                }
                if (horizontalSpacing <= 0f)
                {
                    Debug.LogWarning($"[GeneracionEscena] horizontalSpacing ({horizontalSpacing}) is zero or negative. Platforms will spawn very rapidly.", this);
                }

                float interval = horizontalSpacing / platformSpeed;
                // Debug.Log($"[GeneracionEscena] SpawnLoop: Next pair will be spawned in {interval} seconds (Spacing: {horizontalSpacing}, Speed: {platformSpeed}).", this);
                yield return new WaitForSeconds(interval);
            }
            Debug.Log("[GeneracionEscena] SpawnLoop finished.", this);
        }

        /// <summary>Instantiates a pair of platforms with a vertical gap.</summary>
        private void SpawnPair()
        {
            float centerY = Random.Range(minHeight + holeHeight/2f, maxHeight - holeHeight/2f);
            float topY = centerY + holeHeight/2f;
            float bottomY = centerY - holeHeight/2f;

            // Top platform
            Vector3 topPos = new Vector3(spawnX, topY, 0f);
            GameObject top = Instantiate(platformPrefab, topPos, Quaternion.identity);
            var topScroller = top.AddComponent<JUEGOPROYECTO.Management.ScrollingPlatform>();
            topScroller.speed = platformSpeed;
            topScroller.leftBound = leftBound;
            top.AddComponent<JUEGOPROYECTO.Management.PlatformCollisionReset>();

            // Bottom platform
            Vector3 bottomPos = new Vector3(spawnX, bottomY, 0f);
            GameObject bottom = Instantiate(platformPrefab, bottomPos, Quaternion.identity);
            var bottomScroller = bottom.AddComponent<JUEGOPROYECTO.Management.ScrollingPlatform>();
            bottomScroller.speed = platformSpeed;
            bottomScroller.leftBound = leftBound;
            bottom.AddComponent<JUEGOPROYECTO.Management.PlatformCollisionReset>();

            // Score marker for this gap (1 point per gap passed)
            GameObject marker = new GameObject("ScoreMarker");
            marker.transform.position = new Vector3(spawnX, centerY, 0f);
            var markerScroller = marker.AddComponent<JUEGOPROYECTO.Management.ScrollingPlatform>();
            markerScroller.speed = platformSpeed;
            markerScroller.leftBound = leftBound;

            _scoreMarkers.Add(new ScoreMarker
            {
                markerObject = marker,
                counted = false
            });
        }

        private void UpdateScoreProgress()
        {
            float playerX = playerCharacter.transform.position.x;

            for (int i = _scoreMarkers.Count - 1; i >= 0; i--)
            {
                ScoreMarker marker = _scoreMarkers[i];

                if (marker.markerObject == null)
                {
                    _scoreMarkers.RemoveAt(i);
                    continue;
                }

                if (!marker.counted && playerX > marker.markerObject.transform.position.x)
                {
                    marker.counted = true;
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddScore(1);
                        _score = ScoreManager.Instance.CurrentScore;
                    }
                    else
                    {
                        _score++;
                    }
                    UpdateScoreUI();
                    Destroy(marker.markerObject);
                    _scoreMarkers.RemoveAt(i);
                }
            }
        }

        private void UpdateScoreUI()
        {
            if (scoreText != null)
            {
                scoreText.text = _score.ToString();
            }
        }
    }
}
