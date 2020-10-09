using Graybox.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Graybox.Gui
{
    public class gb_ToolListButton : MonoBehaviour
    {

        [SerializeField]
        private Text _toolNameText;
        [SerializeField]
        private Button _toolButton;

        public void Initialize(gb_Tool tool)
        {
            _toolNameText.text = tool.ToolName;
            _toolButton.onClick.AddListener(() =>
            {
                gb_ToolManager.Instance.SetActiveTool(tool);
            });
        }

    }
}

