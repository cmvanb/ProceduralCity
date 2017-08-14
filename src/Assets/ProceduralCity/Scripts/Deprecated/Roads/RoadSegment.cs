using UnityEngine;
using CUnity.Common.Math;

namespace CUnity.ProceduralCity.Deprecated
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

        public int Level { get; private set; }
        public LineSegment2D LineSegment2D { get; private set; }

        public RoadSegment(LineSegment2D segment, int level)
        {
            this.LineSegment2D = segment;

            this.Level = level;
        }

        public RoadSegment(Vector2 pointA, Vector2 pointB, int level)
        {
            this.LineSegment2D = new LineSegment2D(pointA, pointB);

            //this.PointA = pointA;
            //this.PointB = pointB;
            this.Level = level;
        }

        // TODO: Refactor this method to LineSegment2D. -Casper 2017-08-09
        public bool IsEqual(RoadSegment segment)
        {
            // NOTE: Should we perhaps be testing for approximate equality? For Unity.Vector2, that
            // can be accomplished with the == operator. -Casper 2017-08-09
            if (this.PointA.Equals(segment.PointA)
                && this.PointB.Equals(segment.PointB))
            {
                return true;
            }
            else if (this.PointA.Equals(segment.PointB)
                && this.PointB.Equals(segment.PointA))
            {
                return true;
            }

            return false;
        }

        // TODO: Refactor this method to LineSegment2D. -Casper 2017-08-09
        public float CalculateLength()
        {
            return Vector2.Distance(this.PointA, this.PointB);
        }
    }
}
