using Graybox.In;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox.Tools
{
    public abstract class gb_Tool : MonoBehaviour
    {
        public abstract string ToolName { get; }
        public bool HasFocus
        {
            get
            {
                foreach(var handle in _handles)
                {
                    if(handle && handle.IsMouseDown)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public Camera Camera => gb_InputManager.ActiveSceneView.Camera;
        public Transform Target => gb_ToolManager.Instance.SelectedObject != null 
            ? gb_ToolManager.Instance.SelectedObject.transform 
            : null;

        private List<gb_ToolHandle> _handles = new List<gb_ToolHandle>();
        private Transform _lastTarget;

        private void Awake()
        {
            OnAwake();
        }

        private void Start()
        {
            OnStart();   
        }

        private void OnDestroy()
        {
            OnDestroyed();
        }

        private void OnEnable()
        {
            gb_InputManager.OnDrag.AddListener(OnDrag);
            gb_InputManager.OnDragEnd.AddListener(OnDragEnd);
            gb_InputManager.OnBoxSelect.AddListener(OnBoxSelect);

            OnEnabled();
        }

        private void OnDisable()
        {
            gb_InputManager.OnDrag.RemoveListener(OnDrag);
            gb_InputManager.OnDragEnd.RemoveListener(OnDragEnd);
            gb_InputManager.OnBoxSelect.RemoveListener(OnBoxSelect);

            OnDisabled();
        }

        private void Update()
        {
            if(_lastTarget != Target)
            {
                _lastTarget = Target;
                OnTargetChanged(Target);
            }

            for (int i = _handles.Count - 1; i >= 0; i--)
            {
                if (!_handles[i])
                {
                    _handles.RemoveAt(i);
                }
            }

            OnUpdate();
        }

        protected virtual void OnTargetChanged(Transform target) { }
        protected void RegisterHandle(gb_ToolHandle handle) 
        {
            _handles.Add(handle);
        }

        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }
        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
        protected virtual void OnDestroyed() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnDragEnd(Rect dragRect) { }
        protected virtual void OnDrag(Rect dragRect) { }
        protected virtual void OnBoxSelect(Rect dragRect, List<GameObject> hits) { }

    }
}

