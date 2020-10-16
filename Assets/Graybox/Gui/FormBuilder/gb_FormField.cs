using UnityEngine;
using UnityEngine.UI;

namespace Graybox.Gui
{
    public class gb_FormField : MonoBehaviour
    {

        [SerializeField]
        private Text _labelText;

        public string Label
        {
            get { return _labelText.text; }
            set { _labelText.text = value; }
        }

    }
}

