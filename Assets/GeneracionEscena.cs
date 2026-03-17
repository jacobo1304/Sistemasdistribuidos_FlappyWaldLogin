using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.CorgiEngine;
using JUEGOPROYECTO.Management;
using TMPro;

namespace JUEGOPROYECTO.Management
{
    public class GeneracionEscena : MonoBehaviour
    {
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
        public float levelDuration = 20f;

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
        public string NextSceneName = "Nivel3";

        [Header("UI Timer")]
        [Tooltip("TextMeshPro UI element to display remaining time.")]
        public TMP_Text timerText;

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

        [Header("Victory Settings")]
        [Tooltip("The pre-existing GameObject in the scene to activate upon victory.")]
        public GameObject victoryObjectInScene; // New field for the scene object
        [Tooltip("TextMeshPro UI element to display the victory message.")]
        public TMP_Text victoryTextUI; // UI for victory message
        [Tooltip("Message to display upon victory.")]
        public string victoryMessage = "¡Victoria!"; // Serialized victory message
        // [Tooltip("Y position offset from player's startCharacterY to spawn the victory prefab. e.g., -4f to spawn below.")]
        // public float victoryPrefabYOffset = -4f; // No longer needed

        private GameObject _staticPlatformInstance;
        private float _elapsedTime = 0f;
        private bool _levelStarted = false;
        private bool _isRestarting = false; // Flag to manage restart process
        private bool _playerConfirmedOnStaticPlatform = false; // New flag

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
            _elapsedTime = 0f;

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
            if (timerText != null)
            {
                timerText.gameObject.SetActive(false);
                UpdateTimerUI(); // Update to show initial time (e.g., "20") even if hidden
            }
            if (victoryTextUI != null) // Hide victory text
            {
                victoryTextUI.gameObject.SetActive(false);
            }

            // Deactivate victory object
            if (victoryObjectInScene != null)
            {
                victoryObjectInScene.SetActive(false);
                Debug.Log($"[GeneracionEscena] SetupInitialState: Deactivated victoryObjectInScene '{victoryObjectInScene.name}'.", this);
            }
            else
            {
                Debug.LogWarning("[GeneracionEscena] SetupInitialState: victoryObjectInScene is not assigned in the Inspector.", this);
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

            // If level is active, update the timer
            if (_levelStarted && _elapsedTime < levelDuration)
            {
                _elapsedTime += Time.deltaTime;
                UpdateTimerUI();
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

            // Setup and show timer
            if (timerText != null)
            {
                timerText.gameObject.SetActive(true);
                _elapsedTime = 0f; // Reset timer for the current run
                UpdateTimerUI();
            }

            // Permit jetpack usage
            if (_jetpackAbility != null) _jetpackAbility.PermitAbility(true);

            // Start level mechanics
            StartCoroutine(SpawnLoop());
            StartCoroutine(LevelTimer());
        }

        private void HandlePlayerDeath()
        {
            if (_isRestarting) return; // Already handling a restart/death
            // _isRestarting will be set to true at the beginning of SetupInitialState

            Debug.Log("Player died. Resetting level.", this);
            SetupInitialState(); // Reset everything to the initial state
        }

        private void ClearDynamicElements()
        {
            // Destroy all scrolling platforms (that are not the static one, though static shouldn't have the script)
            ScrollingPlatform[] activePlatforms = FindObjectsOfType<ScrollingPlatform>();
            foreach (ScrollingPlatform platform in activePlatforms)
            {
                if (platform.gameObject != _staticPlatformInstance) // Check just in case
                {
                    Destroy(platform.gameObject);
                }
            }

            // Destroy victory object if it exists
            // VictoryZone victoryZone = FindObjectOfType<VictoryZone>(); // Corrected from FindObjectsOfType
            // if (victoryZone != null) Destroy(victoryZone.gameObject); 
            // The victoryObjectInScene is managed by SetupInitialState (deactivated)
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
        }

        /// <summary>
        /// Waits specified time then spawns victory zone that triggers next scene
        /// </summary>
        IEnumerator LevelTimer()
        {
            float timer = 0f;
            Debug.Log($"[GeneracionEscena] LevelTimer: Coroutine started. Waiting for level duration: {levelDuration}s", this);
            while (timer < levelDuration)
            {
                if (!_levelStarted) // If level stopped for some other reason (e.g. death before timer end)
                {
                    Debug.Log("[GeneracionEscena] LevelTimer: Level stopped prematurely. Exiting LevelTimer.", this);
                    yield break;
                }
                timer += Time.deltaTime;
                yield return null;
            }

            if (!_levelStarted) // Double check in case it was stopped right as duration was met
            {
                Debug.Log("[GeneracionEscena] LevelTimer: Level stopped as duration was met. Exiting LevelTimer before spawning victory.", this);
                yield break;
            }

            if (victoryObjectInScene == null) // Check the new serialized object
            {
                Debug.LogWarning("[GeneracionEscena] LevelTimer: victoryObjectInScene is not assigned in the Inspector. Cannot activate victory object.", this);
                yield break;
            }
            
            // Player character null check is still relevant for other potential uses, but not for positioning the victory object if it's pre-placed.
            if (playerCharacter == null)
            {
                Debug.LogError("[GeneracionEscena] LevelTimer: playerCharacter is null. This might affect other logic if victory depended on player state/position.", this);
                // For activating a pre-placed object, this might not be a critical failure for victory itself.
            }

            // No longer instantiating, so winPos calculation is not needed for spawning.
            // The victoryObjectInScene is assumed to be pre-placed.
            // float winX = playerCharacter.transform.position.x;
            // float winY = startCharacterY + victoryPrefabYOffset; 
            // float winZ = playerCharacter.transform.position.z; 
            // Vector3 winPos = new Vector3(winX, winY, winZ); 

            Debug.Log($"[GeneracionEscena] LevelTimer: Level duration reached. Attempting to activate victoryObjectInScene '{victoryObjectInScene.name}'", this);
            
            victoryObjectInScene.SetActive(true); // Activate the pre-existing object

            if (victoryObjectInScene.activeSelf)
            {
                Debug.Log($"[GeneracionEscena] LevelTimer: Successfully activated '{victoryObjectInScene.name}' (Instance ID: {victoryObjectInScene.GetInstanceID()}) at {victoryObjectInScene.transform.position}", this);
                Debug.Log($"[GeneracionEscena] VictoryObject '{victoryObjectInScene.name}' - IsActive: {victoryObjectInScene.activeSelf}, IsActiveInHierarchy: {victoryObjectInScene.activeInHierarchy}", victoryObjectInScene);
                Debug.Log($"[GeneracionEscena] VictoryObject '{victoryObjectInScene.name}' - Layer: {LayerMask.LayerToName(victoryObjectInScene.layer)} (ID: {victoryObjectInScene.layer})", victoryObjectInScene);
                Debug.Log($"[GeneracionEscena] VictoryObject '{victoryObjectInScene.name}' - Scale: {victoryObjectInScene.transform.localScale}", victoryObjectInScene);

                Renderer rend = victoryObjectInScene.GetComponentInChildren<Renderer>(); // GetComponentInChildren to find renderer on child objects if main has none
                if (rend != null)
                {
                    Debug.Log($"[GeneracionEscena] VictoryObject '{victoryObjectInScene.name}' - Renderer found (possibly on child): {rend.GetType().Name}, IsEnabled: {rend.enabled}, IsVisible: {rend.isVisible}", victoryObjectInScene);
                    if (rend.material != null)
                    {
                        Debug.Log($"[GeneracionEscena] VictoryObject '{victoryObjectInScene.name}' - Material: {rend.material.name}, Shader: {rend.material.shader.name}", victoryObjectInScene);
                        if (rend.material.HasProperty("_Color"))
                        {
                            Debug.Log($"[GeneracionEscena] VictoryObject '{victoryObjectInScene.name}' - Material Color: {rend.material.color}", victoryObjectInScene);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[GeneracionEscena] VictoryObject '{victoryObjectInScene.name}' - Renderer has no material assigned.", victoryObjectInScene);
                    }
                }
                else
                {
                    Debug.LogWarning($"[GeneracionEscena] VictoryObject '{victoryObjectInScene.name}' - No Renderer component found in children. It might not be visible.", victoryObjectInScene);
                }

                SpriteRenderer spriteRend = victoryObjectInScene.GetComponentInChildren<SpriteRenderer>(); // GetComponentInChildren
                if (spriteRend != null)
                {
                    Debug.Log($"[GeneracionEscena] VictoryObject '{victoryObjectInScene.name}' - SpriteRenderer found (possibly on child): IsEnabled: {spriteRend.enabled}, Sprite: {(spriteRend.sprite != null ? spriteRend.sprite.name : "null")}, Color: {spriteRend.color}", victoryObjectInScene);
                }
                else if (rend == null) 
                {
                    Debug.LogWarning($"[GeneracionEscena] VictoryObject '{victoryObjectInScene.name}' - No SpriteRenderer component found in children either.", victoryObjectInScene);
                }

                // Check for VictoryZone, assume it's pre-configured
                VictoryZone vz = victoryObjectInScene.GetComponent<VictoryZone>();
                if (vz == null)
                {
                    Debug.LogWarning($"[GeneracionEscena] VictoryObject '{victoryObjectInScene.name}' does not have a VictoryZone component attached. Scene transition will not occur on touch.", victoryObjectInScene);
                }
                else
                {
                    // Optionally, you could verify vz.NextSceneName here if needed, but it should be set in Inspector.
                    Debug.Log($"[GeneracionEscena] VictoryObject '{victoryObjectInScene.name}' has VictoryZone. Next scene should be '{vz.NextSceneName}'.", victoryObjectInScene);
                }
                // var vz = win.AddComponent<JUEGOPROYECTO.Management.VictoryZone>(); // No longer adding component dynamically
                // vz.NextSceneName = NextSceneName;

                // Display victory message
                if (victoryTextUI != null)
                {
                    victoryTextUI.text = victoryMessage;
                    victoryTextUI.gameObject.SetActive(true);
                    Debug.Log($"[GeneracionEscena] LevelTimer: Displayed victory message: '{victoryMessage}'", this);
                }
                else
                {
                    Debug.LogWarning("[GeneracionEscena] LevelTimer: victoryTextUI is not assigned. Cannot display victory message.", this);
                }

                // Permit horizontal movement for the player
                if (_horizontalMovementAbility != null)
                {
                    _horizontalMovementAbility.PermitAbility(true);
                    Debug.Log("[GeneracionEscena] LevelTimer: CharacterHorizontalMovement permitted.", this);
                }
                else
                {
                    Debug.LogWarning("[GeneracionEscena] LevelTimer: _horizontalMovementAbility is null. Cannot permit horizontal movement.", this);
                }

                // Stop further level progression (like platform spawning)
                _levelStarted = false;
                Debug.Log("[GeneracionEscena] LevelTimer: Victory achieved. _levelStarted set to false. Platform spawning should stop.", this);
            }
            else
            {
                Debug.LogError($"[GeneracionEscena] LevelTimer: Failed to activate victoryObjectInScene '{victoryObjectInScene.name}'!", this);
            }
        }

        private void UpdateTimerUI()
        {
            if (timerText != null)
            {
                float remaining = Mathf.Max(levelDuration - _elapsedTime, 0f);
                timerText.text = remaining.ToString("0"); // display seconds remaining
            }
        }
    }
}
