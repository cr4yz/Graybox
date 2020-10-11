using Graybox.In;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graybox.Tools
{
    public class gb_ToolManager : gb_Singleton<gb_ToolManager>
    {

        private gb_Tool _activeTool;
        public gb_Tool[] Tools { get; private set; }

        private void Awake()
        {
            Tools = GetComponentsInChildren<gb_Tool>();
            foreach(var tool in Tools)
            {
                tool.gameObject.SetActive(false);
            }
            SetActiveTool(Tools[0]);
        }

        public bool ToolHasFocus()
        {
            foreach(var tool in Tools)
            {
                if(tool.gameObject.activeSelf && tool.HasFocus)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetActiveTool(gb_Tool tool)
        {
            if (_activeTool)
            {
                _activeTool.gameObject.SetActive(false);
                _activeTool = null;
            }
            if (tool)
            {
                _activeTool = tool;
                _activeTool.gameObject.SetActive(true);
            }
        }

    }
}

