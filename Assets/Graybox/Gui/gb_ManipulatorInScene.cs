using System.Collections.Generic;
using System.Linq;
using Graybox.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Graybox.Gui
{
    public class gb_ManipulatorInScene : gb_GuiElement
    {

        [SerializeField]
        private Button _extrudeButton;
        [SerializeField]
        private Button _extrudeSettingsButton;

        private gb_Manipulator _manipulatorTool;

        private void Start()
        {
            _manipulatorTool = GameObject.FindObjectOfType<gb_Manipulator>(true);
            _extrudeButton.onClick.AddListener(() =>
            {
                _manipulatorTool.ExtrudeSelection(gb_Settings.Instance.ExtrusionSize);
            });
            _extrudeSettingsButton.onClick.AddListener(() =>
            {
                gb_FormWindow.Instance
                    .Restart()
                    .WithField("Extrusion Amount",
                        value => gb_Settings.Instance.ExtrusionSize = value,
                        () => gb_Settings.Instance.ExtrusionSize, false)
                    .WithShortcut("Shortcut", gb_Binds.Shortcuts.First(x => x.Name == "Extrude"))
                    .WithTitle("Extrude Settings")
                    .AtCursor()
                    .Show();
            });
        }

        public void SetModeVertices()
        {
            _manipulatorTool.Mode = gb_ManipulatorMode.Vertices;
        }

        public void SetModeEdges()
        {
            _manipulatorTool.Mode = gb_ManipulatorMode.Edges;
        }

        public void SetModeFaces()
        {
            _manipulatorTool.Mode = gb_ManipulatorMode.Faces;
        }

        public void SetHandleMove()
        {
            _manipulatorTool.Handle = gb_ManipulatorHandle.Move;
        }

        public void SetHandleRotate()
        {
            _manipulatorTool.Handle = gb_ManipulatorHandle.Rotate;
        }

        public void SetHandleScale()
        {
            _manipulatorTool.Handle = gb_ManipulatorHandle.Scale;
        }

    }
}

