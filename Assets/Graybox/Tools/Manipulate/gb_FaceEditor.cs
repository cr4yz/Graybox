using Graybox.In;
using Graybox.Utility;
using System.Collections.Generic;
using System.Linq;
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
        private Quaternion _previousDelta = Quaternion.identity;

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
            _previousDelta = rotation;

            var mtx = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
            var points = GetUniquePoints(_selectedFaces);
            var pbm = Target.GetComponent<ProBuilderMesh>();
            TransformPoints(pbm, points, mtx, AveragePointOf(pbm, points));

            ApplyMesh();
        }

        private void TransformPoints(ProBuilderMesh mesh, int[] points, Matrix4x4 mtx, Vector3 centerOfRotation)
        {
            var verts = mesh.GetVertices();
            var t = new HashSet<int>(points);
            for(int i = 0; i < points.Length; i++)
            {
                var newPosition = mtx.MultiplyPoint3x4(verts[points[i]].position - centerOfRotation) + centerOfRotation;
                var otherPoints = verts.Where((v, idx) => !t.Contains(idx) && v.position == verts[points[i]].position);
                foreach(var point in otherPoints)
                {
                    point.position = newPosition;
                }
                verts[points[i]].position = newPosition;
            }
            mesh.SetVertices(verts);
        }

        private Vector3 AveragePointOf(ProBuilderMesh mesh, IList<int> indexes)
        {
            var verts = mesh.GetVertices(indexes);
            var result = Vector3.zero;
            foreach(var vert in verts)
            {
                result += vert.position;
            }
            return result / verts.Length;
        }

        private int[] GetUniquePoints(IEnumerable<Face> faces)
        {
            var result = new List<int>();

            foreach(var face in faces)
            {
                result.AddRange(face.distinctIndexes);
            }

            return result.ToArray();
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
            var meshes = new ProBuilderMesh[] { Target.GetComponent<ProBuilderMesh>() };
            var options = new PickerOptions()
            {
                depthTest = false,
                rectSelectMode = RectSelectMode.Complete
            };
            dragRect = dragRect.SceneToGui(gb_InputManager.ActiveSceneView);
            var faceHits = SelectionPicker.PickFacesInRect(gb_InputManager.ActiveSceneView.Camera, dragRect, meshes, options);
            foreach(var hit in faceHits)
            {
                DoSelect(hit.Value);
            }
        }

        private Face _hoveredFace;

        protected override void OnUpdate()
        {
            if (!Target)
            {
                _hoveredFace = null;
                return;
            }

            var pbm = Target.GetComponent<ProBuilderMesh>();

            _hoveredFace = SelectionPicker.PickFace(gb_InputManager.ActiveSceneView.Camera, gb_InputManager.Instance.ScreenToScene(Input.mousePosition), pbm);

            if (gb_Binds.JustUp(gb_Bind.Select)
                && gb_InputManager.Instance.CanPick())
            {
                if(_hoveredFace != null)
                {
                    DoSelect(new Face[] { _hoveredFace });
                }
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Extrude(gb_Settings.Instance.ExtrusionSize);
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    OnRotate(_previousDelta);
                }
            }
        }

        private void Extrude(float distance)
        {
            var pbm2 = Target.GetComponent<ProBuilderMesh>();
            pbm2.Extrude(_selectedFaces, ExtrudeMethod.FaceNormal, distance);
            ApplyMesh();
            UpdatePivotObject();
        }

        private void ApplyMesh()
        {
            var pbm = Target.GetComponent<ProBuilderMesh>();
            pbm.ToMesh();
            pbm.Refresh();
            Target.GetComponent<Collider>().enabled = false;
            Target.GetComponent<Collider>().enabled = true;
            Target.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
            Target.GetComponent<gb_ObjectComponent>().Object.Save();
        }

        protected override void OnLateUpdate()
        {
            if (!Target)
            {
                return;
            }

            var pbm = Target.GetComponent<ProBuilderMesh>();
            var verts = pbm.VerticesInWorldSpace();

            foreach (var face in _selectedFaces)
            {
                DrawFace(verts, face, Color.green);
            }

            if(_hoveredFace != null && !_selectedFaces.Contains(_hoveredFace))
            {
                DrawFace(verts, _hoveredFace, Color.red);
            }
        }

        private void DrawFace(Vector3[] worldVerts, Face face, Color color)
        {
            foreach (var edge in face.edges)
            {
                var wsa = worldVerts[edge.a];
                var wsb = worldVerts[edge.b];
                var ssa = gb_InputManager.ActiveSceneView.WorldToScreen(wsa);
                var ssb = gb_InputManager.ActiveSceneView.WorldToScreen(wsb);
                gb_InputManager.ActiveSceneView.Draw.Draw2dLine(ssa, ssb, 5f, color);
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
