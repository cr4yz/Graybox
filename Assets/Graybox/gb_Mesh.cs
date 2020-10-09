using System.Linq;
using UnityEngine;

namespace Graybox
{
    public class gb_Mesh : gb_Object
    {
        public Vector3[] Vertices { get; set; }
        public int[] Triangles { get; set; }
        public Color[] Colors { get; set; }
        public Vector3[] Normals { get; set; }
        public Vector2[] Uvs { get; set; }
        public gb_Material[] Materials { get; set; }
        public bool ColliderIsConvex { get; set; }
        public bool ColliderIsTrigger { get; set; }

        private gb_DrawMeshOutline _outline;

        protected override void OnLoad()
        {
            if (!GameObject.TryGetComponent(out MeshFilter mf))
            {
                mf = GameObject.AddComponent<MeshFilter>();
            }

            mf.mesh = new Mesh()
            {
                vertices = Vertices,
                triangles = Triangles,
                colors = Colors,
                normals = Normals,
                uv = Uvs
            };

            if (!GameObject.TryGetComponent(out MeshRenderer mr))
            {
                mr = GameObject.AddComponent<MeshRenderer>();
            }

            mr.materials = Materials.Select(x => (Material)x).ToArray();

            if (!GameObject.TryGetComponent(out MeshCollider mc))
            {
                mc = GameObject.AddComponent<MeshCollider>();
            }

            mc.convex = ColliderIsConvex;
            mc.isTrigger = ColliderIsTrigger;
        }

        protected override void OnSave()
        {
            var mesh = GameObject.GetComponent<MeshFilter>().mesh;
            Vertices = mesh.vertices;
            Triangles = mesh.triangles;
            Colors = mesh.colors;
            Normals = mesh.normals;
            Uvs = mesh.uv;

            var meshCollider = GameObject.GetComponent<MeshCollider>();
            ColliderIsConvex = meshCollider.convex;
            ColliderIsTrigger = meshCollider.isTrigger;

            var mr = GameObject.GetComponent<MeshRenderer>();
            Materials = mr.materials.Select(x => (gb_Material)x).ToArray();
        }

        protected override void OnAdded(gb_Map map)
        {
            foreach (var sceneView in GameObject.FindObjectsOfType<gb_SceneView>())
            {
                sceneView.Draw.Add(new gb_DrawMeshOutline(sceneView, this)
                {
                    Duration = float.MaxValue
                });
            }
        }

    }
}

