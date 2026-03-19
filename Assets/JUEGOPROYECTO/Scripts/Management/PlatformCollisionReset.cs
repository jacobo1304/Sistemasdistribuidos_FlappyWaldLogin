using UnityEngine;
using MoreMountains.CorgiEngine;

namespace JUEGOPROYECTO.Management
{
    /// <summary>
    /// Resets the level when the player collides with the platform.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlatformCollisionReset : MonoBehaviour
    {
        private bool _killTriggered;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TryKill(collision);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision == null || collision.collider == null)
            {
                return;
            }

            TryKill(collision.collider);
        }

        private void TryKill(Collider2D other)
        {
            if (_killTriggered || other == null)
            {
                return;
            }

            Character character = other.GetComponentInParent<Character>();
            if (character == null)
            {
                return;
            }

            if (character.CharacterHealth == null)
            {
                return;
            }

            _killTriggered = true;
            character.CharacterHealth.Kill();
        }
    }
}
