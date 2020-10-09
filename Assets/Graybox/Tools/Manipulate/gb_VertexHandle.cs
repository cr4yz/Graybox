using Graybox.In;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox.Tools
{
    public class gb_VertexHandle : gb_ToolHandle
    {
        private Vector3 _initialScale;

        private void Start()
        {
            _initialScale = transform.localScale;
        }

        protected override void Update()
        {
            base.Update();

            var handlesScale = 1f;

            if (gb_InputManager.ActiveSceneView.SceneAngle == gb_SceneViewAngle.ThreeDimensional)
            {
                var distToCamera = (gb_InputManager.ActiveSceneView.Camera.transform.position - transform.position).magnitude * .1f;
                handlesScale = Mathf.Max(1, distToCamera);
            }
            else
            {
                handlesScale = gb_InputManager.ActiveSceneView.Camera.orthographicSize / 2f;
            }

            transform.localScale = _initialScale * handlesScale;
        }

    }
}

