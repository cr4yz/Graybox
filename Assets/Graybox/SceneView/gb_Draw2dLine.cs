using UnityEngine;

namespace Graybox
{
    public class gb_Draw2dLine : gb_Drawable
    {

        protected override int DrawMode => GL.QUADS;
        protected override bool ScreenSpace => true;

        public float Width = 2;
        public Vector2 PointA;
        public Vector2 PointB;

        protected override void Draw()
        {
            DrawLine(PointA, PointB, Width);
        }

        public static void DrawLine(Vector2 a, Vector2 b, float width)
        {
            var perp = Vector2.Perpendicular(b - a).normalized * width / 2;
            GL.Vertex(a + perp);
            GL.Vertex(b + perp);
            GL.Vertex(b - perp);
            GL.Vertex(a - perp);
        }

    }
}

