using Graybox.Utility;
using UnityEngine;

namespace Graybox.Tools
{
    public class gb_ScaleGizmo : gb_GizmoTool
    {

        public override string ToolName => "Scale";

        private Vector3 _absScale;

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (Target)
            {
                transform.rotation = Target.rotation;
            }
        }

        protected override void OnHandleDown(gb_GizmoHandles handle)
        {
            base.OnHandleDown(handle);

            _absScale = Target.localScale;
        }

        protected override void OnDeltaUpdate(gb_GizmoHandles handle, Vector2 screenDelta, Vector3 worldDelta)
        {
            _absScale += Vector3.Scale(handle.Axis, worldDelta);
            //Target.localScale = _absScale;
            Target.localScale = _absScale.Snap(handle.Axis, gb_Settings.Instance.ScaleSnapSize);
        }

    }
}

