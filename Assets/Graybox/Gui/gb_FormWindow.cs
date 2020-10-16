using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Graybox.Gui
{
    public class gb_FormWindow : gb_Singleton<gb_FormWindow>
    {

        [SerializeField]
        private Text _titleText;
        [SerializeField]
        private RectTransform _container;
        [SerializeField]
        private gb_ShortcutField _shortcutTemplate;
        [SerializeField]
        private gb_NumberField _numberFieldTemplate;

        private Vector3 _defaultPosition;
        private List<GameObject> _clones = new List<GameObject>();

        private void Awake()
        {
            _defaultPosition = GetComponent<RectTransform>().anchoredPosition;
            _numberFieldTemplate.gameObject.SetActive(false);
            _shortcutTemplate.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            Restart();
        }

        public gb_FormWindow WithTitle(string title)
        {
            _titleText.text = title;
            return this;
        }

        public gb_FormWindow WithField(string label, UnityAction<float> assignment, Func<float> readValue, bool wholeNumbers)
        {
            var clone = GameObject.Instantiate(_numberFieldTemplate, _numberFieldTemplate.transform.parent);
            clone.gameObject.SetActive(true);
            clone.Label = label;
            clone.WholeNumbers = wholeNumbers;
            clone.ReadValue = readValue;
            clone.OnValueChanged.AddListener(assignment);
            _clones.Add(clone.gameObject);
            return this;
        }

        public gb_FormWindow AtCursor()
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y - Screen.height);
            return this;
        }

        public gb_FormWindow WithShortcut(string label, gb_Shortcut shortcut)
        {
            var clone = GameObject.Instantiate(_shortcutTemplate, _shortcutTemplate.transform.parent);
            clone.gameObject.SetActive(true);
            clone.Label = label;
            clone.Shortcut = shortcut;
            _clones.Add(clone.gameObject);
            return this;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public gb_FormWindow Restart()
        {
            GetComponent<RectTransform>().anchoredPosition = _defaultPosition;
            _titleText.text = "Settings";
            foreach (var clone in _clones)
            {
                GameObject.Destroy(clone);
            }
            return this;
        }

    }
}

