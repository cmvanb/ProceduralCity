using UnityEngine;

namespace AltSrc.UnityCommon.Math
{
    public static class Vector3Extensions
    {
        public static Vector2 ToVec2XY(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector2 ToVec2XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }
    }
}


