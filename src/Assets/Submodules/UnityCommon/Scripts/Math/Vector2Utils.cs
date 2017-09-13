using UnityEngine;

namespace AltSrc.UnityCommon.Math
{
    public static class Vector2Utils
    {
        public static Vector2 FromAngleMagnitude(float angle, float magnitude)
        {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * magnitude;
        }
    }
}

