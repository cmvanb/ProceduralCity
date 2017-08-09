using System.Collections.Generic;
using UnityEngine;

namespace CUnity.ProceduralCity.Generation
{
    // TODO: See if you can refactor this class into a simple List<Vector2>. -Casper 2017-08-09
    public class RoadIntersection
    {
        public List<Vector2> Points { get; private set; }

        public RoadIntersection(List<Vector2> points)
        {
            this.Points = points;
        }
    }
}
