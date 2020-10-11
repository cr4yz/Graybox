using Graybox.In;
using UnityEngine;
using UnityEngine.Events;

namespace Graybox.Tools
{
    public class gb_ToolHandle : MonoBehaviour
    {

        public UnityEvent OnHandleDown = new UnityEvent();
        public UnityEvent OnHandleUp = new UnityEvent();

        public bool IsMouseDown { get; private set; }
        public bool IsHovered { get; private set; }
        public Vector3 HitPoint { get; private set; }

        [SerializeField]
        private Material _highlightMaterial;
        [SerializeField]
        private Renderer _highlightRenderer;

        private Material _defaultMaterial;
        private bool _highlighted;
        private static gb_ToolHandle _activeHandle;

        private void Awake()
        {
            if (_highlightRenderer)
            {
                _defaultMaterial = _highlightRenderer.material;
            }
            else
            {
                _defaultMaterial = new Material(Shader.Find("Standard"));
            }
        }

        protected virtual void Update()
        {
            var shouldBeHighlighted = (IsMouseDown || IsHovered) && (_activeHandle == this || _activeHandle == null);
            if (shouldBeHighlighted != _highlighted)
            {
                _highlighted = shouldBeHighlighted;
                _highlightRenderer.material = _highlighted
                    ? _highlightMaterial
                    : _defaultMaterial;
            }

            IsHovered = IsMouseOver();

            if (IsHovered)
            {
                if (gb_Binds.JustDown(gb_Bind.Select))
                {
                    _activeHandle = this;
                    IsMouseDown = true;
                    OnHandleDown.Invoke();
                }
            }

            if (gb_Binds.JustUp(gb_Bind.Select) && IsMouseDown)
            {
                _activeHandle = null;
                IsMouseDown = false;
                OnHandleUp.Invoke();
            }
        }

        private bool IsMouseOver()
        {
            foreach (var hit in gb_InputManager.HitsUnderCursor)
            {
                if (hit.transform == transform)
                {
                    HitPoint = hit.point;
                    return true;
                }
            }
            return false;
        }

    }
}

