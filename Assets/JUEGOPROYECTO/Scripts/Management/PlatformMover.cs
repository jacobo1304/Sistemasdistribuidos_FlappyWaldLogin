using UnityEngine;

namespace JUEGOPROYECTO.Management
{
    /// <summary>
    /// Moves the platform horizontally back and forth.
    /// </summary>
    [RequireComponent(typeof(Transform))]
    public class PlatformMover : MonoBehaviour
    {
        [Tooltip("Movement speed in units per second.")]
        public float speed = 2f;
        [Tooltip("Horizontal range from the start position.")]
        public float range = 2f;

        private Vector3 _startPosition;
        private int _direction = 1;

        void Start()
        {
            _startPosition = transform.position;
        }

        void Update()
        {
            transform.Translate(Vector3.right * speed * _direction * Time.deltaTime);
            if (Mathf.Abs(transform.position.x - _startPosition.x) >= range)
            {
                _direction *= -1;
            }
        }
    }
}
