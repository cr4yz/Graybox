using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Graybox
{
    [RequireComponent(typeof(Canvas))]
    public class gb_SceneViewRaycaster : BaseRaycaster
    {

        public gb_SceneView SceneView;

        public override Camera eventCamera => SceneView.Camera;

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (!SceneView.IsHovered)
            {
                return;
            }
            var pos = SceneView.ScreenToScene(Input.mousePosition);
            var hits = new List<Graphic>();
            var canvas = GetComponent<Canvas>();
            Raycast(canvas, eventCamera, pos, hits);
            foreach(var hit in hits)
            {
                var result = new RaycastResult()
                {
                    gameObject = hit.gameObject,
                    depth = hit.depth,
                    sortingLayer = canvas.sortingLayerID,
                    sortingOrder = canvas.sortingOrder,
                    module = this,
                    index = resultAppendList.Count
                };
                resultAppendList.Add(result);
            }
        }

        /// <summary>
        /// Perform a raycast into the screen and collect all graphics underneath it.
        /// </summary>
        [NonSerialized] static readonly List<Graphic> s_SortedGraphics = new List<Graphic>();
        private static void Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, List<Graphic> results)
        {
            // Debug.Log("ttt" + pointerPoision + ":::" + camera);
            // Necessary for the event system
            var foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
            s_SortedGraphics.Clear();
            for (int i = 0; i < foundGraphics.Count; ++i)
            {
                Graphic graphic = foundGraphics[i];

                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if (graphic.depth == -1)
                    continue;

                if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera))
                    continue;

                if (graphic.Raycast(pointerPosition, eventCamera))
                {
                    s_SortedGraphics.Add(graphic);
                }
            }

            s_SortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
            //		StringBuilder cast = new StringBuilder();
            for (int i = 0; i < s_SortedGraphics.Count; ++i)
                results.Add(s_SortedGraphics[i]);
            //		Debug.Log (cast.ToString());
        }

    }
}

