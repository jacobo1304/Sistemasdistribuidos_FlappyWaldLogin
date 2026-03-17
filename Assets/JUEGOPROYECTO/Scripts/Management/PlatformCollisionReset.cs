using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.CorgiEngine;

namespace JUEGOPROYECTO.Management
{
    /// <summary>
    /// Resets the level when the player collides with the platform.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlatformCollisionReset : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<Character>() != null)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
