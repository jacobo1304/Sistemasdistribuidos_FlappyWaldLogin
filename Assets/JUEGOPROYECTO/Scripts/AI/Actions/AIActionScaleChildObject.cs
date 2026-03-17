using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;

namespace MoreMountains.CorgiEngine
{
    /// <summary>
    /// This AI Action activates a specified child GameObject and gradually scales it up.
    /// Designed for effects like an expanding circular area.
    /// </summary>
    [AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Scale Child Object")]
    public class AIActionScaleChildObject : AIAction
    {
        /// <summary>The GameObject (ideally a child of this AI's character) to activate and scale.</summary>
        [Tooltip("The GameObject (ideally a child of this AI's character) to activate and scale.")]
        public GameObject ChildObjectToScale;
        /// <summary>The speed at which the object scales (units per second, applied uniformly).</summary>
        [Tooltip("The speed at which the object scales (units per second, applied uniformly).")]
        public float ScaleSpeed = 1.0f;
        /// <summary>The maximum scale multiplier relative to its initial scale (e.g., 2.0 means double size).</summary>
        [Tooltip("The maximum scale multiplier relative to its initial scale (e.g., 2.0 means double size).")]
        public float MaxScaleMultiplier = 2.0f;
        /// <summary>If true, the object's scale will be reset to its initial scale when this AI state is exited.</summary>
        [Tooltip("If true, the object's scale will be reset to its initial scale when this AI state is exited.")]
        public bool ResetScaleOnExit = true;
        /// <summary>If true, the object will be deactivated when this AI state is exited.</summary>
        [Tooltip("If true, the object will be deactivated when this AI state is exited.")]
        public bool DeactivateOnExit = true;

        protected Transform _childTransform;
        protected Vector3 _initialScale;
        protected Vector3 _targetMaxScaleVector;
        protected bool _initialized = false;

        /// <summary>
        /// On Initialization, we cache references.
        /// </summary>
        public override void Initialization()
        {
            Debug.Log($"[AIActionScaleChildObject] Initialization called on {this.GetType().Name} for {(_brain.gameObject.name)}");
            if(!ShouldInitialize) return;
            base.Initialization();
            // _character is initialized in base.Initialization()
            
            if (ChildObjectToScale == null)
            {
                Debug.LogWarningFormat("{0} on {1} : ChildObjectToScale has not been set in the Inspector.", this.GetType().Name, _brain.gameObject.name);
                _initialized = false;
                return;
            }
            _childTransform = ChildObjectToScale.transform;
            _initialized = true;
        }

        /// <summary>
        /// On entering the state, we activate the object, store its initial scale, and calculate the target maximum scale.
        /// </summary>
        public override void OnEnterState()
        {
            base.OnEnterState();
            Debug.Log($"[AIActionScaleChildObject] OnEnterState called. Initialized={_initialized}, ChildObjectToScale={(ChildObjectToScale!=null?ChildObjectToScale.name:"null")}");
            if (!_initialized || _childTransform == null)
            {
                return;
            }

            ChildObjectToScale.SetActive(true);
            _initialScale = _childTransform.localScale;
            _targetMaxScaleVector = _initialScale * MaxScaleMultiplier;
        }

        /// <summary>
        /// In PerformAction, we scale the object up each frame until it reaches its max defined scale.
        /// </summary>
        public override void PerformAction()
        {
            Debug.Log($"[AIActionScaleChildObject] PerformAction called. CurrentScale={_childTransform?.localScale}");
            if (!_initialized || _childTransform == null)
            {
                return;
            }

            // Check if we've already reached or exceeded the max scale on the X-axis (assuming uniform scaling)
            if (_childTransform.localScale.x < _targetMaxScaleVector.x)
            {
                _childTransform.localScale += Vector3.one * ScaleSpeed * Time.deltaTime;
                // Clamp the scale to the target max scale
                _childTransform.localScale = Vector3.Min(_childTransform.localScale, _targetMaxScaleVector);
            }
        }

        /// <summary>
        /// On exiting the state, we optionally reset the scale and deactivate the object.
        /// </summary>
        public override void OnExitState()
        {
            Debug.Log($"[AIActionScaleChildObject] OnExitState called. ResetScale={ResetScaleOnExit}, DeactivateOnExit={DeactivateOnExit}");
            base.OnExitState();
            if (!_initialized || _childTransform == null)
            {
                return;
            }

            if (ResetScaleOnExit)
            {
                _childTransform.localScale = _initialScale;
            }
            if (DeactivateOnExit)
            {
                ChildObjectToScale.SetActive(false);
            }
        }
    }
}
