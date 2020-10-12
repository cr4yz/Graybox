using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox
{
    public class gb_DrawQuad : gb_Drawable
    {
        protected override int DrawMode => GL.QUADS;
        protected override bool ScreenSpace => true;

        private Rect _rect;
        private gb_SceneView _sceneView;

        public gb_DrawQuad(Rect worldSpaceRect, gb_SceneView sceneView)
        {
            _rect = worldSpaceRect;
            _sceneView = sceneView;
        }

        protected override void Draw()
        {
            var min = _sceneView.WorldToScreen(_rect.min);
            var max = _sceneView.WorldToScreen(_rect.max);
            GL.Vertex3(min.x, min.y, 0);
            GL.Vertex3(min.x, max.y, 0);
            GL.Vertex3(max.x, max.y, 0);
            GL.Vertex3(max.x, min.y, 0);
        }
    }
}

