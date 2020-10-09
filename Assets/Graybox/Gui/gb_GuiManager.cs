using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox.Gui
{
    public class gb_GuiManager : gb_Singleton<gb_GuiManager>
    {

        private List<gb_GuiElement> _guiElements = new List<gb_GuiElement>();

        public void Add(gb_GuiElement guiElement)
        {
            _guiElements.Add(guiElement);
        }

        public void Remove(gb_GuiElement guiElement)
        {
            _guiElements.Remove(guiElement);
        }

    }
}

