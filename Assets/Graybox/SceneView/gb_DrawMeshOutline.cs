using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Graybox
{
    public class gb_DrawMeshOutline : gb_Drawable
    {

        private gb_SceneView _sceneView;
        //private Transform _transform;
        //private List<Edge> _edges;
        //private gb_Mesh _mesh;
        private bool _drawCenterCross;
        private ProBuilderMesh _pbm;

        public gb_DrawMeshOutline(gb_SceneView sceneView, ProBuilderMesh mesh, bool drawCenterCross = true)
        {
            _pbm = mesh;
            //_mesh = mesh;
            _sceneView = sceneView;
            //_transform = mesh.GameObject.transform;
            //_edges = FindBoundary(_mesh.Normals, _mesh.Vertices, GetEdges(mesh));
            _drawCenterCross = drawCenterCross;
        }

        protected override int DrawMode => GL.LINES;
        protected override bool ScreenSpace => true;

        protected override void Draw()
        {
            if (!_pbm)
            {
                Debug.LogError("PBM Missing");
                return;
            }

            var verts = _pbm.VerticesInWorldSpace();
            foreach (var face in _pbm.faces)
            {
                foreach(var edge in face.edges)
                {
                    GL.Vertex(_sceneView.WorldToScreen(verts[edge.a]));
                    GL.Vertex(_sceneView.WorldToScreen(verts[edge.b]));
                }
            }
            if (_drawCenterCross)
            {
                var center = MeshToScreenPoint(_pbm.GetComponent<MeshFilter>().sharedMesh.bounds.center);
                gb_Draw2dLine.DrawLine(center + new Vector2(0, -16), center + new Vector2(0, 16), 1f);
                gb_Draw2dLine.DrawLine(center + new Vector2(-16, 0), center + new Vector2(16, 0), 1f);
            }
        }

        //Vector3 manualWorldToScreenPoint(Camera cam, Vector3 wp)
        //{
        //    // calculate view-projection matrix
        //    Matrix4x4 mat = cam.projectionMatrix * cam.worldToCameraMatrix;

        //    // multiply world point by VP matrix
        //    Vector4 temp = mat * new Vector4(wp.x, wp.y, wp.z, 1f);

        //    if (temp.w == 0f)
        //    {
        //        // point is exactly on camera focus point, screen point is undefined
        //        // unity handles this by returning 0,0,0
        //        return Vector3.zero;
        //    }
        //    else
        //    {
        //        // convert x and y from clip space to window coordinates
        //        temp.x = (temp.x / temp.w + 1f) * .5f * cam.pixelWidth;
        //        temp.y = (temp.y / temp.w + 1f) * .5f * cam.pixelHeight;
        //        return new Vector3(temp.x, temp.y, wp.z);
        //    }
        //}

        private Vector2 MeshToScreenPoint(Vector3 meshPoint)
        {
            var localToWorld = _pbm.transform.localToWorldMatrix;
            meshPoint = localToWorld.MultiplyPoint3x4(meshPoint);
            return _sceneView.Camera.WorldToScreenPoint(meshPoint);
        }

        //public struct Edge
        //{
        //    public int v1;
        //    public int v2;
        //    public int triangleIndex;
        //    public Edge(int aV1, int aV2, int aIndex)
        //    {
        //        v1 = aV1;
        //        v2 = aV2;
        //        triangleIndex = aIndex;
        //    }
        //}

        //public static List<Edge> GetEdges(gb_Mesh mesh)
        //{
        //    List<Edge> result = new List<Edge>();
        //    for (int i = 0; i < mesh.Triangles.Length; i += 3)
        //    {
        //        int v1 = mesh.Triangles[i];
        //        int v2 = mesh.Triangles[i + 1];
        //        int v3 = mesh.Triangles[i + 2];
        //        result.Add(new Edge(v1, v2, i));
        //        result.Add(new Edge(v2, v3, i));
        //        result.Add(new Edge(v3, v1, i));
        //    }
        //    return result;
        //}

        //public static List<Edge> FindBoundary(Vector3[] normals, Vector3[] verts, List<Edge> aEdges)
        //{
        //    List<Edge> result = new List<Edge>(aEdges);
        //    //foreach(var edge in aEdges)
        //    //{
        //    //    var dir = (verts[edge.v2] - verts[edge.v1]).normalized;
        //    //    var normal = Vector3.Cross(dir, edge.normal);
        //    //    var center = (verts[edge.v1] + verts[edge.v2]) / 2f;
        //    //    var plane = new Plane(normal, center);
        //    //    var neg = verts.Count(x => plane.GetSide(x) == false);
        //    //    if (neg == verts.Length)
        //    //    {
        //    //        result.Add(edge);
        //    //    }
        //    //}
        //    for (int i = result.Count - 1; i > 0; i--)
        //    {
        //        for (int n = i - 1; n >= 0; n--)
        //        {
        //            if (result[i].v1 == result[n].v2 && result[i].v2 == result[n].v1)
        //            {
        //                // shared edge so remove both
        //                result.RemoveAt(i);
        //                result.RemoveAt(n);
        //                i--;
        //                break;
        //            }
        //        }
        //    }
        //    return result;
        //}

        //public static List<Edge> SortEdges(List<Edge> aEdges)
        //{
        //    List<Edge> result = new List<Edge>(aEdges);
        //    for (int i = 0; i < result.Count - 2; i++)
        //    {
        //        Edge E = result[i];
        //        for (int n = i + 1; n < result.Count; n++)
        //        {
        //            Edge a = result[n];
        //            if (E.v2 == a.v1)
        //            {
        //                // in this case they are already in order so just continoue with the next one
        //                if (n == i + 1)
        //                    break;
        //                // if we found a match, swap them with the next one after "i"
        //                result[n] = result[i + 1];
        //                result[i + 1] = a;
        //                break;
        //            }
        //        }
        //    }
        //    return result;
        //}

    }
}

