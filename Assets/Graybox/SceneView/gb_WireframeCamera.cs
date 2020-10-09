using UnityEngine;

namespace Graybox
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class gb_WireframeCamera : MonoBehaviour
    {
        [Header("Replacement Shader")]
        public bool enableWireframe = true;
        public string replacementTag = "RenderType";
        public bool setCameraBackgroundColor = true;
        public Color backgroundColor = Color.black;

        [Header("Wireframe Shader Properties")]
        [Range(0, 0.5f)]
        public float wireThickness = 0.05f;
        public bool removeDiagonals = true;
        public Color frontColor = Color.green;
        public Color backColor = Color.gray;

        private Color _initialClearColor;
        private CameraClearFlags _initialClearFlag;
        private Camera _camera;
        private bool _wireframeEnabled;

        protected void OnEnable()
        {
            _camera = GetComponent<Camera>();
            _initialClearFlag = _camera.clearFlags;
            _initialClearColor = _camera.backgroundColor;
        }

        protected void OnDisable()
        {
            ResetCamera();
        }

        protected void Update()
        {
            Shader.SetGlobalFloat("_WireframeVal", wireThickness);
            Shader.SetGlobalColor("_FrontColor", frontColor);
            Shader.SetGlobalColor("_BackColor", backColor);
            Shader.SetGlobalFloat("_RemoveDiag", removeDiagonals ? 1.0f : 0f);

            if (enableWireframe && setCameraBackgroundColor)
            {
                _camera.backgroundColor = backgroundColor;
                _camera.clearFlags = CameraClearFlags.Depth;
            }

            ApplyShader();
        }

        private void ApplyShader()
        {
            if(_wireframeEnabled != enableWireframe)
            {
                _wireframeEnabled = enableWireframe;
                if (enableWireframe)
                {
                    _camera.SetReplacementShader(Shader.Find("Graybox/Wireframe"), replacementTag);
                }
                else
                {
                    ResetCamera();
                }
            }
        }

        private void ResetCamera()
        {
            _camera.ResetReplacementShader();
            _camera.backgroundColor = _initialClearColor;
            _camera.clearFlags = _initialClearFlag;
        }
    }

}

