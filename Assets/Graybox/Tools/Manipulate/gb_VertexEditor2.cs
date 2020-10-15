using Graybox.In;
using Graybox.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Graybox.Tools
{
    public class gb_VertexEditor2 : gb_Tool
    {
        public override string ToolName => "Vertex Editor";
        //public override bool HasFocus => _hoveredVert != -1 ? true : base.HasFocus;

        private gb_TransformEvent _pivotObject;
        private gb_Tool _transformTool;
        private List<int> _selectedVerts = new List<int>();
        private int _hoveredVert = -1;

        protected override void OnAwake()
        {
            _pivotObject = new GameObject("Pivot Object").AddComponent<gb_TransformEvent>();
            _pivotObject.transform.SetParent(transform, true);
            _pivotObject.OnMove.AddListener(OnMove);
            _transformTool = GameObject.FindObjectOfType<gb_TranslateGizmo>(true);
        }

        protected override void OnDisabled()
        {
            _transformTool.gameObject.SetActive(false);
            _transformTool.TargetOverride = null;
            _selectedVerts.Clear();
            _hoveredVert = -1;
        }

        private void OnMove(Vector3 delta)
        {
            var pbm = Target.GetComponent<ProBuilderMesh>();
            foreach(var vert in _selectedVerts)
            {
                var newPos = GetSharedVertexPosition(vert, false) + delta;
                pbm.SetSharedVertexPosition(vert, newPos);
            }
            pbm.ToMesh();
            pbm.Refresh();
            pbm.GetComponent<gb_ObjectComponent>().Object.Save();
            pbm.GetComponent<Collider>().enabled = false;
            pbm.GetComponent<Collider>().enabled = true;
            pbm.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
            UpdatePivotObject();
        }

        protected override void OnBoxSelect(Rect dragRect, List<GameObject> hits)
        {
            if (!Target)
            {
                return;
            }
            DoSelect(GetSharedVerts(Target.GetComponent<ProBuilderMesh>(), dragRect.SceneToGui(gb_InputManager.ActiveSceneView)));
        }

        protected override void OnLateUpdate()
        {
            if (!Target)
            {
                return;
            }

            var scenePos = gb_InputManager.ActiveSceneView.ScreenToScene(Input.mousePosition);
            var min = scenePos - new Vector2(8, 8);
            var rect = new Rect(min, new Vector2(16, 16)).SceneToGui(gb_InputManager.ActiveSceneView);
            var hoveredVerts = GetSharedVerts(Target.GetComponent<ProBuilderMesh>(), rect);
            _hoveredVert = hoveredVerts.Count > 0 ? hoveredVerts[0] : -1;

            if (gb_Binds.JustUp(gb_Bind.Select) 
                && gb_InputManager.Instance.CanPick(this)
                && _hoveredVert != -1)
            {
                DoSelect(new int[] { _hoveredVert }, true);
            }

            var pbm = Target.GetComponent<ProBuilderMesh>();

            for (int i = 0; i < pbm.sharedVertices.Count; i++)
            {
                var color = _selectedVerts.Contains(i) 
                    ? gb_Settings.Instance.ElementSelectedColor
                    : gb_Settings.Instance.ElementColor;

                if(i == _hoveredVert)
                {
                    color = gb_Settings.Instance.ElementHoverColor;
                }

                var pos = gb_InputManager.ActiveSceneView.WorldToScreen(GetSharedVertexPosition(i));
                pos -= new Vector2(8, 8);
                var sz = new Vector2(16, 16);
                var ssRect = new Rect(pos, sz);
                gb_InputManager.ActiveSceneView.Draw.Draw2dQuad(ssRect, color);
            }
        }

        private Vector3 GetSharedVertexPosition(int vert, bool inWorldSpace = true)
        {
            var pbm = Target.GetComponent<ProBuilderMesh>();
            if (inWorldSpace)
            {
                var verts = pbm.VerticesInWorldSpace();
                return verts[pbm.sharedVertices[vert][0]];
            }
            else
            {
                var verts = pbm.GetVertices();
                return verts[pbm.sharedVertices[vert][0]].position;
            }
        }

        private List<int> GetSharedVerts(ProBuilderMesh pbm, Rect guiSpaceRect) 
        {
            var result = new List<int>();
            var meshes = new ProBuilderMesh[] { pbm };
            var options = new PickerOptions()
            {
                depthTest = false,
                rectSelectMode = RectSelectMode.Complete
            };
            var vertHits = SelectionPicker.PickVerticesInRect(gb_InputManager.ActiveSceneView.Camera, guiSpaceRect, meshes, options);
            foreach (var hit in vertHits)
            {
                result.AddRange(hit.Value);
            }
            return result;
        }

        private void DoSelect(IEnumerable<int> verts, bool selectOnlyFirst = false)
        {
            if(verts.Count() == 0)
            {
                _selectedVerts.Clear();
                UpdatePivotObject();
                return;
            }

            if (selectOnlyFirst)
            {
                verts = new List<int>() { verts.First() };
            }

            if (gb_Binds.IsDown(gb_Bind.Append))
            {
                foreach(var vert in verts)
                {
                    if (!_selectedVerts.Contains(vert))
                    {
                        _selectedVerts.Add(vert);
                    }
                }
            }
            else if (gb_Binds.IsDown(gb_Bind.Subtract))
            {
                foreach(var vert in verts)
                {
                    _selectedVerts.Remove(vert);
                }
            }
            else
            {
                _selectedVerts.Clear();
                _selectedVerts.AddRange(verts);
            }
            UpdatePivotObject();
        }

        private void UpdatePivotObject()
        {
            _pivotObject.transform.position = Vector3.zero;
            _pivotObject.transform.rotation = Quaternion.identity;
            _pivotObject.transform.localScale = Vector3.one;
            _transformTool.TargetOverride = _pivotObject.transform;

            if (_selectedVerts.Count == 0
                || !Target)
            {
                _transformTool.gameObject.SetActive(false);
                return;
            }

            _transformTool.gameObject.SetActive(true);

            var center = Vector3.zero;
            foreach(var sharedVert in _selectedVerts)
            {
                center += GetSharedVertexPosition(sharedVert);
            }
            center /= _selectedVerts.Count;
            _pivotObject.transform.position = center;
            _pivotObject.Sync();
        }

    }
}

