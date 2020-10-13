using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox
{
    public class gb_Draw2dQuad : gb_Drawable
    {
        protected override int DrawMode => GL.QUADS;
        protected override bool ScreenSpace => true;

        private Rect _rect;
        private gb_SceneView _sceneView;

        public gb_Draw2dQuad(Rect rect, gb_SceneView sceneView)
        {
            _sceneView = sceneView;
            _rect = rect;
        }

        protected override void Draw()
        {
            var min = _rect.min;
            var max = _rect.max;
            GL.Vertex3(min.x, min.y, 0);
            GL.Vertex3(min.x, max.y, 0);
            GL.Vertex3(max.x, max.y, 0);
            GL.Vertex3(max.x, min.y, 0);
        }
    }
}

