using Graybox.Gui;
using Graybox.Tools;
using Graybox.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Graybox.In
{
    [DefaultExecutionOrder(-50)]
    public class gb_InputManager : gb_Singleton<gb_InputManager>
    {

        public static UnityEvent<Rect> OnDrag = new UnityEvent<Rect>();
        public static UnityEvent<Rect> OnDragEnd = new UnityEvent<Rect>();
        public static UnityEvent<Rect, List<GameObject>> OnBoxSelect = new UnityEvent<Rect, List<GameObject>>();

        public static Camera MainCamera => Camera.main;
        public static gb_SceneView ActiveSceneView { get; set; }
        public static List<RaycastHit> HitsUnderCursor { get; } = new List<RaycastHit>();
        public static List<GameObject> SelectedObjects { get; } = new List<GameObject>();
        public static GameObject ActiveObject => SelectedObjects.Count > 0 ? SelectedObjects[0] : null;

        private RaycastHit[] _hitCache = new RaycastHit[256];

        private Vector3 _dragBegin;
        private Rect _dragRect;
        private bool _dragging;
        private gb_SceneLabel _xLabel;
        private gb_SceneLabel _yLabel;

        private void Awake()
        {
            ActiveSceneView = GameObject.FindObjectOfType<gb_SceneView>();
        }

        private void Update()
        {
            var ray = ScreenToRay(Input.mousePosition);
            var hits = Physics.RaycastNonAlloc(ray, _hitCache);
            HitsUnderCursor.Clear();
            for (int i = 0; i < hits; i++) 
            {
                HitsUnderCursor.Add(_hitCache[i]);
            }

            UpdateMouseDrag();
        }

        private void UpdateMouseDrag()
        {
            var is3d = ActiveSceneView.SceneAngle == gb_SceneViewAngle.ThreeDimensional;
            var mpos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000);

            if (gb_Binds.JustDown(gb_Bind.Select))
            {
                _dragBegin = ScreenToWorld(mpos);
            }

            if (gb_Binds.JustUp(gb_Bind.Select))
            {
                if (_dragging)
                {
                    OnDragEnd?.Invoke(_dragRect);
                    PerformBoxSelect(_dragRect);
                    _dragging = false;
                    if (_xLabel)
                    {
                        _xLabel.Destroy();
                        _yLabel.Destroy();
                    }
                }
            }

            if (gb_Binds.IsDown(gb_Bind.Select))
            {
                Vector3 finalDragStart, finalDragEnd;

                var wsDragEnd = ScreenToWorld(mpos);
                finalDragStart = WorldToScene(_dragBegin, !is3d);
                finalDragEnd = WorldToScene(wsDragEnd, !is3d);

                if (Vector2.Distance(finalDragStart, finalDragEnd) > 10
                    && !gb_ToolManager.Instance.ToolHasFocus())
                {
                    _dragging = true;
                    _dragRect = new Rect(finalDragStart, finalDragEnd - finalDragStart);
                    ActiveSceneView.Draw.Draw2dRect(_dragRect, 2f, 0, false, Color.yellow);
                    OnDrag?.Invoke(_dragRect);

                    if (!is3d)
                    {
                        if (!_xLabel)
                        {
                            CreateDragLabels();
                        }

                        var wsMin = SceneToWorld(_dragRect.min);
                        var wsMax = SceneToWorld(_dragRect.max);
                        var szX = Mathf.Abs(wsMax.x - wsMin.x);
                        var szY = Mathf.Abs(wsMax.y - wsMin.y);
                        _xLabel.WorldPosition = new Vector3((wsMin.x + wsMax.x) / 2, wsMax.y, 0);
                        _yLabel.WorldPosition = new Vector3(wsMin.x, (wsMin.y + wsMax.y) / 2, 0);
                        _xLabel.SetText(gb_Settings.Instance.ConvertTo(szX).ToString(".00"));
                        _yLabel.SetText(gb_Settings.Instance.ConvertTo(szY).ToString(".00"));
                    }
                }
            }

            Vector2 WorldToScene(Vector3 point, bool snapToGrid)
            {
                if (snapToGrid)
                {
                    point = point.Snap(ActiveSceneView.GridSize * ActiveSceneView.GridScale);
                }
                return WorldToScreen(point);
            }
        }

        private void CreateDragLabels()
        {
            _xLabel = ActiveSceneView.Draw.CreateLabel("X Axis", Vector3.zero, new Vector2(0, -30));
            _yLabel = ActiveSceneView.Draw.CreateLabel("Y Axis", Vector3.zero, new Vector2(-30, 0));
        }

        private void PerformBoxSelect(Rect rect)
        {
            var hits = new List<GameObject>();
            foreach(var obj in FindObjectsOfType<GameObject>())
            {
                var screenPos = WorldToScreen(obj.transform.position);
                var r = obj.GetComponent<Renderer>();
                if (rect.Contains(screenPos, true)
                    || (r && rect.Contains(WorldToScreen(r.bounds.min), true))
                    || (r && rect.Contains(WorldToScreen(r.bounds.max), true)))
                {
                    hits.Add(obj);
                }
            }
            OnBoxSelect?.Invoke(rect, hits);
        }

        public Ray ScreenToRay(Vector3 screenPos)
        {
            return ActiveSceneView.ScreenToRay(screenPos);
        }

        public Vector3 ScreenToWorld(Vector3 screenPos)
        {
            return ActiveSceneView.ScreenToWorld(screenPos);
        }

        public Vector3 SceneToWorld(Vector3 screenPos)
        {
            return ActiveSceneView.SceneToWorld(screenPos);
        }

        public Vector2 ScreenToScene(Vector3 screenPos)
        {
            return ActiveSceneView.ScreenToScene(screenPos);
        }

        public Vector2 WorldToScreen(Vector3 worldPos) 
        {
            return ActiveSceneView.WorldToScreen(worldPos);
        }

    }
}

