using Graybox.In;
using UnityEngine;

namespace Graybox.Tools
{
    public abstract class gb_GizmoTool : gb_Tool
    {
        [SerializeField]
        private GameObject _gizmoObject;

        private float _startDist;
        private Vector2 _handlePosition;
        private Vector3 _prevWorldPosition;
        private gb_GizmoHandles _activeHandle;

        protected override void OnAwake()
        {
            foreach (var handle in GetComponentsInChildren<gb_GizmoHandles>())
            {
                handle.OnHandleDown.AddListener(() => HandleDown(handle));
                handle.OnHandleUp.AddListener(() => HandleUp(handle));
                RegisterHandle(handle);
            }
        }

        protected override void OnDisabled()
        {
            _activeHandle = null;
        }

        protected override void OnUpdate()
        {
            if (!Target)
            {
                _gizmoObject.SetActive(false);
                return;
            }

            _gizmoObject.SetActive(true);

            float gizmoScale;

            if(gb_InputManager.ActiveSceneView.SceneAngle == gb_SceneViewAngle.ThreeDimensional)
            {
                var distToCamera = (gb_InputManager.ActiveSceneView.Camera.transform.position - transform.position).magnitude * .1f;
                gizmoScale = Mathf.Max(1, distToCamera);
            }
            else
            {
                gizmoScale = gb_InputManager.ActiveSceneView.Camera.orthographicSize / 2.25f;
            }

            transform.localScale = Vector3.one * gizmoScale;

            if (_activeHandle != null)
            {
                var pos = gb_InputManager.Instance.ScreenToWorld(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _startDist));
                var worldDelta = pos - _prevWorldPosition;
                _prevWorldPosition = pos;

                var newPosition = (Vector2)Input.mousePosition;
                OnDeltaUpdate(_activeHandle, newPosition - _handlePosition, worldDelta);
                _handlePosition = newPosition;
            }

            transform.position = Target.position;
        }

        private void HandleDown(gb_ToolHandle handle)
        {
            var gizmoHandle = handle as gb_GizmoHandles;

            _startDist = Vector3.Distance(Target.position, gb_InputManager.ActiveSceneView.Camera.transform.position);
            _activeHandle = gizmoHandle;
            _handlePosition = Input.mousePosition;
            _prevWorldPosition = gb_InputManager.Instance.ScreenToWorld(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _startDist));
            OnHandleDown(gizmoHandle);
        }

        private void HandleUp(gb_ToolHandle handle)
        {
            _activeHandle = null;
            OnHandleUp(handle as gb_GizmoHandles);
        }

        protected virtual void OnHandleDown(gb_GizmoHandles handle) { }
        protected virtual void OnHandleUp(gb_GizmoHandles handle) { }
        protected virtual void OnDeltaUpdate(gb_GizmoHandles handle, Vector2 screenDelta, Vector3 worldDelta) { }

    }
}

