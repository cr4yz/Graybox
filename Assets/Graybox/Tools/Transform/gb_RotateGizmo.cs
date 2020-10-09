using Graybox.In;
using UnityEngine;

namespace Graybox.Tools
{
    public class gb_RotateGizmo : gb_GizmoTool
    {

        public override string ToolName => "Rotate";

        private bool _hitIsBehind;
        private Quaternion m_startingRotation;
        private Vector3 m_startingRotationAxis;

        protected override void OnHandleDown(gb_GizmoHandles handle)
        {
            var hitDist = Vector3.Distance(handle.HitPoint, gb_InputManager.ActiveSceneView.Camera.transform.position);
            var gizmoDist = Vector3.Distance(handle.transform.position, gb_InputManager.ActiveSceneView.Camera.transform.position);
            _hitIsBehind = hitDist > gizmoDist + Mathf.Epsilon;

            m_startingRotation = Target.rotation;
            m_startingRotationAxis = (Target.rotation * Quaternion.Inverse(m_startingRotation)) * handle.Axis;
        }

        protected override void OnDeltaUpdate(gb_GizmoHandles handle, Vector2 screenDelta, Vector3 worldDelta)
        {
            var delta = m_startingRotation * Quaternion.Inverse(Target.rotation) * Camera.cameraToWorldMatrix.MultiplyVector(new Vector3(screenDelta.y, -screenDelta.x, 0));
            var rotationAxis = Quaternion.Inverse(Target.rotation) * (m_startingRotationAxis * (_hitIsBehind ? -1 : 1));
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

            Target.rotation *= rotation;
        }

    }
}

