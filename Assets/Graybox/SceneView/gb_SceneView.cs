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

        public Camera Camera { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public gb_SceneDrawManager Draw { get; private set; }
        public float GridSize { get; set; } = 32;
        public float GridScale => gb_Settings.Instance.UnitScale;

        private GameObject _rootObject;
        private gb_WireframeCamera _wireframe;

        private Camera _gridCamera;
        private Camera _gizmoCamera;

        //private GameObject _gridObject;
        private const float _orthographicCameraDistance = -50000;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();

            _renderTexture = new RenderTexture(1280, 720, 1);
            _renderTexture.antiAliasing = 8;
            _renderTexture.filterMode = FilterMode.Trilinear;
            GetComponent<RawImage>().texture = _renderTexture;

            _rootObject = new GameObject("[Scene View]");

            var cameraObj = new GameObject();
            cameraObj.transform.SetParent(_rootObject.transform, true);
            Draw = cameraObj.AddComponent<gb_SceneDrawManager>();
            Draw.SceneView = this;
            Camera = cameraObj.AddComponent<Camera>();
            Camera.forceIntoRenderTexture = true;
            Camera.targetTexture = _renderTexture;
            Camera.cullingMask = 0;
            //Camera.cullingMask &= ~LayerMask.GetMask("Gizmos", "Grids");
            Camera.orthographic = true;
            Camera.orthographicSize = 5;
            Camera.clearFlags = CameraClearFlags.Depth;
            Camera.depth = 1;

            _flyCamera = cameraObj.AddComponent<gb_SceneCamera>();
            _flyCamera.enableZoom = false;
            _flyCamera.enabled = false;
            _wireframe = cameraObj.AddComponent<gb_WireframeCamera>();
            _wireframe.setCameraBackgroundColor = false;

            _gridCamera = CreateCamera(cameraObj.transform, "Grid", 0, 0, CameraClearFlags.SolidColor, Camera);
            _gridCamera.backgroundColor = Color.black;
            var draw = _gridCamera.gameObject.AddComponent<gb_SceneDrawManager>();
            draw.Add(new Gb_Draw2dGrid(this)
            {
                Duration = float.MaxValue
            });

            _gizmoCamera = CreateCamera(cameraObj.transform, "Gizmos", 2, LayerMask.GetMask("Gizmos"), CameraClearFlags.Nothing, Camera);
            _gizmoCamera.gameObject.SetActive(false);
            _gizmoCamera.farClipPlane = Mathf.Abs(_orthographicCameraDistance * 2);
            //_gridCamera.backgroundColor = Color.black;
            CreateGrid();

            SetWireframeColor(Color.green);
            SetWireframeBackColor(Color.yellow);
            SetWireframeThickness(.05f);
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
            if (_currentAngle != SceneAngle)
            {
                SetAngle(SceneAngle);
            }

            UpdateAspectRatio();
            UpdateHotkeys();
            UpdateGridZoom();
        }

        public void EnableWireframe(bool enabled)
        {
            _wireframe.enableWireframe = enabled;
        }

        public void SetWireframeColor(Color color)
        {
            _wireframe.frontColor = color;
        }

        public void SetWireframeBackColor(Color color)
        {
            _wireframe.backColor = color;
        }

        public void SetWireframeThickness(float thickness)
        {
            _wireframe.wireThickness = thickness;
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

        private void CreateGrid()
        {
            //var gridObjScale = 2000f;
            //var gridSpacing = 1f / gridObjScale;
            //_gridObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //_gridObject.layer = 7;
            //_gridObject.transform.SetParent(_rootObject.transform);
            //_gridObject.transform.position = Vector3.zero;
            //_gridObject.transform.localScale = new Vector3(gridObjScale, 1, gridObjScale);
            //_gridMaterial = new Material(Shader.Find("Graybox/Grid"));
            //_gridMaterial.SetVector("_GridSpacing", Vector4.one * gridSpacing);
            //_gridObject.GetComponent<Renderer>().material = _gridMaterial;
        }

        private void SetAngle(gb_SceneViewAngle angle)
        {
            if(angle == gb_SceneViewAngle.ThreeDimensional)
            {
                Camera.orthographic = false;
                Camera.clearFlags = CameraClearFlags.Skybox;
                Camera.cullingMask = -1 & ~LayerMask.GetMask("Gizmos", "Grids");
                //_gridCamera.gameObject.SetActive(false);
                //_gridObject.SetActive(false);
                _flyCamera.lockRotation = false;
                _flyCamera.lockWasd = false;
                _wireframe.enableWireframe = false;
            }
            else
            {
                //_gridObject.SetActive(true);
                //_gridCamera.gameObject.SetActive(true);
                _flyCamera.lockRotation = true;
                _flyCamera.lockWasd = true;
                _wireframe.enableWireframe = true;
                Camera.clearFlags = CameraClearFlags.Nothing;
                Camera.cullingMask = 0;
                Camera.orthographic = true;
                Camera.transform.position = new Vector3(0, 0, _orthographicCameraDistance);
                EnableWireframe(true);
                switch (angle)
                {
                    case gb_SceneViewAngle.Front:
                        _rootObject.transform.eulerAngles = new Vector3(0, 0, 0);
                        _rootObject.transform.position = new Vector3(0, 0, 0);
                        //_gridObject.transform.localEulerAngles = new Vector3(-90, 0, 0);
                        break;
                    case gb_SceneViewAngle.Side:
                        _rootObject.transform.eulerAngles = new Vector3(0, 90, 0);
                        _rootObject.transform.position = new Vector3(0, 0, 0);
                        //_gridObject.transform.localEulerAngles = new Vector3(0, -90, 90);
                        break;
                    case gb_SceneViewAngle.Top:
                        _rootObject.transform.eulerAngles = new Vector3(90, 0, 0);
                        _rootObject.transform.position = new Vector3(0, 0, 0);
                        //_gridObject.transform.localEulerAngles = new Vector3(-90, 0, 0);
                        break;
                }
            }
            _currentAngle = angle;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _flyCamera.enabled = true;
            _gizmoCamera.gameObject.SetActive(true);
            gb_InputManager.ActiveSceneView = this;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _gizmoCamera.gameObject.SetActive(false);
            _flyCamera.enabled = false;
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
                    && gb_ToolManager.Instance.SelectedObject)
                {
                    Focus(gb_ToolManager.Instance.SelectedObject.transform);
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
                if (gb_Binds.JustDown(gb_Bind.IncreaseGrid))
                {
                    GridSize /= 2f;
                    GridSize = Mathf.Max(GridSize, 1);
                }

                if (gb_Binds.JustDown(gb_Bind.ReduceGrid))
                {
                    GridSize *= 2f;
                    GridSize = Mathf.Min(GridSize, 512);
                }

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
            var worldPoint = Camera.ViewportToWorldPoint(point);
            return worldPoint - Camera.transform.position;
        }

        public Vector3 SceneToWorld(Vector3 screenPos)
        {
            return Camera.ScreenToWorldPoint(screenPos) - Camera.transform.position;
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

