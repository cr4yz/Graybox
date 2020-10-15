using Graybox.Gui;
using Graybox.Tools;
using Graybox.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        public static bool IsDragging { get; private set; }
        public static bool ToolHasFocus { get; private set; }

        private RaycastHit[] _hitCache = new RaycastHit[256];

        private Vector3 _dragBegin;
        private Rect _dragRect;
        private gb_SceneLabel _szLabel;
        private bool _selectRequiresRelease;
        private int _pickFrameBuffer;

        private void Awake()
        {
            ActiveSceneView = GameObject.FindObjectOfType<gb_SceneView>();
        }

        private void Update()
        {
            _pickFrameBuffer--;

            ToolHasFocus = gb_ToolManager.Instance.ToolHasFocus();
            HitsUnderCursor.Clear();

            if (ActiveSceneView.IsFocused)
            {
                var ray = ScreenToRay(Input.mousePosition);
                var hits = Physics.RaycastNonAlloc(ray, _hitCache);
                for (int i = 0; i < hits; i++)
                {
                    HitsUnderCursor.Add(_hitCache[i]);
                }
                UpdateMouseDrag();
            }
        }

        private void Pick()
        {
            var potentialObjects = new List<gb_ObjectComponent>();
            SelectedObjects.Clear();

            foreach (var hit in HitsUnderCursor)
            {
                if (hit.transform.GetComponentInParent<gb_GizmoTool>())
                {
                    return;
                }
                if (hit.transform.TryGetComponent(out gb_ObjectComponent objComponent))
                {
                    potentialObjects.Add(objComponent);
                }
            }

            if (potentialObjects.Count > 0)
            {
                SelectedObjects.AddRange(potentialObjects.Select(x => x.gameObject));
            }
        }

        public bool CanPick(gb_Tool allowFocus = null)
        {
            foreach(var tool in gb_ToolManager.Instance.Tools)
            {
                if (tool.HasFocus)
                {
                    if(allowFocus == tool)
                    {
                        continue;
                    }
                    return false;
                }
            }
            if ((ActiveSceneView && !ActiveSceneView.IsHovered) 
                || _pickFrameBuffer > 0)
            {
                return false;
            }
            return true;
        }

        private void UpdateMouseDrag()
        {
            if (IsDragging && gb_Binds.JustDown(gb_Bind.Cancel))
            {
                _selectRequiresRelease = true;
                IsDragging = false;
                if (_szLabel)
                {
                    _szLabel.Destroy();
                }
            }

            if (_selectRequiresRelease)
            {
                if (gb_Binds.JustUp(gb_Bind.Select))
                {
                    _pickFrameBuffer = 1;
                    _selectRequiresRelease = false;
                }
                return;
            }

            var is3d = ActiveSceneView.SceneAngle == gb_SceneViewAngle.ThreeDimensional;
            var mpos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000);

            if (gb_Binds.JustDown(gb_Bind.Select))
            {
                _dragBegin = ScreenToWorld(mpos);
            }

            if (gb_Binds.JustUp(gb_Bind.Select))
            {
                if (IsDragging)
                {
                    OnDragEnd?.Invoke(_dragRect);
                    PerformBoxSelect(_dragRect);
                    IsDragging = false;
                    _pickFrameBuffer = 1;

                    if (_szLabel)
                    {
                        _szLabel.Destroy();
                    }
                }
                else
                {
                    if (CanPick())
                    {
                        Pick();
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
                    && !ToolHasFocus)
                {
                    IsDragging = true;
                    _dragRect = new Rect(finalDragStart, finalDragEnd - finalDragStart);
                    ActiveSceneView.Draw.Draw2dRect(_dragRect, 2f, 0, false, Color.yellow);
                    OnDrag?.Invoke(_dragRect);

                    if (!is3d)
                    {
                        if (!_szLabel)
                        {
                            _szLabel = ActiveSceneView.Draw.CreateLabel("...", Vector3.zero, new Vector2(0, -30));
                        }

                        var wsMin = SceneToWorld(_dragRect.min);
                        var wsMax = SceneToWorld(_dragRect.max);
                        var szX = gb_Settings.Instance.ConvertTo(Mathf.Abs(wsMax.x - wsMin.x));
                        var szY = gb_Settings.Instance.ConvertTo(Mathf.Abs(wsMax.y - wsMin.y));
                        var szZ = gb_Settings.Instance.ConvertTo(Mathf.Abs(wsMax.z - wsMin.z));
                        var sz = new Vector3(szX, szY, szZ);
                        _szLabel.WorldPosition = wsMin;
                        _szLabel.SetText(sz.ToString());
                    }
                }
            }

            Vector2 WorldToScene(Vector3 point, bool snapToGrid)
            {
                if (snapToGrid)
                {
                    point = point.Snap(gb_Settings.Instance.SnapSize);
                }
                return WorldToScreen(point);
            }
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

