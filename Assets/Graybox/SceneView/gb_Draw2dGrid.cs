using UnityEngine;

namespace Graybox
{
    public class Gb_Draw2dGrid : gb_Drawable
    {

        public Color GridColor = new Color(.4f, .4f, .4f, .5f);
        public Color MediumDivisionColor = new Color(.6f, .6f, .6f, .9f);
        public Color LargeDivisionColor = Color.cyan;
        public Color CenterColor = Color.red;
        public float GridWorldSize = 2000;
        public Vector3 GridWorldOffset = Vector3.zero;

        private gb_SceneView _sceneView;

        protected override int DrawMode => GL.LINES;
        protected override bool ScreenSpace => true;

        public Gb_Draw2dGrid(gb_SceneView sceneView)
        {
            _sceneView = sceneView;
        }

        protected override void Draw()
        {
            var cellSize = gb_Settings.Instance.SnapSize;
            var columns = (int)(GridWorldSize / cellSize);
            var columnsHalf = columns / 2;
            var offset = WorldToScreenPoint(Vector3.one * -columnsHalf * cellSize);
            var s1 = WorldToScreenPoint(new Vector3(0, 0, 0));
            var s2 = WorldToScreenPoint(new Vector3(cellSize, cellSize, 0));
            var screenDelta = s2 - s1;

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
                GL.Color(color);
                GL.Vertex3(offset.x + i * screenDelta.x, offset.y, 0);
                GL.Vertex3(offset.x + i * screenDelta.x, offset.y + columns * screenDelta.y, 0);
                GL.Vertex3(offset.x, offset.y + i * screenDelta.y, 0);
                GL.Vertex3(offset.x + columns * screenDelta.x, offset.y + i * screenDelta.y, 0);
            }
        }

        private Vector2 WorldToScreenPoint(Vector3 worldPoint)
        {
            worldPoint = _sceneView.Camera.transform.TransformDirection(worldPoint);
            return _sceneView.Camera.WorldToScreenPoint(worldPoint);
        }

        private Vector2 ScreenToWorldPoint(Vector3 screenPoint)
        {
            return _sceneView.Camera.ScreenToWorldPoint(screenPoint);
        }

    }
}

