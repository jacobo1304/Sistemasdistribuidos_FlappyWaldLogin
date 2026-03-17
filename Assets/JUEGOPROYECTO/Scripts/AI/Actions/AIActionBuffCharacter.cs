using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;

namespace MoreMountains.CorgiEngine
{
    /// <summary>
    /// This AI Action buffs the character by playing a sound, increasing speed, size, and weapon fire rate.
    /// The buffs last as long as the AI is in this state.
    /// </summary>
    [AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Buff Character")]
    public class AIActionBuffCharacter : AIAction
    {
        [Header("Buff Settings")]
        /// <summary>The sound effect to play when the buff starts.</summary>
        [Tooltip("The sound effect to play when the buff starts.")]
        public AudioClip BuffSound;
        /// <summary>Multiplier for the character's movement speed (e.g., 1.5 for 50% faster).</summary>
        [Tooltip("Multiplier for the character's movement speed (e.g., 1.5 for 50% faster).")]
        public float SpeedMultiplier = 1.5f;
        /// <summary>Multiplier for the character's local scale (e.g., 1.2 for 20% larger).</summary>
        [Tooltip("Multiplier for the character's local scale (e.g., 1.2 for 20% larger).")]
        public float SizeMultiplier = 1.2f;
        /// <summary>Multiplier for the weapon's fire rate (e.g., 1.5 for 50% faster shooting).</summary>
        [Tooltip("Multiplier for the weapon's fire rate (e.g., 1.5 for 50% faster shooting).")]
        public float ShootRateMultiplier = 1.5f;

        protected CharacterHorizontalMovement _horizontalMovement;
        protected CharacterHandleWeapon _handleWeapon;
        protected Character _character;
        protected CorgiController _corgiController; // Added for position adjustment
        protected BoxCollider2D _boxCollider; // Added to get collider size

        protected float _originalMovementSpeed;
        protected Vector3 _originalScale;
        protected float _originalTimeBetweenUses;
        protected Weapon _cachedWeapon; // Cache the weapon that was buffed
        protected bool _buffsApplied = false;
        protected Vector3 _positionAdjustment = Vector3.zero; // Added to store position offset

        /// <summary>
        /// On Initialization, we get the necessary character abilities.
        /// </summary>
        public override void Initialization()
        {
            Debug.Log($"[AIActionBuffCharacter] Initialization called on {this.GetType().Name} for {_brain.gameObject.name}");
            if (!ShouldInitialize) return;
            base.Initialization(); // This initializes _brain, and usually _character
            _character = _brain.gameObject.GetComponentInParent<Character>();

            if (_character != null)
            {
                _horizontalMovement = _character.FindAbility<CharacterHorizontalMovement>();
                _handleWeapon = _character.FindAbility<CharacterHandleWeapon>();
                _corgiController = _character.GetComponent<CorgiController>(); 
                _boxCollider = _character.GetComponent<BoxCollider2D>(); // Get BoxCollider2D

                if (_corgiController == null)
                {
                    Debug.LogWarningFormat("{0} on {1}: CorgiController component not found on Character. Positional adjustment for scaling will not be applied.", this.GetType().Name, _brain.gameObject.name);
                }
            }
            else
            {
                Debug.LogWarningFormat("{0} on {1}: Character component not found in parent. Buffs cannot be applied.", this.GetType().Name, _brain.gameObject.name);
            }
        }

        /// <summary>
        /// When entering the state, apply all buffs.
        /// </summary>
        public override void OnEnterState()
        {
            Debug.Log($"[AIActionBuffCharacter] OnEnterState called. BuffSound={BuffSound}");
            base.OnEnterState();
            if (_character == null) 
            {
                Debug.LogWarningFormat("{0} on {1}: Character is null in OnEnterState. Buffs cannot be applied.", this.GetType().Name, _brain.gameObject.name);
                return;
            }

            // Play sound
            if (BuffSound != null)
            {
                MMSoundManagerSoundPlayEvent.Trigger(BuffSound, MMSoundManager.MMSoundManagerTracks.Sfx, _character.transform.position);
            }

            // Apply speed buff
            if (_horizontalMovement != null)
            {
                _originalMovementSpeed = _horizontalMovement.MovementSpeed;
                _horizontalMovement.MovementSpeed *= SpeedMultiplier;
                _buffsApplied = true;
            }

            // Apply size buff
            _originalScale = _character.transform.localScale;
            _character.transform.localScale *= SizeMultiplier;
            _buffsApplied = true;

            // Adjust position if CorgiController and BoxCollider2D are present to prevent ground clipping/pushing
            if (_corgiController != null && _boxCollider != null && SizeMultiplier != 1.0f)
            {
                // Calculate the actual height of the collider before this buff's scale was applied
                // The collider's size is in local space, so we multiply by the original Y scale.
                float actualColliderHeightBeforeBuff = _boxCollider.size.y * _originalScale.y;
                
                // Calculate how much the bottom edge of a center-pivoted character would move down
                // This assumes the character's pivot is at its center.
                float upwardDisplacement = (actualColliderHeightBeforeBuff * (SizeMultiplier - 1f)) / 2f;
                
                _positionAdjustment = new Vector3(0, upwardDisplacement, 0);
                // We directly modify the transform's position. If the CorgiController fights this,
                // we might need to use _corgiController.MovePosition() or a similar method,
                // but for an instantaneous adjustment, this is often okay.
                _character.transform.position += _positionAdjustment;
                Debug.Log($"[AIActionBuffCharacter] Applied position adjustment: {_positionAdjustment} due to scaling.");
            }

            // Apply shoot rate buff
            if (_handleWeapon != null && _handleWeapon.CurrentWeapon != null)
            {
                _originalTimeBetweenUses = _handleWeapon.CurrentWeapon.TimeBetweenUses;
                _handleWeapon.CurrentWeapon.TimeBetweenUses /= ShootRateMultiplier;
                _buffsApplied = true;
            }
        }

        /// <summary>
        /// PerformAction is called every frame, but for this buff, effects are applied on enter and reverted on exit.
        /// </summary>
        public override void PerformAction()
        {
            Debug.Log($"[AIActionBuffCharacter] PerformAction called");
            // Nothing to do here continuously for this version of the buff.
        }

        /// <summary>
        /// When exiting the state, revert all buffs.
        /// </summary>
        public override void OnExitState()
        {
            Debug.Log($"[AIActionBuffCharacter] OnExitState called. BuffsApplied={_buffsApplied}");
            base.OnExitState();
            if (_character == null || !_buffsApplied) 
            {
                 if (_character == null) Debug.LogWarningFormat("{0} on {1}: Character is null in OnExitState. Buffs cannot be reverted.", this.GetType().Name, _brain.gameObject.name);
                _positionAdjustment = Vector3.zero; // Ensure reset if buffs were not properly applied
                return;
            }

            // Revert position adjustment before reverting scale
            if (_corgiController != null && _positionAdjustment != Vector3.zero)
            {
                _character.transform.position -= _positionAdjustment;
                Debug.Log($"[AIActionBuffCharacter] Reverted position adjustment: {_positionAdjustment}.");
                _positionAdjustment = Vector3.zero; // Reset for next time
            }

            // Revert speed buff
            if (_horizontalMovement != null)
            {
                _horizontalMovement.MovementSpeed = _originalMovementSpeed;
            }

            // Revert size buff
            _character.transform.localScale = _originalScale;

            // Revert shoot rate buff
            if (_handleWeapon != null && _handleWeapon.CurrentWeapon != null)
            {
                _handleWeapon.CurrentWeapon.TimeBetweenUses = _originalTimeBetweenUses;
            }
            _buffsApplied = false;
        }
    }
}
