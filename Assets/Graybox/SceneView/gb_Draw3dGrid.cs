using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox
{
    public class gb_Draw3dGrid : gb_Drawable
    {
        protected override int DrawMode => GL.LINES;
        protected override bool ScreenSpace => false;

        public float GridWorldSize = 20f;
        public Vector3 Normal = Vector3.up;
        public Color GridColor = Color.white;
        public Color CenterColor = Color.red;
        public Vector3 Center;

        protected override void Draw()
        {
            var cellSize = gb_Settings.Instance.SnapSize;
            var columns = (int)(GridWorldSize / cellSize);
            var columnsHalf = columns / 2;
            var offset = Center + Vector3.one * -columnsHalf * cellSize;
            var screenDelta = Vector3.one * cellSize;
            var m = Matrix4x4.TRS(Vector3.zero, Quaternion.FromToRotation(Vector3.forward, Normal), Vector3.one);

            for (int i = 0; i < columns + 1; i++)
            {
                var color = GridColor;
                if (i == columnsHalf || i == 0 || i == columns)
                {
                    color = CenterColor;
                }
                else
                {
                    // do other subdivisions
                }

                var p1 = new Vector3(offset.x + i * screenDelta.x, offset.y, Center.z);
                var p2 = new Vector3(offset.x + i * screenDelta.x, offset.y + columns * screenDelta.y, Center.z);
                var p3 = new Vector3(offset.x, offset.y + i * screenDelta.y, Center.z);
                var p4 = new Vector3(offset.x + columns * screenDelta.x, offset.y + i * screenDelta.y, Center.z);

                p1 = m.MultiplyPoint3x4(p1 - Center) + Center;
                p2 = m.MultiplyPoint3x4(p2 - Center) + Center;
                p3 = m.MultiplyPoint3x4(p3 - Center) + Center;
                p4 = m.MultiplyPoint3x4(p4 - Center) + Center;

                GL.Color(color);
                GL.Vertex(p1);
                GL.Vertex(p2);
                GL.Vertex(p3);
                GL.Vertex(p4);
            }
        }
    }
}

