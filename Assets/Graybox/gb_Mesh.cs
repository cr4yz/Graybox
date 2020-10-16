using Graybox.In;
using Graybox.Tools;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

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

        [JsonIgnore]
        public MeshFilter MeshFilter => _mf;
        [JsonIgnore]
        private MeshFilter _mf;
        [JsonIgnore]
        private MeshRenderer _mr;
        [JsonIgnore]
        private MeshCollider _mc;

        protected override void OnSave()
        {
            var mesh = _mf.sharedMesh;
            Vertices = mesh.vertices;
            Triangles = mesh.triangles;
            Colors = mesh.colors;
            Normals = mesh.normals;
            Uvs = mesh.uv;

            ColliderIsConvex = _mc.convex;
            ColliderIsTrigger = _mc.isTrigger;

            Materials = _mr.materials.Select(x => (gb_Material)x).ToArray();
        }

        protected override void OnLoad()
        {
            _mf.sharedMesh = new Mesh()
            {
                vertices = Vertices,
                triangles = Triangles,
                colors = Colors,
                normals = Normals,
                uv = Uvs
            };

            _mr.materials = Materials.Select(x => (Material)x).ToArray();

            _mc.sharedMesh = _mf.sharedMesh;
            _mc.convex = ColliderIsConvex;
            _mc.isTrigger = ColliderIsTrigger;
        }

        protected override void OnIntegrated()
        {
            if (!GameObject.TryGetComponent(out _mf))
            {
                _mf = GameObject.AddComponent<MeshFilter>();
            }
            if (!GameObject.TryGetComponent(out _mr))
            {
                _mr = GameObject.AddComponent<MeshRenderer>();
            }
            if (!GameObject.TryGetComponent(out _mc))
            {
                _mc = GameObject.AddComponent<MeshCollider>();
            }
        }

        public override void OnPostRender(gb_SceneView sceneView)
        {
            var isSelected = gb_InputManager.ActiveObject == GameObject;

            if (!isSelected && sceneView.SceneAngle == gb_SceneViewAngle.ThreeDimensional)
            {
                return;
            }

            var color = isSelected ? gb_Settings.Instance.ObjectSelectedColor : gb_Settings.Instance.ObjectColor;
            sceneView.Draw.Add(new gb_DrawMeshOutline(sceneView, GameObject.GetComponent<ProBuilderMesh>())
            {
                Color = color
            });
        }

    }
}

