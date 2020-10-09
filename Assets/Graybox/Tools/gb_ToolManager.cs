using Graybox.In;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graybox.Tools
{
    public class gb_ToolManager : gb_Singleton<gb_ToolManager>
    {

        private gb_Tool _activeTool;

        public gb_ObjectComponent SelectedObject { get; private set; }
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

        private void LateUpdate()
        {
            TryPickObject();
        }

        public bool ToolHasFocus()
        {
            return _activeTool.HasFocus;
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

        private void TryPickObject()
        {
            if(_activeTool && _activeTool.HasFocus)
            {
                return;
            }

            if (!Input.GetKeyDown(KeyCode.Mouse0))
            {
                return;
            }

            var potentialObjects = new List<gb_ObjectComponent>();

            foreach(var hit in gb_InputManager.HitsUnderCursor)
            {
                if (hit.transform.GetComponentInParent<gb_GizmoTool>())
                {
                    return;
                }
                if(hit.transform.TryGetComponent(out gb_ObjectComponent objComponent))
                {
                    potentialObjects.Add(objComponent);
                }
            }

            if (potentialObjects.Count() == 0)
            {
                SelectedObject = null;
            }
            else
            {
                if (SelectedObject != null)
                {
                    SelectedObject = potentialObjects.SkipWhile(x => x.transform.gameObject != SelectedObject)
                        .Skip(1)
                        .FirstOrDefault();
                }
                if (!SelectedObject)
                {
                    SelectedObject = potentialObjects.First();
                }
            }
        }

    }
}

