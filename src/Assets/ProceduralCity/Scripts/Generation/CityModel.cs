using System.Collections.Generic;
using UnityEngine;
using AltSrc.UnityCommon.DataStructures;
using AltSrc.ProceduralCity.Generation.Roads;

namespace AltSrc.ProceduralCity.Generation
{
    public class CityModel
    {
        public string CityName { get; set; }
        public Rect CityBounds { get; set; }
        public Texture2D PopulationHeatMap { get; set; }
        public List<RoadSegment> RoadSegments { get; set; }
        public QuadTree<RoadSegment> QuadTree { get; set; }

        public CityModel()
        {
            Debug.Log("CityModel constructor");
        }

        public float GetPopulationAt(Vector2 position)
        {
            if (CityBounds.Contains(position))
            {
                Vector2 texturePosition = new Vector2(
                    (position.x / CityBounds.width) * PopulationHeatMap.width,
                    (position.y / CityBounds.height) * PopulationHeatMap.height);

                return PopulationHeatMap.GetPixel((int)texturePosition.x, (int)texturePosition.y).r;
            }

            return 0f;
        }

        public float CalculatePopulationForRoad(RoadSegment segment)
        {
            return (GetPopulationAt(segment.PointA) + GetPopulationAt(segment.PointB)) / 2f;
        }
    }
}
