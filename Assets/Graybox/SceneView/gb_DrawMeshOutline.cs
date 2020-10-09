using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graybox
{
    public class gb_DrawMeshOutline : gb_Drawable
    {

        private gb_SceneView _sceneView;
        private Transform _transform;
        private Edge[] _edges;

        public gb_DrawMeshOutline(gb_SceneView sceneView, gb_Mesh mesh)
        {
            _sceneView = sceneView;
            _transform = mesh.GameObject.transform;
            _edges = GetMeshEdges(mesh, true);
        }

        protected override int DrawMode => GL.LINES;
        protected override bool ScreenSpace => true;

        protected override void Draw()
        {
            foreach(var edge in _edges)
            {
                GL.Vertex(WorldToScreenPoint(edge.v1));
                GL.Vertex(WorldToScreenPoint(edge.v2));
            }
        }

        private Vector2 WorldToScreenPoint(Vector3 worldPoint)
        {
            var localToWorld = _transform.localToWorldMatrix;
            worldPoint = localToWorld.MultiplyPoint3x4(worldPoint);
            return _sceneView.Camera.WorldToScreenPoint(worldPoint);
        }

        private Edge[] GetMeshEdges(gb_Mesh mesh, bool omitHypotenuse = true)
        {
            var edges = new HashSet<Edge>();

            for (int i = 0; i < mesh.Triangles.Length; i += 3)
            {
                var v1 = mesh.Vertices[mesh.Triangles[i]];
                var v2 = mesh.Vertices[mesh.Triangles[i + 1]];
                var v3 = mesh.Vertices[mesh.Triangles[i + 2]];
                var e1 = new Edge(v1, v2);
                var e2 = new Edge(v1, v3);
                var e3 = new Edge(v2, v3);

                if (omitHypotenuse)
                {
                    if (e1.Length > e2.Length && e1.Length > e3.Length)
                    {
                        edges.Add(e2);
                        edges.Add(e3);
                    }
                    else if (e2.Length > e1.Length && e2.Length > e3.Length)
                    {
                        edges.Add(e1);
                        edges.Add(e3);
                    }
                    else
                    {
                        edges.Add(e1);
                        edges.Add(e2);
                    }
                }
                else
                {
                    edges.Add(e1);
                    edges.Add(e2);
                    edges.Add(e3);
                }
            }

            return edges.ToArray();
        }

        public struct Edge
        {
            public Vector3 v1;
            public Vector3 v2;

            public float Length => Vector3.Distance(v2, v1);

            public Edge(Vector3 v1, Vector3 v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }
        }

    }
}

