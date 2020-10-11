using Graybox.In;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox.Tools
{
    public abstract class gb_Tool : MonoBehaviour
    {
        public virtual bool DontDrawDragRect { get; }
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
        public Transform Target
        {
            get
            {
                if (TargetOverride)
                {
                    return TargetOverride;
                }
                return gb_InputManager.ActiveObject != null ? gb_InputManager.ActiveObject.transform : null;
            }
        }
        [HideInInspector]
        public Transform TargetOverride;

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
            TargetOverride = null;

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

        private void LateUpdate()
        {
            OnLateUpdate();
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
        protected virtual void OnLateUpdate() { }
        protected virtual void OnDragEnd(Rect dragRect) { }
        protected virtual void OnDrag(Rect dragRect) { }
        protected virtual void OnBoxSelect(Rect dragRect, List<GameObject> hits) { }

    }
}

