using UnityEngine;
using UnityEngine.UI;

namespace Graybox.Gui
{
    public class gb_ShortcutField : gb_FormField
    {

        public gb_Shortcut Shortcut;

        [SerializeField]
        private Button _shortcutButton;

        private bool _isHot;

        private KeyCode[] _modifierKeys = new KeyCode[]
        {
            KeyCode.LeftControl,
            KeyCode.LeftShift,
            KeyCode.LeftAlt
        };

        private void Start()
        {
            _shortcutButton.onClick.AddListener(SetHot);
            _shortcutButton.GetComponentInChildren<Text>().text = Shortcut.KeysAsString();
        }

        private void Update()
        {
            if (_isHot)
            {
                if(Input.GetKeyDown(KeyCode.Escape)
                    || Input.GetKeyDown(KeyCode.Mouse0)
                    || Input.GetKeyDown(KeyCode.Mouse1))
                {
                    UnsetHot();
                }
            }
        }

        private void UnsetHot()
        {

        }

        private void SetHot()
        {

        }

    }
}

