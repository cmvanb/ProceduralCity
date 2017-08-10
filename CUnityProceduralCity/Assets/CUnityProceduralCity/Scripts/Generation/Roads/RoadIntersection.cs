using System.Collections.Generic;
using UnityEngine;

namespace CUnity.ProceduralCity.Generation
{
    // TODO: See if you can refactor this class into a simple List<Vector2>. -Casper 2017-08-09
    public class RoadIntersection
    {
        /*
        public List<Vector2> Points { get; private set; }

        // NOTE: DEPRECATED -Casper 2017-08-10
        public RoadIntersection(List<Vector2> points)
        {
            this.Points = points;
        }
        */

        public int SegmentsCount
        {
            get
            {
                return this.segments.Count;
            }
        }

        protected List<RoadSegment> segments = new List<RoadSegment>();

        protected List<bool> attachPointBs = new List<bool>();

        public void AddSegment(RoadSegment segment, bool attachPointB = false)
        {
            /*
            Debug.Assert(
                0 <= pointIndex && pointIndex <= 1,
                "pointIndex [" + pointIndex + "] is out of range.");
            */

            segments.Add(segment);
            attachPointBs.Add(attachPointB);
        }

        public Vector2 GetSegmentPoint(int segmentIndex, bool getOtherPoint = false)
        {
            RoadSegment segment = this.segments[segmentIndex];

            bool returnPointB = this.attachPointBs[segmentIndex] != getOtherPoint;

            return returnPointB ? segment.PointB : segment.PointA;
        }
    }
}
