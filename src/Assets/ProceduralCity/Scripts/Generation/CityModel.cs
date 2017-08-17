using System.Collections.Generic;
using UnityEngine;
using AltSrc.ProceduralCity.Generation.Roads;

namespace AltSrc.ProceduralCity.Generation
{
    public class CityModel
    {
        public string CityName { get; private set; }

        public List<RoadSegment> RoadSegments { get; private set; }

        public CityModel()
        {
            Debug.Log("CityModel constructor");
        }
    }
}
