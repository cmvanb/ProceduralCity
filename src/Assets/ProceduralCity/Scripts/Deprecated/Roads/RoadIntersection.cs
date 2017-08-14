using System.Collections.Generic;
using UnityEngine;
using CUnity.Common.DataStructures;

namespace CUnity.ProceduralCity.Deprecated
{
    public class RoadIntersection
    {
        public int SegmentsCount
        {
            get
            {
                return this.tuples.Count;
            }
        }

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
