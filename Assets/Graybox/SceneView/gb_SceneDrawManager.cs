using Graybox.Gui;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox
{
    public class gb_SceneDrawManager : MonoBehaviour
    {

        public gb_SceneView SceneView;

        private List<gb_Drawable> _drawables = new List<gb_Drawable>();
        private List<gb_Drawable> _pool = new List<gb_Drawable>();

        private void Update()
        {
            for(int i = _drawables.Count - 1; i >=0; i--)
            {
                var drawable = _drawables[i];
                drawable.Duration -= Time.deltaTime;
                if(drawable.Duration <= 0 && drawable.Drawn)
                {
                    PoolDrawable(_drawables[i]);
                    _drawables.RemoveAt(i);
                }
            }
        }

        private void OnPostRender()
        {
            foreach(var drawable in _drawables)
            {
                drawable.OnPostRender();
                drawable.Drawn = true;
            }
        }

        public void Add(gb_Drawable drawable)
        {
            _drawables.Add(drawable);
        }

        public void Draw2dRect(Rect rect, float borderWidth, float duration, bool filled, Color color)
        {
            Draw2dRect(rect.min, rect.max, borderWidth, duration, filled, color);
        }

        public void Draw2dRect(Vector2 min, Vector2 max, float borderWidth, float duration, bool filled, Color color)
        {
            var rect = GetDrawable<gb_Draw2dRect>();
            rect.Min = min;
            rect.Max = max;
            rect.BorderWidth = borderWidth;
            rect.Color = color;
            rect.Filled = filled;
            rect.Duration = duration;
            _drawables.Add(rect);
        }

        public gb_SceneLabel CreateLabel(string text, Vector3 worldPosition, Vector2 screenOffset = default)
        {
            var obj = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/SceneLabel"), SceneView.transform);
            var sceneLabel = obj.GetComponent<gb_SceneLabel>();
            sceneLabel.SceneView = SceneView;
            sceneLabel.WorldPosition = worldPosition;
            sceneLabel.ScreenOffset = screenOffset;
            sceneLabel.SetText(text);
            return sceneLabel;
        }

        private T GetDrawable<T>()
            where T : gb_Drawable, new()
        {
            for(int i = _pool.Count - 1; i >= 0; i--)
            {
                if(_pool[i] is T)
                {
                    var result = _pool[i] as T;
                    _pool.RemoveAt(i);
                    return result;
                }
            }
            return new T();
        }

        private void PoolDrawable(gb_Drawable drawable)
        {
            drawable.Drawn = false;
            // low effort pooling
            if (_pool.Count > 100)
            {
                _pool.Clear();
            }
            _pool.Add(drawable);
        }

    }
}

