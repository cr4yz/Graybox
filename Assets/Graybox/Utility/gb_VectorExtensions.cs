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

        public static Vector3 Snap(this Vector3 vector, Vector3 axis, float snapValue)
        {
            return new Vector3(axis.x != 0 ? Mathf.Round(vector.x / snapValue) * snapValue : vector.x,
                              axis.y != 0 ? Mathf.Round(vector.y / snapValue) * snapValue : vector.y,
                              axis.z != 0 ? Mathf.Round(vector.z / snapValue) * snapValue : vector.z);
        }

        public static Vector3 Absolute(this Vector3 vector)
        {
            return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
        }

        public static Vector2 Absolute(this Vector2 vector)
        {
            return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }

        public static bool IsParallel(Vector3 direction, Vector3 otherDirection, float precision = .0001f)
        {
            return Vector3.Cross(direction, otherDirection).sqrMagnitude < precision;
        }

        public static float MagnitudeInDirection(this Vector3 vector, Vector3 direction, bool normalizeParameters = true)
        {
            if (normalizeParameters) direction.Normalize();
            return Vector3.Dot(vector, direction);
        }
    }
}
