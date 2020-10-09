using UnityEngine;

namespace Graybox.Utility
{
    public static class gb_VectorExtensions
    {
        public static Vector3 Snap(this Vector3 vector, float snapValue)
        {
            return new Vector3(Mathf.Round(vector.x / snapValue) * snapValue,
                              Mathf.Round(vector.y / snapValue) * snapValue,
                              Mathf.Round(vector.z / snapValue) * snapValue);
        }
        public static Vector3 Absolute(this Vector3 vector)
        {
            return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
        }
        public static Vector2 Absolute(this Vector2 vector)
        {
            return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }
    }
}
