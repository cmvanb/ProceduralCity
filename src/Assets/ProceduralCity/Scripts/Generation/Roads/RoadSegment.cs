using System.Collections.Generic;
using UnityEngine;
using AltSrc.UnityCommon.Math;
using AltSrc.UnityCommon.DataStructures;

namespace AltSrc.ProceduralCity.Generation.Roads
{
    public class RoadSegment : IBounds
    {
        public LineSegment2D LineSegment2D { get; set; }
        // TODO: Consider refactoring this variable out into CityGenerator. -Casper 2017-08-31
        /// <summary>
        ///   The closer Priority is to 0, the sooner this segment will be popped out of the queue
        ///   and used in CityGenerator.cs.
        /// </summary>
        public int Priority { get; set; }
        public RoadType RoadType { get; private set; }
        public bool HasBeenSplit { get; set; }
        public List<RoadSegment> LinksForward { get; set; }
        public List<RoadSegment> LinksBackward { get; set; }

        public Vector2 PointA { get { return this.LineSegment2D.PointA; } }
        public Vector2 PointB { get { return this.LineSegment2D.PointB; } }
        public Rect Bounds { get { return this.LineSegment2D.Bounds; } }


        public RoadSegment(
            Vector2 pointA,
            Vector2 pointB,
            int priority,
            RoadType roadType = RoadType.Normal,
            bool hasBeenSplit = false)
        {
            this.LineSegment2D = new LineSegment2D(pointA, pointB);
            this.Priority = priority;
            this.RoadType = roadType;
            this.HasBeenSplit = hasBeenSplit;
            this.LinksForward = new List<RoadSegment>();
            this.LinksBackward = new List<RoadSegment>();
        }

        public RoadSegment Split(Vector2 point, RoadSegment other)
        {
            // perform split
            RoadSegment splitSegment = RoadSegment.FromExisting(this);
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

                    if (index != -1)
                    {
                        link.LinksForward[index] = splitSegment;
                    }
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

            return splitSegment;
        }

        public bool StartIsBackwards()
        {
            if (this.LinksBackward.Count > 0)
            {
                return this.LinksBackward[0].PointA == this.PointA
                    || this.LinksBackward[0].PointB == this.PointA;
            }
            else if (this.LinksForward.Count > 0)
            {
                return this.LinksForward[0].PointA == this.PointB
                    || this.LinksForward[0].PointB == this.PointB;
            }

            return false;
        }

        public override string ToString()
        {
            return 
                this.PointA.ToString() + " to " + this.PointB.ToString() 
                + ", L: " + this.LineSegment2D.Length 
                + ", D: " + this.LineSegment2D.DirectionInDegrees
                + ", R: " + this.RoadType;
        }

        public static void ClearAllLinksToRoadSegment(RoadSegment s)
        {
            for (int i = 0; i < s.LinksBackward.Count; ++i)
            {
                RoadSegment link = s.LinksBackward[i];

                if (link.LinksForward.Contains(s))
                {
                    link.LinksForward.Remove(s);
                }
                if (link.LinksBackward.Contains(s))
                {
                    link.LinksBackward.Remove(s);
                }

                s.LinksBackward.Remove(link);
            }

            for (int i = 0; i < s.LinksForward.Count; ++i)
            {
                RoadSegment link = s.LinksForward[i];

                if (link.LinksForward.Contains(s))
                {
                    link.LinksForward.Remove(s);
                }
                if (link.LinksBackward.Contains(s))
                {
                    link.LinksBackward.Remove(s);
                }

                s.LinksForward.Remove(link);
            }
        }

        /// <summary>
        ///   Construct a new road segment identical to an existing road segment.
        ///       @param existingSegment - the road segment to copy
        ///   NOTE: The new segment won't copy over the links arrays, these must be manually populated.
        /// </summary>
        public static RoadSegment FromExisting(RoadSegment existingSegment)
        {
            return new RoadSegment(
                existingSegment.PointA,
                existingSegment.PointB,
                existingSegment.Priority,
                existingSegment.RoadType,
                existingSegment.HasBeenSplit);
        }

        /// <summary>
        ///   Construct a new road segment from a point, direction and length.
        ///       @param pointA - the first point in the new road segment
        ///       @param angle - the direction in degrees
        ///       @param length - the length of the new road segment
        ///       @param priority - the priority of the road segment in CityGenerator.cs
        ///       @param roadType - determines how this road will be rendered
        /// </summary>
        public static RoadSegment UsingDirection(
            Vector2 pointA,
            float angle,
            float length,
            int priority,
            RoadType roadType,
            bool hasBeenSplit)
        {
            Vector2 pointB = pointA + Vector2Utils.FromAngleInDegreesAndMagnitude(angle, length);

            return new RoadSegment(
                pointA,
                pointB,
                priority,
                roadType,
                hasBeenSplit);
        }
    }
}
