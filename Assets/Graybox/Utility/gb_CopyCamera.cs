using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox.Utility
{
    [RequireComponent(typeof(Camera))]
    public class gb_CopyCamera : MonoBehaviour
    {

        public Camera CameraToCopy;

        private Camera _myCamera;

        private void Start()
        {
            _myCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (CameraToCopy)
            {
                _myCamera.aspect = CameraToCopy.aspect;
                _myCamera.fieldOfView = CameraToCopy.fieldOfView;
                _myCamera.orthographic = CameraToCopy.orthographic;
                _myCamera.orthographicSize = CameraToCopy.orthographicSize;
            }
        }

    }
}
