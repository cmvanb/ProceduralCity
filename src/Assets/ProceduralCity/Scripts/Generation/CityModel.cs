using System.Collections.Generic;
using UnityEngine;
using AltSrc.ProceduralCity.Generation.Roads;

namespace AltSrc.ProceduralCity.Generation
{
    public class CityModel
    {
        public string CityName { get; set; }

        public List<RoadSegment> RoadSegments { get; set; }

        public Texture2D PopulationHeatMap { get; set; }

        public CityModel()
        {
            Debug.Log("CityModel constructor");
        }
    }
}
