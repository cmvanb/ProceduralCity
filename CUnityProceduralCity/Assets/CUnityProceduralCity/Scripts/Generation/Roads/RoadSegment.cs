using UnityEngine;

namespace CUnityProceduralCity
{
    public class RoadSegment
    {
        public Vector2 PointA { get; private set; }
        public Vector2 PointB { get; private set; }
        public int Level { get; private set; }

        public RoadSegment(Vector2 pointA, Vector2 pointB, int level)
        {
            this.PointA = pointA;
            this.PointB = pointB;
            this.Level = level;
        }

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

        public float CalculateLength()
        {
            return Vector2.Distance(this.PointA, this.PointB);
        }
    }
}
