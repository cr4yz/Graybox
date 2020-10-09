using Graybox.Gui;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox
{
    public class gb_GuiElement : MonoBehaviour
    {

        private void Awake()
        {
            gb_GuiManager.Instance.Add(this);
        }

        private void OnDestroy()
        {
            gb_GuiManager.Instance.Remove(this);
        }

    }
}

