using Graybox.In;
using Graybox.Tools;
using Graybox.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Graybox
{
    public enum gb_SceneViewAngle
    {
        ThreeDimensional,
        Top,
        Front,
        Side
    }

    [RequireComponent(typeof(RawImage))]
    [RequireComponent(typeof(RectTransform))]
    public class gb_SceneView : MonoBehaviour, 
        IPointerEnterHandler,
        IPointerExitHandler
    {
        public gb_SceneViewAngle SceneAngle;

        private gb_SceneViewAngle _currentAngle;
        private RenderTexture _renderTexture;
        private Vector3[] _worldCorners = new Vector3[4];
        private gb_SceneCamera _flyCamera;

        public bool IsHovered { get; private set; }
        public bool IsFocused { get; private set; }
        public Camera Camera { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public gb_SceneDrawManager Draw { get; private set; }
        public GameObject InScene { get; private set; }

        private GameObject _rootObject;
        private Camera _gridCamera;
        private Camera _gizmoCamera;
        private Gb_Draw2dGrid _gridDrawable;

        private const float _orthographicCameraDistance = -50000;

        private void Awake()
        {
            var rtSize = new Vector2(Screen.width, Screen.height);

            RectTransform = GetComponent<RectTransform>();

            _renderTexture = new RenderTexture((int)rtSize.x, (int)rtSize.y, 0);
            _renderTexture.antiAliasing = 8;
            _renderTexture.filterMode = FilterMode.Trilinear;
            GetComponent<RawImage>().texture = _renderTexture;

            _rootObject = new GameObject("[Scene View]");

            var cameraObj = new GameObject();
            cameraObj.transform.SetParent(_rootObject.transform, true);
            Draw = cameraObj.AddComponent<gb_SceneDrawManager>();
            Draw.SceneView = this;
            Draw.RenderObjects = true;
            Camera = cameraObj.AddComponent<Camera>();
            Camera.forceIntoRenderTexture = true;
            Camera.targetTexture = _renderTexture;
            Camera.cullingMask = LayerMask.NameToLayer("UI");
            Camera.orthographic = true;
            Camera.orthographicSize = 5;
            Camera.clearFlags = CameraClearFlags.Depth;
            Camera.depth = 1;

            InScene = new GameObject("InScene");
            InScene.transform.SetParent(_rootObject.transform, true);
            var inSceneCanvas = InScene.AddComponent<Canvas>();
            inSceneCanvas.worldCamera = Camera;
            inSceneCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            inSceneCanvas.planeDistance = 1;
            var canvasScaler = InScene.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = rtSize;
            InScene.AddComponent<gb_SceneViewRaycaster>().SceneView = this;
            InScene.SetLayerRecursively(LayerMask.NameToLayer("UI"));

            _flyCamera = cameraObj.AddComponent<gb_SceneCamera>();
            _flyCamera.enableZoom = false;
            _flyCamera.enabled = false;

            _gridCamera = CreateCamera(cameraObj.transform, "Grid", 0, 0, CameraClearFlags.SolidColor, Camera);
            _gridCamera.gameObject.AddComponent<gb_SceneDrawManager>();
            _gridCamera.backgroundColor = Color.black;
            _gridDrawable = new Gb_Draw2dGrid(this)
            {
                Duration = float.MaxValue
            };

            _gizmoCamera = CreateCamera(cameraObj.transform, "Gizmos", 2, LayerMask.GetMask("Gizmos"), CameraClearFlags.Nothing, Camera);
            _gizmoCamera.gameObject.SetActive(false);
            _gizmoCamera.farClipPlane = Mathf.Max(Mathf.Abs(_orthographicCameraDistance) * 2, 1000);

            SetAngle(SceneAngle);
        }

        private void OnEnable()
        {
            if (Camera)
            {
                Camera.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            IsHovered = false;
            _wantsFocus = false;

            if (Camera)
            {
                Camera.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (Camera)
            {
                GameObject.Destroy(Camera.gameObject);
            }
            if (_renderTexture)
            {
                GameObject.Destroy(_renderTexture);
            }
        }

        private void Update()
        {
            IsHovered = RectTransformUtility.RectangleContainsScreenPoint(RectTransform, Input.mousePosition, Camera.main);

            if(_wantsFocus && !IsFocused)
            {
                TrySetActive();
            }

            if (_currentAngle != SceneAngle)
            {
                SetAngle(SceneAngle);
            }

            UpdateAspectRatio();
            UpdateHotkeys();
            UpdateGridZoom();
        }

        public void Focus(Transform tr)
        {
            if (SceneAngle == gb_SceneViewAngle.ThreeDimensional)
            {
                var renderer = tr.GetComponent<Renderer>();
                float cameraDistance = 2.0f; // Constant factor
                Vector3 objectSizes = renderer.bounds.max - renderer.bounds.min;
                float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
                float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * Camera.fieldOfView); // Visible height 1 meter in front
                float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
                distance += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
                Camera.transform.position = renderer.bounds.center - distance * Camera.transform.forward;
            }
            else
            {
                Camera.orthographicSize = 5;
                Camera.transform.position = tr.position;
                Camera.transform.position += Camera.transform.forward * _orthographicCameraDistance;
            }
        }

        private Camera CreateCamera(Transform parent, string name, int depth, int cullingMask, CameraClearFlags clearFlags, Camera copyFrom = null)
        {
            var cameraObj = new GameObject(name);
            cameraObj.transform.SetParent(parent, true);
            var camera = cameraObj.AddComponent<Camera>();
            camera.cullingMask = cullingMask;
            camera.depth = depth;
            camera.clearFlags = clearFlags;
            camera.forceIntoRenderTexture = true;
            camera.targetTexture = _renderTexture;
            if (copyFrom)
            {
                cameraObj.AddComponent<gb_CopyCamera>().CameraToCopy = Camera;
            }
            return camera;
        }

        private void SetAngle(gb_SceneViewAngle angle)
        {
            if(angle == gb_SceneViewAngle.ThreeDimensional)
            {
                Camera.orthographic = false;
                Camera.clearFlags = CameraClearFlags.Skybox;
                Camera.cullingMask = -1;
                Camera.cullingMask &= ~LayerMask.GetMask("Gizmos", "Grids");
                _flyCamera.lockRotation = false;
                _flyCamera.lockWasd = false;
                _gridCamera.GetComponent<gb_SceneDrawManager>().Remove(_gridDrawable);
            }
            else
            {
                _flyCamera.lockRotation = true;
                _flyCamera.lockWasd = true;
                _gridCamera.GetComponent<gb_SceneDrawManager>().Add(_gridDrawable);
                Camera.clearFlags = CameraClearFlags.Nothing;
                Camera.cullingMask = LayerMask.GetMask("UI");
                Camera.orthographic = true;
                Camera.transform.position = new Vector3(0, 0, _orthographicCameraDistance);
                switch (angle)
                {
                    case gb_SceneViewAngle.Front:
                        _rootObject.transform.eulerAngles = new Vector3(0, 0, 0);
                        _rootObject.transform.position = new Vector3(0, 0, 0);
                        break;
                    case gb_SceneViewAngle.Side:
                        _rootObject.transform.eulerAngles = new Vector3(0, 90, 0);
                        _rootObject.transform.position = new Vector3(0, 0, 0);
                        break;
                    case gb_SceneViewAngle.Top:
                        _rootObject.transform.eulerAngles = new Vector3(90, 0, 0);
                        _rootObject.transform.position = new Vector3(0, 0, 0);
                        break;
                }
            }
            _currentAngle = angle;
        }

        private bool _wantsFocus;
        private void TrySetActive()
        {
            if (gb_InputManager.IsDragging)
            {
                return;
            }
            IsFocused = true;
            _flyCamera.enabled = true;
            _gizmoCamera.gameObject.SetActive(true);
            if (gb_InputManager.ActiveSceneView && gb_InputManager.ActiveSceneView != this)
            {
                gb_InputManager.ActiveSceneView._gizmoCamera.gameObject.SetActive(false);
                gb_InputManager.ActiveSceneView._flyCamera.enabled = false;
                gb_InputManager.ActiveSceneView.IsFocused = false;
            }
            gb_InputManager.ActiveSceneView = this;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            //IsHovered = true;
            _wantsFocus = true;
            TrySetActive();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            //IsHovered = false;
            _wantsFocus = false;
        }

        private void UpdateAspectRatio()
        {
            RectTransform.GetWorldCorners(_worldCorners);
            var min = _worldCorners[0];
            var max = _worldCorners[2];
            var width = max.x - min.x;
            var height = max.y - min.y;
            Camera.aspect = width / height;
        }

        private void UpdateHotkeys()
        {
            if (gb_InputManager.ActiveSceneView == this)
            {
                if (gb_Binds.JustDown(gb_Bind.Focus)
                    && gb_InputManager.ActiveObject)
                {
                    Focus(gb_InputManager.ActiveObject.transform);
                }
            }
        }

        private void UpdateGridZoom()
        {
            if(gb_InputManager.ActiveSceneView != this)
            {
                return;
            }

            if (Camera.orthographic)
            {
                var scrollOrthoDelta = Mathf.Lerp(1, 100f, Camera.orthographicSize / 1000f);

                if (Input.mouseScrollDelta.y > 0)
                {
                    var posa = gb_InputManager.Instance.ScreenToWorld(Input.mousePosition);
                    Camera.orthographicSize = Mathf.Max(.1f, Camera.orthographicSize - scrollOrthoDelta);
                    var posb = gb_InputManager.Instance.ScreenToWorld(Input.mousePosition);
                    Camera.transform.position += posa - posb;
                }

                if (Input.mouseScrollDelta.y < 0)
                {
                    var posa = gb_InputManager.Instance.ScreenToWorld(Input.mousePosition);
                    Camera.orthographicSize = Mathf.Min(1000, Camera.orthographicSize + scrollOrthoDelta);
                    var posb = gb_InputManager.Instance.ScreenToWorld(Input.mousePosition);
                    Camera.transform.position += posa - posb;
                }
            }
            else
            {

            }
        }

        /* AHHHHHHHHHHHHHH */

        public Ray SceneToRay(Vector3 scenePos)
        {
            return Camera.ScreenPointToRay(scenePos);
        }

        public Ray ScreenToRay(Vector3 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, screenPos, gb_InputManager.MainCamera, out Vector2 localPoint);
            var normalizedPoint = Rect.PointToNormalized(RectTransform.rect, localPoint);
            return Camera.ViewportPointToRay(normalizedPoint);
        }

        public Vector3 ScreenToWorld(Vector3 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, screenPos, gb_InputManager.MainCamera, out Vector2 localPoint);
            var normalizedPoint = Rect.PointToNormalized(RectTransform.rect, localPoint);
            var point = new Vector3(normalizedPoint.x, normalizedPoint.y, screenPos.z);
            var result = Camera.ViewportToWorldPoint(point);
            if(SceneAngle != gb_SceneViewAngle.ThreeDimensional)
            {
                result -= Camera.transform.TransformDirection(0, 0, _orthographicCameraDistance);
            }
            return result;
        }

        public Vector3 SceneToWorld(Vector3 screenPos)
        {
            var result = Camera.ScreenToWorldPoint(screenPos);
            if (SceneAngle != gb_SceneViewAngle.ThreeDimensional)
            {
                result -= Camera.transform.TransformDirection(0, 0, _orthographicCameraDistance);
            }
            return result;
        }

        public Vector2 ScreenToScene(Vector3 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, screenPos, gb_InputManager.MainCamera, out Vector2 localPoint);
            var normalizedPoint = Rect.PointToNormalized(RectTransform.rect, localPoint);
            return Camera.ViewportToScreenPoint(normalizedPoint);
        }

        public Vector2 WorldToScreen(Vector3 worldPos)
        {
            return Camera.WorldToScreenPoint(worldPos);
        }

        public Vector2 ScreenToRect(Vector2 screenPos)
        {
            return ViewportToRect(new Vector2(screenPos.x / Camera.pixelWidth, screenPos.y / Camera.pixelHeight));
        }

        public Vector2 ViewportToRect(Vector3 viewport)
        {
            var arr = new Vector3[4];
            RectTransform.GetLocalCorners(arr);
            var min = arr[0];
            var max = arr[2];
            var sz = max - min;
            return Vector3.Scale(sz, viewport);
        }

    }
}

