using UnityEngine;
using AltSrc.UnityCommon.Math;

namespace AltSrc.ProceduralCity.Generation.Roads
{
    public class RoadSegment
    {
        public Vector2 PointA
        {
            get
            {
                return this.LineSegment2D.PointA;
            }
        }

        public Vector2 PointB
        {
            get
            {
                return this.LineSegment2D.PointB;
            }
        }

        public LineSegment2D LineSegment2D { get; private set; }

        /// <summary>
        ///   The closer Priority is to 0, the sooner this segment will be popped out of the queue
        ///   and used.
        /// </summary>
        public int Priority { get; private set; }

        public RoadType RoadType { get; private set; }

        public RoadSegment(
            Vector2 pointA,
            Vector2 pointB,
            int priority,
            RoadType roadType = RoadType.Normal)
        {
            this.LineSegment2D = new LineSegment2D(pointA, pointB);

            this.Priority = priority;

            this.RoadType = roadType;
        }
    }
}
