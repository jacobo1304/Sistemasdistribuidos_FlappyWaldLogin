using UnityEngine;

namespace JUEGOPROYECTO.Management
{
    /// <summary>
    /// Moves a platform from right to left at a constant speed and destroys it when it passes the left bound.
    /// </summary>
    public class ScrollingPlatform : MonoBehaviour
    {
        [Tooltip("Movement speed in units per second.")]
        public float speed = 4f;
        [Tooltip("X position at which the platform is destroyed.")]
        public float leftBound = -10f;

        void Update()
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime);
            if (transform.position.x <= leftBound)
            {
                Destroy(gameObject);
            }
        }
    }
}
