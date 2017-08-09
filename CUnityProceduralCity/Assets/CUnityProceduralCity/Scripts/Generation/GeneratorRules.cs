using System;
using UnityEngine;

namespace CUnityProceduralCity
{
    public enum CenterShape
    {
        O,
        X,
        Y
    };

    // TODO: Refactor this struct to a ScriptableObject for flexibility. -Casper 2017-08-01

    [Serializable]
    public struct GeneratorRules
    {
        public string CityName;
        public Vector3 CenterPosition;
        public CenterShape CenterShape;
        public float CenterAngle;
        public float CityScale;

        public GeneratorRules(
            string cityName,
            Vector3 centerPosition,
            CenterShape centerShape,
            float centerAngle,
            float cityScale)
        {
            this.CityName = cityName;
            this.CenterPosition = centerPosition;
            this.CenterShape = centerShape;
            this.CenterAngle = centerAngle;
            this.CityScale = cityScale;
        }
    }
}
