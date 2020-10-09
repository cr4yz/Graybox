using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox
{
    public class gb_Draw2dRect : gb_Drawable
    {
        protected override int DrawMode => GL.QUADS;
        protected override bool ScreenSpace => true;

        public Vector2 Min;
        public Vector2 Max;
        public bool Filled;
        public float BorderWidth;

        protected override void Draw()
        {
            if (Filled)
            {
            }
            else
            {
                var p1 = Min;
                var p2 = new Vector2(Min.x, Max.y);
                var p3 = Max;
                var p4 = new Vector2(Max.x, Min.y);
                gb_Draw2dLine.DrawLine(p1, p2, BorderWidth);
                gb_Draw2dLine.DrawLine(p2, p3, BorderWidth);
                gb_Draw2dLine.DrawLine(p3, p4, BorderWidth);
                gb_Draw2dLine.DrawLine(p4, p1, BorderWidth);
            }
        }


    }
}

