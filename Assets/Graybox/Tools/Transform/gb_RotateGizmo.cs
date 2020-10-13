using Graybox.In;
using Graybox.Utility;
using UnityEngine;

namespace Graybox.Tools
{
    public class gb_RotateGizmo : gb_GizmoTool
    {

        public override string ToolName => "Rotate";

        private bool _hitIsBehind;
        private Quaternion _startingRotation;
        private Quaternion _absRotation;
        private Vector3 _startingRotationAxis;

        protected override void OnHandleDown(gb_GizmoHandles handle)
        {
            var hitDist = Vector3.Distance(handle.HitPoint, gb_InputManager.ActiveSceneView.Camera.transform.position);
            var gizmoDist = Vector3.Distance(handle.transform.position, gb_InputManager.ActiveSceneView.Camera.transform.position);
            _hitIsBehind = hitDist > gizmoDist + Mathf.Epsilon;

            _startingRotation = Target.rotation;
            _startingRotationAxis = (Target.rotation * Quaternion.Inverse(_startingRotation)) * handle.Axis;
            _absRotation = Target.rotation;
        }

        protected override void OnDeltaUpdate(gb_GizmoHandles handle, Vector2 screenDelta, Vector3 worldDelta)
        {
            var delta = _startingRotation * Quaternion.Inverse(_absRotation) * Camera.cameraToWorldMatrix.MultiplyVector(new Vector3(screenDelta.y, -screenDelta.x, 0));
            var rotationAxis = Quaternion.Inverse(_absRotation) * (_startingRotationAxis * (_hitIsBehind ? -1 : 1));
            var rotation = Quaternion.identity;

            if(handle.Axis == Vector3.right)
            {
                rotation = Quaternion.AngleAxis(delta.x, rotationAxis);
            }
            else if(handle.Axis == Vector3.up)
            {
                rotation = Quaternion.AngleAxis(delta.y, rotationAxis);
            }
            else if(handle.Axis == Vector3.forward)
            {
                rotation = Quaternion.AngleAxis(delta.z, rotationAxis);
            }

            _absRotation *= rotation;

            Target.rotation = Quaternion.Euler(_absRotation.eulerAngles.Snap(gb_Settings.Instance.RotationSnapSize));
        }

    }
}

