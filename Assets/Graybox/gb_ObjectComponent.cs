using UnityEngine;

namespace Graybox
{
    public class gb_ObjectComponent : MonoBehaviour
    {
        public gb_Object Object { get; set; }

        private void OnDestroy()
        {
            if (Object.Map)
            {
                Object.Map.MapInfo.Objects.Remove(Object);
            }
        }
    }
}

