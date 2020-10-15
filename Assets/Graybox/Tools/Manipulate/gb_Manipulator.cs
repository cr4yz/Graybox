using Graybox.In;
using Graybox.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Graybox.Tools
{
    public enum gb_ManipulatorMode
    {
        None,
        Vertices,
        Faces,
        Edges
    }

    public enum gb_ManipulatorHandle
    {
        None,
        Move,
        Rotate,
        Scale
    }

    public class gb_Manipulator : gb_Tool
    {

        public override string ToolName => "Manipulator";
        public ProBuilderMesh Pbm => Target != null ? Target.GetComponent<ProBuilderMesh>() : null;
        public gb_ManipulatorMode Mode
        {
            get { return _mode; }
            set { SetManipulatorMode(value); }
        }
        public gb_ManipulatorHandle Handle
        {
            get { return _handle; }
            set { SetManipulatorHandle(value); }
        }

        private gb_ManipulatorMode _mode;
        private gb_ManipulatorHandle _handle;
        private gb_TransformEvent _pivotObject;
        private gb_Tool _transformTool;
        private int _hoveredVertice;
        private Face _hoveredFace;
        private Edge _hoveredEdge;
        private Vector3[] _worldSpaceVertCache;
        private Vertex[] _vertCache;

        protected override void OnAwake()
        {
            _pivotObject = new GameObject("Pivot Object").AddComponent<gb_TransformEvent>();
            _pivotObject.transform.SetParent(transform, true);
            _pivotObject.OnMove.AddListener(OnMove);
            _pivotObject.OnRotate.AddListener(OnRotate);
            _pivotObject.OnScale.AddListener(OnScale);

            Handle = gb_ManipulatorHandle.Move;
            Mode = gb_ManipulatorMode.Vertices;
        }

        protected override void OnEnabled()
        {
            UpdatePivotObject();
            if (Pbm)
            {
                Cache();
            }
        }

        protected override void OnDisabled()
        {
            _hoveredVertice = -1;
            _hoveredFace = null;
            _hoveredEdge = default;
            _transformTool.gameObject.SetActive(false);
        }

        private List<int> VertsToManipulate(bool distinct = false)
        {
            var result = new List<int>();
            switch (Mode)
            {
                case gb_ManipulatorMode.Vertices:
                    foreach(var sharedVert in Pbm.selectedVertices)
                    {
                        result.AddRange(SharedToIndices(sharedVert));
                    }
                    break;
                case gb_ManipulatorMode.Faces:
                    foreach(var face in Pbm.GetSelectedFaces())
                    {
                        if (distinct)
                        {
                            result.AddRange(face.distinctIndexes);
                        }
                        else
                        {
                            result.AddRange(face.indexes);
                        }
                    }
                    break;
                case gb_ManipulatorMode.Edges:
                    foreach(var edge in Pbm.selectedEdges)
                    {
                        result.Add(edge.a);
                        result.Add(edge.b);
                    }
                    break;
            }
            return result.Distinct().ToList();
        }

        private List<int> SharedToIndices(params int[] sharedVerts)
        {
            var result = new List<int>();

            foreach(var sharedIdx in sharedVerts)
            {
                result.AddRange(Pbm.sharedVertices[sharedIdx]);
            }

            return result;
        }

        private void OnMove(Vector3 delta) 
        {
            var mtx = Matrix4x4.TRS(delta, Quaternion.identity, Vector3.one);
            var points = VertsToManipulate(true).ToList();
            TransformPoints(points.ToArray(), mtx, AveragePointOf(points));
            ApplyMesh();
            UpdatePivotObject();
        }

        private void OnRotate(Quaternion delta) 
        {
            var mtx = Matrix4x4.TRS(Vector3.zero, delta, Vector3.one);
            var points = VertsToManipulate(true);
            TransformPoints(points.ToArray(), mtx, AveragePointOf(points));
            ApplyMesh();
        }

        private void OnScale(Vector3 delta) 
        {
            var mtx = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one + delta);
            var points = VertsToManipulate(true);
            TransformPoints(points.ToArray(), mtx, AveragePointOf(points));
            ApplyMesh();
        }

        private void TransformPoints(int[] points, Matrix4x4 mtx, Vector3 centerOfTransformation)
        {
            var verts = Pbm.GetVertices();
            var t = new HashSet<int>(points);
            for (int i = 0; i < points.Length; i++)
            {
                var newPosition = mtx.MultiplyPoint3x4(verts[points[i]].position - centerOfTransformation) + centerOfTransformation;
                var otherPoints = verts.Where((v, idx) => !t.Contains(idx) && v.position == verts[points[i]].position);
                foreach (var point in otherPoints)
                {
                    point.position = newPosition;
                }
                verts[points[i]].position = newPosition;
            }
            Pbm.SetVertices(verts);
        }

        private Vector3 AveragePointOf(IList<int> indexes)
        {
            var result = Vector3.zero;
            foreach(var idx in indexes)
            {
                result += _vertCache[idx].position;
            }
            return result / indexes.Count;
        }

        protected void ApplyMesh()
        {
            Pbm.ToMesh();
            Pbm.Refresh();
            Pbm.GetComponent<Collider>().enabled = false;
            Pbm.GetComponent<Collider>().enabled = true;
            Pbm.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
            Pbm.GetComponent<gb_ObjectComponent>().Object.Save();

            Cache();
        }

        private void Cache()
        {
            _worldSpaceVertCache = Pbm.VerticesInWorldSpace();
            _vertCache = Pbm.GetVertices();
        }

        protected override void OnBoxSelect(Rect dragRect, List<GameObject> hits)
        {
            if (!Pbm)
            {
                return;
            }

            var guiSpaceRect = dragRect.SceneToGui(gb_InputManager.ActiveSceneView);

            switch (Mode) 
            {
                case gb_ManipulatorMode.None:
                    return;
                case gb_ManipulatorMode.Vertices:
                    DoSelect(BoxSelectSharedVerts(guiSpaceRect));
                    break;
                case gb_ManipulatorMode.Faces:
                    DoSelect(BoxSelectFaces(guiSpaceRect));
                    break;
                case gb_ManipulatorMode.Edges:
                    DoSelect(BoxSelectEdges(guiSpaceRect));
                    break;
            }

            UpdatePivotObject();
        }

        private void DoSelect(List<int> verts)
        {
            var selectMode = GetSelectMode();
            switch (selectMode)
            {
                case SelectMode.Normal:
                    Pbm.SetSelectedVertices(verts);
                    break;
                case SelectMode.Append:
                    Pbm.SetSelectedVertices(Pbm.selectedVertices.Union(verts));
                    break;
                case SelectMode.Subtract:
                    Pbm.SetSelectedVertices(Pbm.selectedVertices.Except(verts));
                    break;
            }
        }

        private void DoSelect(List<Face> faces)
        {
            var selectMode = GetSelectMode();
            switch (selectMode)
            {
                case SelectMode.Normal:
                    Pbm.SetSelectedFaces(faces);
                    break;
                case SelectMode.Append:
                    Pbm.SetSelectedFaces(Pbm.GetSelectedFaces().Union(faces).Distinct());
                    break;
                case SelectMode.Subtract:
                    Pbm.SetSelectedFaces(Pbm.GetSelectedFaces().Except(faces));
                    break;
            }
        }

        private void DoSelect(List<Edge> edges)
        {
            var selectMode = GetSelectMode();
            switch (selectMode)
            {
                case SelectMode.Normal:
                    Pbm.SetSelectedEdges(edges);
                    break;
                case SelectMode.Append:
                    Pbm.SetSelectedEdges(Pbm.selectedEdges.Union(edges).Distinct());
                    break;
                case SelectMode.Subtract:
                    Pbm.SetSelectedEdges(Pbm.selectedEdges.Except(edges));
                    break;
            }
        }

        private List<int> BoxSelectSharedVerts(Rect guiSpaceRect)
        {
            var result = new List<int>();
            var meshes = new ProBuilderMesh[] { Pbm };
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

        private List<Face> BoxSelectFaces(Rect guiSpaceRect)
        {
            var result = new List<Face>();
            var meshes = new ProBuilderMesh[] { Pbm };
            var options = new PickerOptions()
            {
                depthTest = false,
                rectSelectMode = RectSelectMode.Complete
            };
            var hits = SelectionPicker.PickFacesInRect(gb_InputManager.ActiveSceneView.Camera, guiSpaceRect, meshes, options);
            foreach(var hit in hits)
            {
                result.AddRange(hit.Value);
            }
            return result;
        }

        private List<Edge> BoxSelectEdges(Rect guiSpaceRect)
        {
            var result = new List<Edge>();
            var meshes = new ProBuilderMesh[] { Pbm };
            var options = new PickerOptions()
            {
                depthTest = false,
                rectSelectMode = RectSelectMode.Complete
            };
            var hits = SelectionPicker.PickEdgesInRect(gb_InputManager.ActiveSceneView.Camera, guiSpaceRect, meshes, options);
            foreach (var hit in hits)
            {
                result.AddRange(hit.Value);
            }
            return result;
        }

        protected override void OnTargetChanged(Transform target)
        {
            _transformTool.gameObject.SetActive(false);
            _pivotObject.gameObject.SetActive(false);

            if (target != null)
            {
                Cache();
                UpdatePivotObject();
            }
        }

        protected override void OnLateUpdate()
        {
            if (!Pbm)
            {
                return;
            }

            switch (Mode) 
            {
                case gb_ManipulatorMode.Vertices:
                    DrawSelectionVertices();
                    break;
                case gb_ManipulatorMode.Edges:
                    DrawSelectionEdges();
                    break;
                case gb_ManipulatorMode.Faces:
                    DrawSelectionFaces();
                    break;
            }
        }

        protected void UpdatePivotObject()
        {
            if (!Pbm)
            {
                _transformTool.gameObject.SetActive(false);
                _pivotObject.gameObject.SetActive(false);
                return;
            }

            var transformToolOn = false;

            switch (Mode)
            {
                case gb_ManipulatorMode.None:
                    transformToolOn = false;
                    break;
                case gb_ManipulatorMode.Vertices:
                    transformToolOn = Pbm.selectedVertexCount > 0;
                    break;
                case gb_ManipulatorMode.Faces:
                    transformToolOn = Pbm.selectedFaceCount > 0;
                    break;
                case gb_ManipulatorMode.Edges:
                    transformToolOn = Pbm.selectedEdgeCount > 0;
                    break;
            }

            _transformTool.gameObject.SetActive(transformToolOn);
            _pivotObject.gameObject.SetActive(transformToolOn);
            _transformTool.TargetOverride = _pivotObject.transform;
            _pivotObject.transform.localScale = Vector3.one;
            _pivotObject.transform.position = GetSelectionPivot();
            _pivotObject.transform.rotation = Quaternion.identity;
            _pivotObject.Sync();
        }

        private Vector3 GetSelectionPivot()
        {
            if (!Pbm)
            {
                return Vector3.zero;
            }

            var result = Vector3.zero;

            switch (Mode)
            {
                case gb_ManipulatorMode.Vertices:
                    if(Pbm.selectedVertexCount == 0)
                    {
                        return result;
                    }
                    foreach(var vert in Pbm.selectedVertices)
                    {
                        result += GetSharedVertexPosition(vert);
                    }
                    result /= Pbm.selectedVertexCount;
                    break;
                case gb_ManipulatorMode.Faces:
                    if(Pbm.selectedFaceCount == 0)
                    {
                        return result;
                    }
                    var faceIndexes = new List<int>();
                    foreach (var face in Pbm.GetSelectedFaces())
                    {
                        faceIndexes.AddRange(face.distinctIndexes);
                    }
                    foreach (var vert in faceIndexes)
                    {
                        result += _worldSpaceVertCache[vert];
                    }
                    result /= faceIndexes.Count;
                    break;
                case gb_ManipulatorMode.Edges:
                    if(Pbm.selectedEdgeCount == 0)
                    {
                        return result;
                    }
                    foreach(var edge in Pbm.selectedEdges)
                    {
                        result += _worldSpaceVertCache[edge.a];
                        result += _worldSpaceVertCache[edge.b];
                    }
                    result /= Pbm.selectedEdges.Count * 2;
                    break;
            }

            return result;
        }

        private void SetManipulatorMode(gb_ManipulatorMode mode)
        {
            _mode = mode;
            _hoveredFace = null;
            _hoveredVertice = -1;

            if (Pbm)
            {
                Pbm.ClearSelection();
            }

            switch (mode)
            {
                case gb_ManipulatorMode.Vertices:
                    break;
            }

            UpdatePivotObject();
        }

        private void SetManipulatorHandle(gb_ManipulatorHandle handle)
        {
            if (_transformTool)
            {
                _transformTool.gameObject.SetActive(false);
            }

            switch (handle)
            {
                case gb_ManipulatorHandle.Move:
                    _transformTool = GameObject.FindObjectOfType<gb_TranslateGizmo>(true);
                    break;
                case gb_ManipulatorHandle.Rotate:
                    _transformTool = GameObject.FindObjectOfType<gb_RotateGizmo>(true);
                    break;
                case gb_ManipulatorHandle.Scale:
                    _transformTool = GameObject.FindObjectOfType<gb_ScaleGizmo>(true);
                    break;
            }

            _transformTool.gameObject.SetActive(true);
            UpdatePivotObject();
        }

        private void DrawSelectionVertices()
        {
            for (int i = 0; i < Pbm.sharedVertices.Count; i++)
            {
                var color = Pbm.selectedVertices.Contains(i)
                    ? gb_Settings.Instance.ElementSelectedColor
                    : gb_Settings.Instance.ElementColor;

                if (i == _hoveredVertice)
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

        private void DrawSelectionFaces()
        {
            foreach (var idx in Pbm.selectedFaceIndexes)
            {
                DrawEdges(Pbm.faces[idx].edges, gb_Settings.Instance.ElementSelectedColor);
            }

            if (_hoveredFace != null)
            {
                DrawEdges(_hoveredFace.edges, gb_Settings.Instance.ElementHoverColor);
            }
        }

        private void DrawSelectionEdges()
        {
            DrawEdges(Pbm.selectedEdges, Color.green);
        }

        private void DrawEdges(IEnumerable<Edge> edges, Color color)
        {
            foreach(var edge in edges)
            {
                var wsa = _worldSpaceVertCache[edge.a];
                var wsb = _worldSpaceVertCache[edge.b];
                var ssa = gb_InputManager.ActiveSceneView.WorldToScreen(wsa);
                var ssb = gb_InputManager.ActiveSceneView.WorldToScreen(wsb);
                gb_InputManager.ActiveSceneView.Draw.Draw2dLine(ssa, ssb, 5f, color);
            }
        }

        private Vector3 GetSharedVertexPosition(int vert, bool inWorldSpace = true)
        {
            return inWorldSpace
                ? _worldSpaceVertCache[Pbm.sharedVertices[vert][0]]
                : _vertCache[Pbm.sharedVertices[vert][0]].position;
        }

        private enum SelectMode
        {
            Normal,
            Append,
            Subtract
        }

        private SelectMode GetSelectMode()
        {
            if (gb_Binds.IsDown(gb_Bind.Append))
            {
                return SelectMode.Append;
            }
            else if (gb_Binds.IsDown(gb_Bind.Subtract))
            {
                return SelectMode.Subtract;
            }
            return SelectMode.Normal;
        }

    }
}

