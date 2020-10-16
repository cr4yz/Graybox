using Graybox.Gui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Graybox
{
    public class gb_GuiElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool BlocksInput => gameObject.activeSelf && (MouseIsOver || InputHasFocus);

        public bool MouseIsOver { get; private set; }
        public bool InputHasFocus
        {
            get
            {
                foreach(var inputField in _inputFields)
                {
                    if (inputField.isFocused)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private InputField[] _inputFields;

        private void Awake()
        {
            gb_GuiManager.Instance.Add(this);
        }

        private void OnEnable()
        {
            _inputFields = GetComponentsInChildren<InputField>();
        }

        private void OnDestroy()
        {
            if (gb_GuiManager.Instance)
            {
                gb_GuiManager.Instance.Remove(this);
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            MouseIsOver = true;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            MouseIsOver = false;
        }

    }
}

