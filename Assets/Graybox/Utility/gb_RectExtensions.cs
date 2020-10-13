using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox.Utility
{
    public static class gb_RectExtensions
    {
        public static Rect FixNegativeSize(this Rect rectOld)
        {
            var rect = new Rect(rectOld);

            if (rect.width < 0)
            {
                rect.x += rect.width;
                rect.width = Mathf.Abs(rect.width);
            }

            if (rect.height < 0)
            {
                rect.y += rect.height;
                rect.height = Mathf.Abs(rect.height);
            }

            return rect;
        }

        public static Rect SceneToGui(this Rect rect, gb_SceneView sceneView)
        {
            var ph = sceneView.Camera.pixelHeight;
            rect.min = new Vector2(rect.min.x, ph - rect.min.y);
            rect.max = new Vector2(rect.max.x, ph - rect.max.y);
            return rect.FixNegativeSize();
        }

    }
}

