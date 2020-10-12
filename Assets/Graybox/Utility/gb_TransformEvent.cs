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
            _prevRotation = Quaternion.identity;
        }

        private void Update()
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
            if(_prevRotation != transform.rotation)
            {
                var delta = Quaternion.Inverse(_prevRotation) * transform.rotation;
                _prevRotation = transform.rotation;
                OnRotate?.Invoke(delta);
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

