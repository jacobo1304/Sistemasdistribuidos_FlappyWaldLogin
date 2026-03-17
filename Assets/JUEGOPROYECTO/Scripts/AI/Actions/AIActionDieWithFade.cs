using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
using MoreMountains.Feedbacks; // Add feedbacks namespace
using System.Collections; // Required for IEnumerator
using UnityEngine.SceneManagement; // Add at top

namespace CorgiEngineExtensions
{
    /// <summary>
    /// This AI Action handles the character's death sequence, playing a sound,
    /// triggering the standard Corgi death, and then fading out the sprite.
    /// </summary>
    [AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Die With Fade")]
    public class AIActionDieWithFade : AIAction
    {
        [Header("Feedback")]
        [Tooltip("Feedback to play on death (handles sound)")]
        public MMFeedbacks DeathFeedback;

        [Header("Death Visuals")]
        [Tooltip("Delay after entering the state before the fade out begins.")]
        public float FadeDelay = 0.0f;
        [Tooltip("Duration of the fade out effect.")]
        public float FadeDuration = 1.0f;
        [Tooltip("Whether to destroy the character's GameObject after it has faded out.")]
        public bool DestroyAfterFade = true;

        [Header("Scene Transition")]
        [Tooltip("Name of the scene to load after death fade and delay.")]
        public string NextSceneName;
        [Tooltip("Delay after fade before loading the specified scene.")]
        public float SceneLoadDelay = 2f;

        protected Character _character;
        protected Health _health;
        protected SpriteRenderer _spriteRenderer;
        protected Color _initialColor;
        protected float _timer = 0f;
        protected bool _fadeComplete = false;
        private bool _sceneLoadStarted = false;

        /// <summary>
        /// On Initialization, we get references to necessary components.
        /// </summary>
        public override void Initialization()
        {
            Debug.Log($"[AIActionDieWithFade] Initialization called on {this.GetType().Name} for {(_brain.gameObject.name)}");
            if (!ShouldInitialize) return;
            base.Initialization(); // Initializes _character and _brain
            _character = _brain.gameObject.GetComponentInParent<Character>();
            if (_character != null)
            {
                _health = _character.GetComponent<Health>();
                _spriteRenderer = _character.GetComponent<SpriteRenderer>();
            }
            if (_character == null)
            {
                Debug.LogErrorFormat("{0} on {1}: Character component not found on AI's parent GameObject. Death action cannot proceed.", this.GetType().Name, _brain.gameObject.name);
            }
        }

        /// <summary>
        /// On entering the state, trigger death and play feedback.
        /// </summary>
        public override void OnEnterState()
        {
            Debug.Log($"[AIActionDieWithFade] OnEnterState called. Playing feedback: {DeathFeedback}");
            base.OnEnterState();

            if (_character == null) return;

            // Trigger Corgi Engine death
            var health = _character.GetComponent<Health>();
            if (health != null && _character.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead)
            {
                health.Kill();
            }

            // Play sound via feedback
            DeathFeedback?.PlayFeedbacks();
            // Prepare for manual fade
            _timer = 0f;
            _fadeComplete = false;
            if (_spriteRenderer != null)
            {
                _initialColor = _spriteRenderer.color;
            }
        }

        /// <summary>
        /// PerformAction is required by AIAction but is handled by feedback.
        /// </summary>
        public override void PerformAction()
        {
            Debug.Log($"[AIActionDieWithFade] PerformAction called. Timer={_timer}, FadeComplete={_fadeComplete}");
            if (_fadeComplete) return;
            if (_spriteRenderer != null)
            {
                _timer += Time.deltaTime;
                if (_timer >= FadeDelay)
                {
                    float elapsed = _timer - FadeDelay;
                    if (elapsed < FadeDuration)
                    {
                        float alpha = Mathf.Lerp(_initialColor.a, 0f, elapsed / FadeDuration);
                        _spriteRenderer.color = new Color(_initialColor.r, _initialColor.g, _initialColor.b, alpha);
                    }
                    else
                    {
                        _spriteRenderer.color = new Color(_initialColor.r, _initialColor.g, _initialColor.b, 0f);
                        _fadeComplete = true;
                        if (!_sceneLoadStarted)
                        {
                            _sceneLoadStarted = true;
                            StartCoroutine(SceneTransitionCoroutine());
                        }
                        if (DestroyAfterFade)
                        {
                            Destroy(_character.gameObject);
                        }
                    }
                }
            }
            else
            {
                _timer += Time.deltaTime;
                if (_timer >= FadeDelay + FadeDuration && DestroyAfterFade)
                {
                    _fadeComplete = true;
                    Destroy(_character.gameObject);
                }
            }
        }

        public override void OnExitState()
        {
            Debug.Log($"[AIActionDieWithFade] OnExitState called. FadeComplete={_fadeComplete}");
            base.OnExitState();
            if (!_fadeComplete && _spriteRenderer != null)
            {
                _spriteRenderer.color = new Color(_initialColor.r, _initialColor.g, _initialColor.b, 0f);
            }
            if (!_fadeComplete && DestroyAfterFade)
            {
                Destroy(_character.gameObject);
            }
            _fadeComplete = true;
        }

        private IEnumerator SceneTransitionCoroutine()
        {
            yield return new WaitForSeconds(SceneLoadDelay);
            if (DestroyAfterFade && _character != null)
            {
                Destroy(_character.gameObject);
            }
            if (!string.IsNullOrEmpty(NextSceneName))
            {
                SceneManager.LoadScene(NextSceneName);
            }
        }
    }
}
