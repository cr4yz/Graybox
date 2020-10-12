using Graybox.In;
using Graybox.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace Graybox.Tools 
{
    public class gb_FaceEditor : gb_Tool
    {
        public override string ToolName => "Face Editor";

        private List<Face> _selectedFaces = new List<Face>();
        private gb_TransformEvent _pivotObject;
        private gb_Tool _transformTool;

        protected override void OnAwake()
        {
            _pivotObject = new GameObject("Pivot Object").AddComponent<gb_TransformEvent>();
            _pivotObject.transform.SetParent(transform, true);
            _pivotObject.OnMove.AddListener(OnMove);
            _pivotObject.OnRotate.AddListener(OnRotate);
            _transformTool = GameObject.FindObjectOfType<gb_RotateGizmo>(true);
        }

        protected override void OnDisabled()
        {
            _transformTool.gameObject.SetActive(false);
            _selectedFaces.Clear();
        }

        private void OnMove(Vector3 delta)
        {
            var pbm = Target.GetComponent<ProBuilderMesh>();
            pbm.TranslateVertices(_selectedFaces, delta);
            ApplyMesh();
        }

        private void OnRotate(Quaternion rotation)
        {
            //vertices[n] = rotation * (vertices[n] - centerPoint) + centerPoint;
            var pbm = Target.GetComponent<ProBuilderMesh>();
            var verts = pbm.GetVertices();
            foreach (var face in _selectedFaces)
            {
                var center = Vector3.zero;
                foreach (var idx in face.distinctIndexes)
                {
                    center += verts[idx].position;
                }
                center /= face.distinctIndexes.Count;
                foreach (var idx in face.distinctIndexes)
                {
                    var pos = verts[idx].position;
                    for(int j = 0; j < verts.Length; j++)
                    {
                        if(verts[j].position == pos)
                        {
                            verts[j].position = rotation * (verts[j].position - center) + center;
                        }
                    }
                }
            }
            pbm.SetVertices(verts, true);
            ApplyMesh();
        }

        protected override void OnTargetChanged(Transform target)
        {
            _pivotObject.Sync();
            _transformTool.gameObject.SetActive(false);
            _selectedFaces.Clear();
        }

        protected override void OnBoxSelect(Rect dragRect, List<GameObject> hits)
        {
            if (!Target)
            {
                return;
            }
            var meshes = GameObject.FindObjectsOfType<ProBuilderMesh>();
            var options = new PickerOptions()
            {
                depthTest = false,
                rectSelectMode = RectSelectMode.Complete
            };
            var ph = gb_InputManager.ActiveSceneView.Camera.pixelHeight;
            dragRect.min = new Vector2(dragRect.min.x, ph - dragRect.min.y);
            dragRect.max = new Vector2(dragRect.max.x, ph - dragRect.max.y);
            var faceHits = SelectionPicker.PickFacesInRect(gb_InputManager.ActiveSceneView.Camera, dragRect, meshes, options);
            foreach(var hit in faceHits)
            {
                DoSelect(hit.Value);
            }
        }

        protected override void OnUpdate()
        {
            if (!Target)
            {
                return;
            }

            if (gb_Binds.JustUp(gb_Bind.Select)
                && !gb_ToolManager.Instance.ToolHasFocus())
            {
                RaycastHit targetHit = gb_InputManager.HitsUnderCursor.FirstOrDefault(x => x.transform == Target);
                if (targetHit.transform == null)
                {
                    return;
                }
                var pbm = targetHit.transform.GetComponent<ProBuilderMesh>();
                var face = SelectionPicker.PickFace(gb_InputManager.ActiveSceneView.Camera, gb_InputManager.Instance.ScreenToScene(Input.mousePosition), pbm);

                if(face != null)
                {
                    DoSelect(new Face[] { face });
                }
            }

            if(Input.GetKeyDown(KeyCode.E) 
                && Input.GetKey(KeyCode.LeftControl))
            {
                var pbm2 = Target.GetComponent<ProBuilderMesh>();
                pbm2.Extrude(_selectedFaces, ExtrudeMethod.FaceNormal, 1f);
                ApplyMesh();
            }
        }

        private void ApplyMesh()
        {
            var pbm = Target.GetComponent<ProBuilderMesh>();
            pbm.ToMesh();
            pbm.Refresh();
            Target.GetComponent<Collider>().enabled = false;
            Target.GetComponent<Collider>().enabled = true;
            Target.GetComponent<gb_ObjectComponent>().Object.Save();
            UpdatePivotObject();
        }

        protected override void OnLateUpdate()
        {
            if (Target)
            {
                var pbm = Target.GetComponent<ProBuilderMesh>();
                var verts = pbm.VerticesInWorldSpace();

                foreach (var face in _selectedFaces)
                {
                    foreach (var edge in face.edges)
                    {
                        var wsa = verts[edge.a];
                        var wsb = verts[edge.b];
                        var ssa = gb_InputManager.ActiveSceneView.WorldToScreen(wsa);
                        var ssb = gb_InputManager.ActiveSceneView.WorldToScreen(wsb);
                        gb_InputManager.ActiveSceneView.Draw.Draw2dLine(ssa, ssb, 5f, Color.red);
                    }
                }
            }
        }

        private void DoSelect(IEnumerable<Face> faces)
        {
            if (gb_Binds.IsDown(gb_Bind.Append))
            {
                foreach(var face in faces)
                {
                    if (!_selectedFaces.Contains(face))
                    {
                        _selectedFaces.Add(face);
                    }
                }
            }
            else if (gb_Binds.IsDown(gb_Bind.Subtract))
            {
                foreach(var face in faces)
                {
                    _selectedFaces.Remove(face);
                }
            }
            else
            {
                _selectedFaces.Clear();
                _selectedFaces.AddRange(faces);
            }
            UpdatePivotObject();
        }

        private void UpdatePivotObject()
        {
            _pivotObject.transform.position = Vector3.zero;
            _pivotObject.transform.localScale = Vector3.one;
            _pivotObject.transform.rotation = Quaternion.identity;
            if(_selectedFaces.Count == 0)
            {
                _transformTool.gameObject.SetActive(false);
            }
            else
            {
                var pbm = Target.GetComponent<ProBuilderMesh>();
                var indexes = new List<int>();
                _transformTool.gameObject.SetActive(true);
                _transformTool.TargetOverride = _pivotObject.transform;
                foreach(var face in _selectedFaces)
                {
                    indexes.AddRange(face.distinctIndexes);
                }
                var verts = pbm.GetVertices(indexes);
                var center = Vector3.zero;
                foreach(var vert in verts)
                {
                    center += vert.position;
                }
                center /= verts.Length;
                center = Target.localToWorldMatrix.MultiplyPoint3x4(center);
                _pivotObject.transform.position = center;
                _pivotObject.Sync();
            }
        }

    }
}
