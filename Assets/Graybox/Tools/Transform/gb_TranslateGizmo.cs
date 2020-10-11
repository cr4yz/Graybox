using Graybox.In;
using Graybox.Utility;
using UnityEngine;

namespace Graybox.Tools
{
    public class gb_TranslateGizmo : gb_GizmoTool
    {

        public override string ToolName => "Move";

        private Vector3 _absMovement;

        protected override void OnHandleDown(gb_GizmoHandles handle)
        {
            _absMovement = Target.position;
        }

        protected override void OnDeltaUpdate(gb_GizmoHandles handle, Vector2 screenDelta, Vector3 worldDelta)
        {
            _absMovement += Vector3.Scale(handle.Axis, worldDelta);
            Target.position = _absMovement.Snap(handle.Axis, gb_Settings.Instance.SnapSize);
        }

    }
}

