using UnityEngine;
using MoreMountains.CorgiEngine; // Required for CorgiEngineEvent and Character
using MoreMountains.Tools;      // Required for MMEventListener
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace JUEGOPROYECTO.Management
{
    /// <summary>
    /// Manages game difficulty settings, affecting player stats, abilities, and level content.
    /// </summary>
    public class GameDifficultyManager : MonoBehaviour, MMEventListener<CorgiEngineEvent> // Implement MMEventListener
    {
        public enum DifficultyMode { Easy, Medium, Hard }

        public static DifficultyMode CurrentDifficulty { get; private set; } = DifficultyMode.Medium;

        [Header("Player Character Settings")]
        [Tooltip("Player health points for Easy mode.")]
        [SerializeField] private int easyHealth = 100;
        [Tooltip("Player health points for Medium mode.")]
        [SerializeField] private int mediumHealth = 75;
        [Tooltip("Player health points for Hard mode.")]
        [SerializeField] private int hardHealth = 50;
        [Tooltip("Number of jumps for Easy mode.")]
        [SerializeField] private int easyJumps = 2;
        [Tooltip("Number of jumps for Medium mode.")]
        [SerializeField] private int mediumJumps = 2; 
        [Tooltip("Number of jumps for Hard mode.")]
        [SerializeField] private int hardJumps = 1; 

        [Header("Coin Spawning")]
        [Tooltip("Prefab for the coin collectible.")]
        [SerializeField] private GameObject coinPrefab;
        [Tooltip("Number of coins to spawn in Easy mode.")]
        [SerializeField] private int easyCoinCount = 20;
        [Tooltip("Number of coins to spawn in Medium mode.")]
        [SerializeField] private int mediumCoinCount = 15;
        [Tooltip("Number of coins to spawn in Hard mode.")]
        [SerializeField] private int hardCoinCount = 10;
        // TODO: Implement logic for coin spawn positions (e.g., list of transforms, area bounds)

        [Header("Enemy Spawning")]
        [Tooltip("List of enemy prefabs to choose from for spawning.")]
        [SerializeField] private List<GameObject> enemyPrefabs;
        [Tooltip("Number of enemies to spawn in Easy mode.")]
        [SerializeField] private int easyEnemyCount = 3;
        [Tooltip("Number of enemies to spawn in Medium mode.")]
        [SerializeField] private int mediumEnemyCount = 5;
        [Tooltip("Number of enemies to spawn in Hard mode.")]
        [SerializeField] private int hardEnemyCount = 8;
        // TODO: Implement logic for enemy spawn positions and selection from the list

        [Header("Game Manager Settings")] // New Header
        [Tooltip("Maximum lives for Easy mode.")]
        [SerializeField] private int easyMaxLives = 5;
        [Tooltip("Maximum lives for Medium mode.")]
        [SerializeField] private int mediumMaxLives = 3;
        [Tooltip("Maximum lives for Hard mode.")]
        [SerializeField] private int hardMaxLives = 1;

        public static GameDifficultyManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                ApplyGlobalDifficultySettings(); // Apply for default difficulty
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            this.MMEventStartListening<CorgiEngineEvent>(); // Subscribe to CorgiEngineEvents
            Debug.Log("[GameDifficultyManager] Subscribed to sceneLoaded and CorgiEngineEvent.");
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            this.MMEventStopListening<CorgiEngineEvent>(); // Unsubscribe from CorgiEngineEvents
            Debug.Log("[GameDifficultyManager] Unsubscribed from sceneLoaded and CorgiEngineEvent.");
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[GameDifficultyManager] Scene '{scene.name}' loaded. Mode: {mode}. Triggering spawn of collectibles and enemies.");
            // Reapply global difficulty settings (in case returning from menu)
            ApplyGlobalDifficultySettings();
            SpawnCollectiblesAndEnemies();
        }

        // Listener for CorgiEngineEvents
        public virtual void OnMMEvent(CorgiEngineEvent corgiEvent)
        {
            if (corgiEvent.EventType == CorgiEngineEventTypes.LevelStart || corgiEvent.EventType == CorgiEngineEventTypes.Respawn)
            {
                string eventName = corgiEvent.EventType == CorgiEngineEventTypes.LevelStart ? "LevelStart" : "Respawn";
                if (corgiEvent.OriginCharacter != null)
                {
                    Debug.Log($"[GameDifficultyManager] {eventName} event received. Player character '{corgiEvent.OriginCharacter.name}' found. Applying difficulty settings to player.");
                    ApplySettingsToPlayer(corgiEvent.OriginCharacter);
                }
                else
                {
                    Debug.LogError($"[GameDifficultyManager] {eventName} event received, but OriginCharacter is null.");
                }
            }
        }
        
        /// <summary>
        /// Sets the game difficulty to Easy. Called by UI Button.
        /// </summary>
        public void SetEasyMode()
        {
            CurrentDifficulty = DifficultyMode.Easy;
            Debug.Log("[GameDifficultyManager] Difficulty set to Easy.");
            ApplyGlobalDifficultySettings();
            // Attempt to apply settings to an existing player character immediately
            Character playerCharacter = FindObjectOfType<Character>();
            if (playerCharacter != null)
            {
                Debug.Log($"[GameDifficultyManager] Applying Easy settings to existing player '{playerCharacter.name}' immediately.");
                ApplySettingsToPlayer(playerCharacter);
            }
        }

        /// <summary>
        /// Sets the game difficulty to Medium. Called by UI Button.
        /// </summary>
        public void SetMediumMode()
        {
            CurrentDifficulty = DifficultyMode.Medium;
            Debug.Log("[GameDifficultyManager] Difficulty set to Medium.");
            ApplyGlobalDifficultySettings();
            // Attempt to apply settings to an existing player character immediately
            Character playerCharacter = FindObjectOfType<Character>();
            if (playerCharacter != null)
            {
                Debug.Log($"[GameDifficultyManager] Applying Medium settings to existing player '{playerCharacter.name}' immediately.");
                ApplySettingsToPlayer(playerCharacter);
            }
        }

        /// <summary>
        /// Sets the game difficulty to Hard. Called by UI Button.
        /// </summary>
        public void SetHardMode()
        {
            CurrentDifficulty = DifficultyMode.Hard;
            Debug.Log("[GameDifficultyManager] Difficulty set to Hard.");
            ApplyGlobalDifficultySettings();
            // Attempt to apply settings to an existing player character immediately
            Character playerCharacter = FindObjectOfType<Character>();
            if (playerCharacter != null)
            {
                Debug.Log($"[GameDifficultyManager] Applying Hard settings to existing player '{playerCharacter.name}' immediately.");
                ApplySettingsToPlayer(playerCharacter);
            }
        }

        private void ApplyGlobalDifficultySettings()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("[GameDifficultyManager] GameManager.Instance is null. Cannot apply global difficulty settings at this time.");
                return;
            }

            int targetMaxLives = mediumMaxLives; 

            switch (CurrentDifficulty)
            {
                case DifficultyMode.Easy:
                    targetMaxLives = easyMaxLives;
                    break;
                case DifficultyMode.Medium:
                    targetMaxLives = mediumMaxLives;
                    break;
                case DifficultyMode.Hard:
                    targetMaxLives = hardMaxLives;
                    break;
            }

            GameManager.Instance.MaximumLives = targetMaxLives;
            GameManager.Instance.CurrentLives = targetMaxLives; // Corrected: Directly set CurrentLives

            Debug.Log($"[GameDifficultyManager] Global settings applied for {CurrentDifficulty} mode. Max Lives set to: {GameManager.Instance.MaximumLives}, Current Lives set to: {GameManager.Instance.CurrentLives}");
        }

        /// <summary>
        /// Applies the current difficulty settings to the provided player character.
        /// This should be called when the player character is initialized in a scene.
        /// </summary>
        /// <param name="playerCharacter">The player's Character component.</param>
        public void ApplySettingsToPlayer(Character playerCharacter)
        {
            if (playerCharacter == null)
            {
                Debug.LogError("[GameDifficultyManager] PlayerCharacter reference is null. Cannot apply settings.");
                return;
            }

            Debug.Log($"[GameDifficultyManager] Applying {CurrentDifficulty} mode settings to player {playerCharacter.name}.");

            Health playerHealthComponent = playerCharacter.GetComponent<Health>();
            CharacterDash dashAbility = playerCharacter.FindAbility<CharacterDash>();
            CharacterJump jumpAbility = playerCharacter.FindAbility<CharacterJump>();
            CharacterWallClinging wallClingAbility = playerCharacter.FindAbility<CharacterWallClinging>();
            CharacterJetpack jetpackAbility = playerCharacter.FindAbility<CharacterJetpack>(); // Add this line

            // Log if ability components are found
            if (dashAbility == null) 
            {
                Debug.LogWarning($"[GameDifficultyManager] CharacterDash ability component not found on {playerCharacter.name}. Dash settings will not be applied.");
            }
            if (jumpAbility == null) 
            {
                Debug.LogWarning($"[GameDifficultyManager] CharacterJump ability component not found on {playerCharacter.name}. Jump settings will not be applied.");
            }
            if (wallClingAbility == null) 
            {
                Debug.LogWarning($"[GameDifficultyManager] CharacterWallClinging ability component not found on {playerCharacter.name}. WallCling settings will not be applied. (This might be normal if you use a different wall grip ability).");
            }
            if (jetpackAbility == null) // Add this block
            {
                Debug.LogWarning($"[GameDifficultyManager] CharacterJetpack ability component not found on {playerCharacter.name}. Jetpack settings will not be applied.");
            }
            // Note: For Wall Grip, Corgi Engine might use CharacterWalljump or another specific ability.
            // Adjust 'CharacterWallClinging' if your setup uses a different ability for wall grip.

            int targetHealth = mediumHealth; // Default to medium settings

            // Default ability states (usually enabled in Corgi)
            bool dashEnabled = true;
            // int numberOfJumps = 2; // REMOVED
            int currentNumberOfJumps = mediumJumps; // Default to medium settings
            bool wallClingEnabled = true;
            bool jetpackEnabled = true; // Add this line, default to true (for Easy)

            switch (CurrentDifficulty)
            {
                case DifficultyMode.Easy:
                    targetHealth = easyHealth;
                    dashEnabled = true;
                    currentNumberOfJumps = easyJumps; // USE SERIALIZED FIELD
                    wallClingEnabled = true;
                    jetpackEnabled = true; // Jetpack enabled for Easy
                    break;

                case DifficultyMode.Medium:
                    targetHealth = mediumHealth;
                    dashEnabled = true; // Changed from false to true
                    currentNumberOfJumps = mediumJumps; // USE SERIALIZED FIELD
                    wallClingEnabled = true;
                    jetpackEnabled = false; // Jetpack disabled for Medium
                    break;

                case DifficultyMode.Hard:
                    targetHealth = hardHealth;
                    dashEnabled = false;
                    currentNumberOfJumps = hardJumps; // USE SERIALIZED FIELD
                    wallClingEnabled = false;
                    jetpackEnabled = false; // Jetpack disabled for Hard
                    break;
            }

            // Apply Health Settings
            if (playerHealthComponent != null)
            {
                playerHealthComponent.MaximumHealth = targetHealth;
                playerHealthComponent.SetHealth(targetHealth, gameObject); // Sets current health and triggers events, passing this manager as instigator
                // playerHealthComponent.UpdateHealthBar(true); // Uncomment if you need to force UI update
                Debug.Log($"[GameDifficultyManager] Player health set to: {playerHealthComponent.CurrentHealth}/{playerHealthComponent.MaximumHealth}");
            }
            else
            {
                Debug.LogWarning("[GameDifficultyManager] Health component not found on player. Cannot set health.");
            }

            // Apply Dash Ability Setting
            if (dashAbility != null)
            {
                Debug.Log($"[GameDifficultyManager] Attempting to set Dash ability for {playerCharacter.name} to: {(dashEnabled ? "Enabled" : "Disabled")}");
                dashAbility.PermitAbility(dashEnabled);
                Debug.Log($"[GameDifficultyManager] Dash ability for {playerCharacter.name} {(dashAbility.AbilityPermitted ? "is now Enabled" : "is now Disabled")}. Requested: {(dashEnabled ? "Enabled" : "Disabled")}");
            }
            // else: warning already logged if component is null

            // Apply Jump Ability Setting (for Double Jump)
            if (jumpAbility != null)
            {
                Debug.Log($"[GameDifficultyManager] Attempting to set NumberOfJumps for {playerCharacter.name} to: {currentNumberOfJumps}");
                jumpAbility.NumberOfJumps = currentNumberOfJumps; // USE currentNumberOfJumps
                Debug.Log($"[GameDifficultyManager] NumberOfJumps for {playerCharacter.name} is now {jumpAbility.NumberOfJumps}. Requested: {currentNumberOfJumps}");
            }
            // else: warning already logged if component is null

            // Apply Wall Grip Ability Setting
            if (wallClingAbility != null)
            {
                Debug.Log($"[GameDifficultyManager] Attempting to set WallClinging ability for {playerCharacter.name} to: {(wallClingEnabled ? "Enabled" : "Disabled")}");
                wallClingAbility.PermitAbility(wallClingEnabled);
                Debug.Log($"[GameDifficultyManager] WallClinging ability for {playerCharacter.name} {(wallClingAbility.AbilityPermitted ? "is now Enabled" : "is now Disabled")}. Requested: {(wallClingEnabled ? "Enabled" : "Disabled")}");
            }
            // else: warning already logged if component is null

            // Apply Jetpack Ability Setting
            if (jetpackAbility != null) // Add this block
            {
                Debug.Log($"[GameDifficultyManager] Attempting to set Jetpack ability for {playerCharacter.name} to: {(jetpackEnabled ? "Enabled" : "Disabled")}");
                jetpackAbility.PermitAbility(jetpackEnabled);
                Debug.Log($"[GameDifficultyManager] Jetpack ability for {playerCharacter.name} {(jetpackAbility.AbilityPermitted ? "is now Enabled" : "is now Disabled")}. Requested: {(jetpackEnabled ? "Enabled" : "Disabled")}");
            }
            // else: warning already logged if component is null
        }

        /// <summary>
        /// Spawns coins and enemies based on the current difficulty settings.
        /// This should be called when a level/scene starts.
        /// </summary>
        public void SpawnCollectiblesAndEnemies()
        {
            Debug.Log($"[GameDifficultyManager] Spawning collectibles and enemies for {CurrentDifficulty} mode.");

            int coinsToSpawn = 0;
            int enemiesToSpawn = 0;

            switch (CurrentDifficulty)
            {
                case DifficultyMode.Easy:
                    coinsToSpawn = easyCoinCount;
                    enemiesToSpawn = easyEnemyCount;
                    break;
                case DifficultyMode.Medium:
                    coinsToSpawn = mediumCoinCount;
                    enemiesToSpawn = mediumEnemyCount;
                    break;
                case DifficultyMode.Hard:
                    coinsToSpawn = hardCoinCount;
                    enemiesToSpawn = hardEnemyCount;
                    break;
            }

            // Spawn Coins
            if (coinPrefab != null)
            {
                for (int i = 0; i < coinsToSpawn; i++)
                {
                    // TODO: Replace Vector3.zero with actual spawn position logic
                    // Example: Instantiate(coinPrefab, GetRandomCoinSpawnPosition(), Quaternion.identity);
                    Debug.Log($"Attempting to spawn coin {i + 1}/{coinsToSpawn}. Implement spawn position logic.");
                }
            }
            else
            {
                Debug.LogWarning("[GameDifficultyManager] Coin prefab is not assigned. Cannot spawn coins.");
            }

            // Spawn Enemies
            if (enemyPrefabs != null && enemyPrefabs.Count > 0)
            {
                for (int i = 0; i < enemiesToSpawn; i++)
                {
                    // GameObject enemyToInstantiate = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)]; // Example: Random enemy from list
                    // TODO: Replace Vector3.zero with actual spawn position logic
                    // Example: Instantiate(enemyToInstantiate, GetRandomEnemySpawnPosition(), Quaternion.identity);
                    Debug.Log($"Attempting to spawn enemy {i + 1}/{enemiesToSpawn}. Implement spawn position and selection logic.");
                }
            }
            else
            {
                Debug.LogWarning("[GameDifficultyManager] Enemy prefabs list is not assigned or empty. Cannot spawn enemies.");
            }
        }
        
        // TODO: Consider adding helper methods like:
        // private Vector3 GetRandomCoinSpawnPosition() { /* ... */ return Vector3.zero; }
        // private Vector3 GetRandomEnemySpawnPosition() { /* ... */ return Vector3.zero; }
    }
}
