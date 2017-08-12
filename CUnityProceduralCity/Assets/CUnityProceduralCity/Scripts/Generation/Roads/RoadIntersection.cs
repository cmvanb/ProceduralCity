using System;
using System.Collections.Generic;
using UnityEngine;

namespace CUnity.ProceduralCity.Generation
{
    public class RoadIntersection
    {
        /*
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
            segments.Add(segment);
            attachPointBs.Add(attachPointB);
        }

        public Vector2 GetSegmentPoint(int segmentIndex, bool getOtherPoint = false)
        {
            RoadSegment segment = this.segments[segmentIndex];

            bool returnPointB = this.attachPointBs[segmentIndex] != getOtherPoint;

            return returnPointB ? segment.PointB : segment.PointA;
        }
        */

        protected List<Tuple<RoadSegment, bool>> tuples = new List<Tuple<RoadSegment, bool>>();

        public void AddSegment(RoadSegment segment, bool attachPointB = false)
        {
            tuples.Add(new Tuple<RoadSegment, bool>(segment, attachPointB));
        }

        public Vector2 GetSegmentPoint(int index, bool getOtherPoint = false)
        {
            RoadSegment segment = this.tuples[index].Item1;

            bool returnPointB = this.tuples[index].Item2 != getOtherPoint;

            return returnPointB ? segment.PointB : segment.PointA;
        }
    }
}
