using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;

namespace MoreMountains.CorgiEngine
{
    /// <summary>
    /// This decision returns true if the Character is currently in the Dangling state.
    /// It relies on the CharacterDangling ability to set the appropriate CharacterStates.MovementStates.Dangling state.
    /// </summary>
    [AddComponentMenu("Corgi Engine/Character/AI/Decisions/AI Decision Is Dangling")]
    public class AIDecisionIsDangling : AIDecision
    {
        private Character _targetCharacter; // Cache the character component

        /// <summary>
        /// Called once by the AIBrain at the start of the game.
        /// We get the Character component from the AIBrain's GameObject.
        /// </summary>
        public override void Initialization()
        {
            base.Initialization();
            // _brain is inherited from AIDecision and should be set by the AIBrain itself.
            if (_brain != null)
            {
                _targetCharacter = _brain.gameObject.GetComponent<Character>();
            }
            else
            {
                Debug.LogError("AIDecisionIsDangling: AIBrain is null during Initialization. This decision may not work correctly.");
            }
        }

        /// <summary>
        /// The Decide method checks if the character is dangling.
        /// </summary>
        /// <returns>True if the character's movement state is Dangling, false otherwise.</returns>
        public override bool Decide()
        {
            return IsDangling();
        }

        /// <summary>
        /// Checks if the character is currently in the Dangling state.
        /// </summary>
        /// <returns>True if dangling, false otherwise.</returns>
        protected virtual bool IsDangling()
        {
            if (_targetCharacter == null)
            {
                // Attempt to re-fetch if it was null during Initialization or became null
                if (_brain != null)
                {
                    _targetCharacter = _brain.gameObject.GetComponent<Character>();
                }

                if (_targetCharacter == null)
                {
                    Debug.LogWarning("AIDecisionIsDangling: _targetCharacter is null. Cannot determine dangling state.");
                    return false; 
                }
            }

            if (_targetCharacter.MovementState == null)
            {
                Debug.LogWarning("AIDecisionIsDangling: Character.MovementState is null for character: " + _targetCharacter.name);
                return false;
            }
            
            return _targetCharacter.MovementState.CurrentState == CharacterStates.MovementStates.Dangling;
        }

        // No OnEnterState or OnExitState overrides needed for this simple decision.
        // No animation handling is required as per the request.
    }
}
