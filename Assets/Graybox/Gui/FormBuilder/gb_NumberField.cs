using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Graybox.Gui
{
    public class gb_NumberField : gb_FormField
    {

        [SerializeField]
        private InputField _numberInput;

        public UnityEvent<float> OnValueChanged = new UnityEvent<float>();
        public Func<float> ReadValue;

        public bool WholeNumbers
        {
            get { return _numberInput.contentType == InputField.ContentType.IntegerNumber; }
            set { _numberInput.contentType = value ? InputField.ContentType.IntegerNumber : InputField.ContentType.DecimalNumber; }
        }

        private void Start()
        {
            _numberInput.SetTextWithoutNotify(ReadValue.Invoke().ToString());
            _numberInput.onValueChanged.AddListener((value) =>
            {
                if(float.TryParse(value, out float newValue))
                {
                    OnValueChanged?.Invoke(newValue);
                }
            });
        }

    }
}

