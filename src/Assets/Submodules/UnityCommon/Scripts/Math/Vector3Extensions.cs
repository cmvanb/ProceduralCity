using UnityEngine;

namespace AltSrc.UnityCommon.Math
{
    public static class Vector3Extensions
    {
        public static Vector3 SetX(this Vector3 v, float x)
        {
            v.Set(x, v.y, v.z);
            return v;
        }

        public static Vector3 SetY(this Vector3 v, float y)
        {
            v.Set(v.x, y, v.z);
            return v;
        }

        public static Vector3 SetZ(this Vector3 v, float z)
        {
            v.Set(v.x, v.y, z);
            return v;
        }

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


