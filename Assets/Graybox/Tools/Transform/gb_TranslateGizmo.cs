using Graybox.In;
using Graybox.Utility;
using UnityEngine;

namespace Graybox.Tools
{
    public class gb_TranslateGizmo : gb_GizmoTool
    {

        public override string ToolName => "Move";

        private Vector3 _absMovement;
        private Vector3 _startPosition;

        protected override void OnHandleDown(gb_GizmoHandles handle)
        {
            if (gb_Binds.IsDown(gb_Bind.DuplicateOnDrag))
            {
                gb_InputManager.Instance.DuplicateSelection();
            }

            _absMovement = Target.position;
            _startPosition = Target.position;
        }

        protected override void OnDeltaUpdate(gb_GizmoHandles handle, Vector2 screenDelta, Vector3 worldDelta)
        {
            var gridNormal = handle.Axis == Vector3.up ? Vector3.right : Vector3.up;
            gb_InputManager.ActiveSceneView.Draw.Add(new gb_Draw3dGrid()
            {
                Center = _startPosition.Snap(gb_Settings.Instance.SnapSize) + gridNormal * .01f,
                Normal = gridNormal
            });

            _absMovement += Vector3.Scale(handle.Axis, worldDelta);
            Target.position = _absMovement.Snap(handle.Axis, gb_Settings.Instance.SnapSize);
        }

    }
}

