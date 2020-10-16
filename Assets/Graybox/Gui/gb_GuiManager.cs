using System.Collections.Generic;

namespace Graybox.Gui
{
    public class gb_GuiManager : gb_Singleton<gb_GuiManager>
    {

        private List<gb_GuiElement> _guiElements = new List<gb_GuiElement>();

        public bool GuiHasFocus()
        {
            foreach(var element in _guiElements)
            {
                if (element.BlocksInput)
                {
                    return true;
                }
            }
            return false;
        }

        public bool GuiInputHasFocus()
        {
            foreach (var element in _guiElements)
            {
                if (element.InputHasFocus)
                {
                    return true;
                }
            }
            return false;
        }

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

