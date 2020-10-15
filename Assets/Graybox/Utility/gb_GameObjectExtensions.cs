using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox.Utility
{
    public static class gb_GameObjectExtensions
    {

        public static void SetLayerRecursively(this GameObject gameObj, int layer)
        {
            gameObj.layer = layer;
            foreach(Transform tr in gameObj.transform)
            {
                SetLayerRecursively(tr.gameObject, layer);
            }
        }

    }
}

