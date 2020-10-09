using Graybox.In;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox.Tools
{
    public class gb_VertexEditor : gb_Tool
    {

        public override string ToolName => "Vertex Editor";

        [SerializeField]
        private GameObject _handlePrefab;

        private GameObject _handleRoot;
        private bool _handlesExist;

        protected override void OnAwake()
        {
            _handleRoot = new GameObject("Vertex Editor Handles");
            _handleRoot.transform.SetParent(transform, true);
        }

        protected override void OnDisabled()
        {
            DestroyHandles();
        }

        protected override void OnTargetChanged(Transform target)
        {
            if (_handlesExist)
            {
                DestroyHandles();
            }

            if(target != null)
            {
                BuildHandles(target);
            }
        }

        protected override void OnBoxSelect(Rect dragRect, List<GameObject> hits)
        {
            // select all verts in box
        }

        protected override void OnDrag(Rect dragRect)
        {
        }

        private void BuildHandles(Transform target)
        {
            if (target.TryGetComponent(out MeshFilter mf))
            {
                var localToWorld = target.localToWorldMatrix;
                foreach(var vert in mf.mesh.vertices)
                {
                    var clone = GameObject.Instantiate(_handlePrefab);
                    clone.transform.localScale = Vector3.one * .1f;
                    clone.transform.position = localToWorld.MultiplyPoint3x4(vert);
                    clone.transform.SetParent(_handleRoot.transform, true);
                    RegisterHandle(clone.GetComponentInChildren<gb_VertexHandle>());
                }
                _handlesExist = true;
            }
        }

        private void DestroyHandles()
        {
            foreach(Transform tr in _handleRoot.transform)
            {
                GameObject.Destroy(tr.gameObject);
            }
            _handlesExist = false;
        }

    }
}

