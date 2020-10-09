using UnityEngine;
using UnityEngine.UI;

namespace Graybox.Gui
{
    [RequireComponent(typeof(RectTransform))]
    public class gb_SceneLabel : MonoBehaviour
    {

        [SerializeField]
        private Text _text;
        private RectTransform _rt;

        [HideInInspector]
        public Vector2 ScreenOffset;
        [HideInInspector]
        public gb_SceneView SceneView;
        [HideInInspector]
        public Vector3 WorldPosition;

        private void Start()
        {
            _rt = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            var screenPos = SceneView.WorldToScreen(WorldPosition);
            _rt.anchoredPosition = SceneView.ScreenToRect(screenPos + ScreenOffset);
        }

        public void SetText(string text)
        {
            _text.text = text;
        }

        public void Destroy()
        {
            GameObject.Destroy(gameObject);
        }

    }
}

