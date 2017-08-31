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
        // TODO: Consider refactoring this variable out into CityGenerator. -Casper 2017-08-31
        public int Priority { get; set; }
        public bool HasBeenSplit { get; set; }
        public LineSegment2D LineSegment2D { get; set; }
        public List<RoadSegment> LinksForward { get; set; }
        public List<RoadSegment> LinksBackward { get; set; }

        public Vector2 PointA { get { return this.LineSegment2D.PointA; } }
        public Vector2 PointB { get { return this.LineSegment2D.PointB; } }
        public Rect Bounds { get { return this.LineSegment2D.Bounds; } }

        public RoadType RoadType { get; private set; }

        public RoadSegment(
            Vector2 pointA,
            Vector2 pointB,
            int priority,
            RoadType roadType = RoadType.Normal,
            bool hasBeenSplit = false)
        {
            this.LineSegment2D = new LineSegment2D(pointA, pointB);
            this.Priority = priority;
            this.LinksForward = new List<RoadSegment>();
            this.LinksBackward = new List<RoadSegment>();
            this.RoadType = roadType;
            this.HasBeenSplit = hasBeenSplit;
        }

        public void Split(Vector2 point, RoadSegment other, out RoadSegment splitSegment)
        {
            // perform split
            splitSegment = RoadSegment.FromExisting(this);
            splitSegment.LineSegment2D = new LineSegment2D(this.PointA, point);
            this.LineSegment2D = new LineSegment2D(point, this.PointB);

            // links are not copied by RoadSegment.FromExisting(), so copy them over into new lists
            splitSegment.LinksBackward = new List<RoadSegment>(this.LinksBackward);
            splitSegment.LinksForward = new List<RoadSegment>(this.LinksForward);

            // figure out which links correspond to which end of the split segment
            bool startIsBackwards = StartIsBackwards();
            var firstSplit = startIsBackwards ? splitSegment : this;
            var secondSplit = startIsBackwards ? this : splitSegment;
            var linksToFix = startIsBackwards ? splitSegment.LinksBackward : splitSegment.LinksForward;

            foreach (RoadSegment link in linksToFix)
            {
                int index = link.LinksBackward.IndexOf(this);

                if (index != -1)
                {
                    link.LinksBackward[index] = splitSegment;
                }
                else
                {
                    index = link.LinksForward.IndexOf(this);
                    link.LinksForward[index] = splitSegment;
                }
            }

            firstSplit.LinksForward = new List<RoadSegment>();
            firstSplit.LinksForward.Add(other);
            firstSplit.LinksForward.Add(secondSplit);

            secondSplit.LinksBackward = new List<RoadSegment>();
            secondSplit.LinksBackward.Add(other);
            secondSplit.LinksBackward.Add(firstSplit);

            other.LinksForward.Add(firstSplit);
            other.LinksForward.Add(secondSplit);
        }

        private bool StartIsBackwards()
        {
            if (this.LinksBackward.Count > 0)
            {
                return this.LinksBackward[0].PointA == this.PointA
                    || this.LinksBackward[0].PointB == this.PointA;
            }

            return this.LinksForward[0].PointA == this.PointB
                || this.LinksForward[0].PointB == this.PointB;
        }

        public static RoadSegment FromExisting(RoadSegment existingSegment)
        {
            return new RoadSegment(
                existingSegment.PointA,
                existingSegment.PointB,
                existingSegment.Priority,
                existingSegment.RoadType,
                existingSegment.HasBeenSplit);
        }
    }
}
