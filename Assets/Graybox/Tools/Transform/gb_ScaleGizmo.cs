using UnityEngine;

namespace Graybox.Tools
{
    public class gb_ScaleGizmo : gb_GizmoTool
    {

        public override string ToolName => "Scale";

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (Target)
            {
                transform.rotation = Target.rotation;
            }
        }

        protected override void OnDeltaUpdate(gb_GizmoHandles handle, Vector2 screenDelta, Vector3 worldDelta)
        {
            worldDelta = transform.InverseTransformDirection(worldDelta);
            var dir = Vector3.Scale(handle.Axis, worldDelta);

            Target.localScale += dir.normalized * screenDelta.magnitude * Time.deltaTime;
        }

    }
}

