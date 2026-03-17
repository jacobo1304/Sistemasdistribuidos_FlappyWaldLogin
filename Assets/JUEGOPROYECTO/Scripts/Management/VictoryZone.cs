using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.CorgiEngine;

namespace JUEGOPROYECTO.Management
{
    /// <summary>
    /// Triggers scene change when player reaches victory object.
    /// </summary>
    public class VictoryZone : MonoBehaviour
    {
        [Tooltip("Name of the scene to load on victory.")]
        public string NextSceneName = "nivel3";

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.GetComponent<Character>() != null)
            {
                SceneManager.LoadScene(NextSceneName);
            }
        }
    }
}
