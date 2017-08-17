using System.Collections.Generic;
using UnityEngine;
using AltSrc.UnityCommon.Math;
using AltSrc.UnityCommon.DataStructures;

namespace AltSrc.ProceduralCity.Generation.Roads
{
    public class RoadSegment : IBounds
    {
        /// <summary>
        ///   The closer Priority is to 0, the sooner this segment will be popped out of the queue
        ///   and used in CityGenerator.cs.
        /// </summary>
        public int Priority { get; set; }

        public Vector2 PointA { get { return this.LineSegment2D.PointA; } }
        public Vector2 PointB { get { return this.LineSegment2D.PointB; } }
        public Rect Bounds { get { return this.LineSegment2D.Bounds; } }

        public LineSegment2D LineSegment2D { get; private set; }
        public List<RoadSegment> LinksForward { get; private set; }
        public List<RoadSegment> LinksBackward { get; private set; }
        public RoadType RoadType { get; private set; }

        public RoadSegment(
            Vector2 pointA,
            Vector2 pointB,
            int priority,
            RoadType roadType = RoadType.Normal)
        {
            this.LineSegment2D = new LineSegment2D(pointA, pointB);
            this.Priority = priority;
            this.LinksForward = new List<RoadSegment>();
            this.LinksBackward = new List<RoadSegment>();
            this.RoadType = roadType;
        }

        public static RoadSegment FromExisting(RoadSegment existingSegment)
        {
            return new RoadSegment(
                existingSegment.PointA,
                existingSegment.PointB,
                existingSegment.Priority,
                existingSegment.RoadType);
        }
    }
}
