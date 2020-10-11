using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graybox.Tools
{
    public class gb_VertexEditor : gb_Tool
    {

        public override string ToolName => "Vertex Editor";

        [SerializeField]
        private GameObject _handlePrefab;

        private GameObject _handleRoot;
        private GameObject _pivotObject;
        private bool _handlesExist;
        private gb_Tool _transformTool;

        private List<gb_VertexHandle> _selectedHandles = new List<gb_VertexHandle>();

        protected override void OnAwake()
        {
            _handleRoot = new GameObject("Vertex Editor Handles");
            _handleRoot.transform.SetParent(transform, true);
            _pivotObject = new GameObject("Vertex Pivot");
            _pivotObject.transform.SetParent(transform, true);
            _transformTool = GameObject.FindObjectOfType<gb_TranslateGizmo>(true);
        }

        protected override void OnDisabled()
        {
            _transformTool.gameObject.SetActive(false);
            DestroyHandles();
        }

        protected override void OnTargetChanged(Transform target)
        {
            _transformTool.gameObject.SetActive(false);

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
            var handles = new List<gb_VertexHandle>();
            foreach(var hit in hits) 
            {
                var vertHandle = hit.GetComponent<gb_VertexHandle>();
                if (vertHandle)
                {
                    handles.Add(vertHandle);
                }
            }
            if (gb_Binds.IsDown(gb_Bind.Subtract))
            {
                foreach(var handle in handles)
                {
                    _selectedHandles.Remove(handle);
                }
            }
            else if (gb_Binds.IsDown(gb_Bind.Append))
            {
                foreach(var handle in handles)
                {
                    if (!_selectedHandles.Contains(handle))
                    {
                        _selectedHandles.Add(handle);
                    }
                }
            }
            else
            {
                _selectedHandles.Clear();
                _selectedHandles.AddRange(handles);
            }
            UpdatePivotObject();
        }

        protected override void OnDrag(Rect dragRect)
        {
        }

        private void BuildHandles(Transform target)
        {
            if (target.TryGetComponent(out MeshFilter mf))
            {
                var localToWorld = target.localToWorldMatrix;
                foreach(var vert in mf.sharedMesh.vertices.Distinct())
                {
                    var clone = GameObject.Instantiate(_handlePrefab);
                    clone.transform.localScale = Vector3.one * .1f;
                    clone.transform.position = localToWorld.MultiplyPoint3x4(vert);
                    clone.transform.SetParent(_handleRoot.transform, true);
                    var vertHandle = clone.GetComponent<gb_VertexHandle>();
                    vertHandle.MeshFilter = mf;
                    vertHandle.VerticesToManipulate = mf.sharedMesh.vertices.Select((v, i) => new { v, i }).Where(x => x.v == vert).Select(x => x.i).ToArray();
                    RegisterHandle(vertHandle);
                    vertHandle.OnHandleDown.AddListener(() =>
                    {
                        if (gb_Binds.IsDown(gb_Bind.Append))
                        {
                            _selectedHandles.Add(vertHandle);
                        }
                        else if (gb_Binds.IsDown(gb_Bind.Subtract))
                        {
                            _selectedHandles.Remove(vertHandle);
                        }
                        else
                        {
                            _selectedHandles.Clear();
                            _selectedHandles.Add(vertHandle);
                        }
                        UpdatePivotObject();
                    });
                }
                _handlesExist = true;
            }
        }

        private void DestroyHandles()
        {
            _selectedHandles.Clear();

            foreach (Transform tr in _handleRoot.transform)
            {
                GameObject.Destroy(tr.gameObject);
            }

            foreach (Transform tr in _pivotObject.transform)
            {
                GameObject.Destroy(tr.gameObject);
            }

            _handlesExist = false;
        }

        private void UpdatePivotObject()
        {
            var children = _pivotObject.GetComponentsInChildren<Transform>();
            foreach(Transform tr in children)
            {
                if(tr == _pivotObject.transform)
                {
                    continue;
                }
                tr.SetParent(_handleRoot.transform, true);
            }

            if(_selectedHandles.Count > 0)
            {
                var pivotPoint = Vector3.zero;
                foreach (var handle in _selectedHandles)
                {
                    pivotPoint += handle.transform.position;
                }
                pivotPoint /= _selectedHandles.Count;
                _pivotObject.transform.position = pivotPoint;
                _pivotObject.transform.localScale = Vector3.one;
                _pivotObject.transform.rotation = Quaternion.identity;

                foreach (var handle in _selectedHandles)
                {
                    handle.transform.SetParent(_pivotObject.transform, true);
                }

                _transformTool.gameObject.SetActive(true);
                _transformTool.TargetOverride = _pivotObject.transform;
            }
            else
            {
                _transformTool.gameObject.SetActive(false);
            }
        }

    }
}

