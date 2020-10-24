
using UnityEngine;

namespace Graybox
{
    public abstract class gb_Drawable
    {
        protected abstract int DrawMode { get; }
        protected abstract bool ScreenSpace { get; }
        public Color Color { get; set; } = Color.green;
        public float Duration { get; set; }
        public bool Drawn;

        static Material _lineMaterial;

        public void OnPostRender(Camera cam)
        {
            if (!_lineMaterial)
            {
                CreateLineMaterial();
            }

            if (ScreenSpace)
            {
                GL.PushMatrix();
                _lineMaterial.SetPass(0);
                GL.LoadPixelMatrix();
                GL.Begin(DrawMode);
                GL.Color(Color);
                Draw();
                GL.End();
                GL.PopMatrix();
            }
            else
            {
                GL.PushMatrix();
                _lineMaterial.SetPass(0);
                GL.LoadProjectionMatrix(cam.projectionMatrix);
                GL.modelview = cam.worldToCameraMatrix;
                GL.Begin(DrawMode);
                GL.Color(Color);
                Draw();
                GL.End();
                GL.PopMatrix();
            }
        }

        protected abstract void Draw();

        private void CreateLineMaterial()
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            _lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            _lineMaterial.SetInt("_ZWrite", 1);
        }
    }
}

