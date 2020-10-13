using UnityEngine;
using UnityEngine.Events;

namespace Graybox.Utility
{
    public class gb_TransformEvent : MonoBehaviour
    {

        public UnityEvent<Vector3> OnMove = new UnityEvent<Vector3>();
        public UnityEvent<Vector3> OnScale = new UnityEvent<Vector3>();
        public UnityEvent<Quaternion> OnRotate = new UnityEvent<Quaternion>();

        private Vector3 _prevPosition;
        private Vector3 _prevScale;
        private Quaternion _prevRotation;

        private void Start()
        {
            _prevPosition = transform.position;
            _prevScale = transform.localScale;
            _prevRotation = transform.rotation;
        }

        private void LateUpdate()
        {
            if(_prevPosition != transform.position)
            {
                var delta = transform.position - _prevPosition;
                _prevPosition = transform.position;
                OnMove?.Invoke(delta);
            }
            if(_prevScale != transform.localScale)
            {
                var delta = transform.localScale - _prevScale;
                _prevScale = transform.localScale;
                OnScale?.Invoke(delta);
            }
            if(Quaternion.Angle(_prevRotation, transform.rotation) > 1f)
            {
                var deltaRotation = transform.rotation * Quaternion.Inverse(_prevRotation);
                _prevRotation = transform.rotation;
                OnRotate?.Invoke(deltaRotation);
            }
        }

        public void Sync()
        {
            _prevPosition = transform.position;
            _prevScale = transform.localScale;
            _prevRotation = transform.rotation;
        }

    }
}

