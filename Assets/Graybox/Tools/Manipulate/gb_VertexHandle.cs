using Graybox.In;
using UnityEngine;

namespace Graybox.Tools
{
    public class gb_VertexHandle : gb_ToolHandle
    {

        public MeshFilter MeshFilter;
        public int[] VerticesToManipulate;

        private Vector3 _initialScale;
        private Vector3 _position;

        private void Start()
        {
            _initialScale = transform.localScale;
            _position = transform.position;
        }

        protected override void Update()
        {
            base.Update();

            var handlesScale = 1f;

            if (gb_InputManager.ActiveSceneView.SceneAngle == gb_SceneViewAngle.ThreeDimensional)
            {
                var distToCamera = (gb_InputManager.ActiveSceneView.Camera.transform.position - transform.position).magnitude * .1f;
                handlesScale = Mathf.Max(1, distToCamera);
            }
            else
            {
                handlesScale = gb_InputManager.ActiveSceneView.Camera.orthographicSize / 2f;
            }

            transform.localScale = _initialScale * handlesScale;

            if(transform.position != _position)
            {
                var verts = MeshFilter.sharedMesh.vertices;

                foreach (var vertIndex in VerticesToManipulate)
                {
                    verts[vertIndex] = MeshFilter.transform.worldToLocalMatrix.MultiplyPoint3x4(transform.position);
                }

                MeshFilter.sharedMesh.vertices = verts;
                MeshFilter.sharedMesh.RecalculateNormals();
                MeshFilter.sharedMesh.RecalculateBounds();
                MeshFilter.sharedMesh.RecalculateTangents();
                MeshFilter.sharedMesh.RecalculateUVDistributionMetrics();
                MeshFilter.GetComponent<MeshCollider>().enabled = false;
                MeshFilter.GetComponent<MeshCollider>().enabled = true;
                MeshFilter.GetComponent<gb_ObjectComponent>().Object.Save();
                _position = transform.position;
                WorldUvs(MeshFilter.transform, MeshFilter.sharedMesh);
            }
        }

        public static void WorldUvs(Transform tr, Mesh mesh)
        {
            var newsUv = new Vector2[mesh.uv.Length];
            var tris = mesh.triangles;
            for (int i = 0; i < tris.Length; i += 3)
            {
                Vector3 a = tr.TransformPoint(mesh.vertices[tris[i]]);
                Vector3 b = tr.TransformPoint(mesh.vertices[tris[i + 1]]);
                Vector3 c = tr.TransformPoint(mesh.vertices[tris[i + 2]]);

                Vector3 center = (a + b + c) / 3;
                Vector3 normal = mesh.normals[tris[i]];

                a = Quaternion.LookRotation(normal) * a;
                b = Quaternion.LookRotation(normal) * b;
                c = Quaternion.LookRotation(normal) * c;

                newsUv[tris[i]] = new Vector2(a.x, a.y);
                newsUv[tris[i + 1]] = new Vector2(b.x, b.y);
                newsUv[tris[i + 2]] = new Vector2(c.x, c.y);
            }

            mesh.uv = newsUv;
        }

    }
}

