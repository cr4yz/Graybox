using UnityEngine;

namespace Graybox.Tools
{
    public class gb_ScaleGizmo : gb_GizmoTool
    {

        public override string ToolName => "Scale";

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (gb_ToolManager.Instance.SelectedObject)
            {
                transform.rotation = gb_ToolManager.Instance.SelectedObject.transform.rotation;
            }
        }

        protected override void OnDeltaUpdate(gb_GizmoHandles handle, Vector2 screenDelta, Vector3 worldDelta)
        {
            worldDelta = transform.InverseTransformDirection(worldDelta);
            var dir = Vector3.Scale(handle.Axis, worldDelta);

            gb_ToolManager.Instance.SelectedObject.transform.localScale += dir.normalized * screenDelta.magnitude * Time.deltaTime;
        }

    }
}

