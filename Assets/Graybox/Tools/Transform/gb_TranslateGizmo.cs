using Graybox.In;
using UnityEngine;

namespace Graybox.Tools
{
    public class gb_TranslateGizmo : gb_GizmoTool
    {

        public override string ToolName => "Move";

        protected override void OnDeltaUpdate(gb_GizmoHandles handle, Vector2 screenDelta, Vector3 worldDelta)
        {
            gb_ToolManager.Instance.SelectedObject.transform.position += Vector3.Scale(handle.Axis, worldDelta);
        }

    }
}

