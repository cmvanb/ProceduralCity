using System.Collections.Generic;
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
    }

    public class RoadIntersection
    {
        public List<Vector2> Points { get; private set; }

        public RoadIntersection(List<Vector2> points)
        {
            this.Points = points;
        }
    }

    public class RoadsModel
    {
        public float Scale { get; private set; }
        public List<RoadSegment> Segments { get; private set; }
        public List<RoadIntersection> Intersections { get; private set; }

        public RoadsModel(float scale)
        {
            this.Scale = scale;
            this.Segments = new List<RoadSegment>();
            this.Intersections = new List<RoadIntersection>();
        }

        public void CreateCenter(CenterShape shape, Vector2 position, float angle)
        {
            if (shape == CenterShape.O)
            {
                CreateOShape(position);
            }
            else if (shape == CenterShape.X)
            {
                CreateXShape(position, angle);
            }
            else if (shape == CenterShape.Y)
            {
                CreateYShape(position, angle);
            }
        }

        public void CreateOShape(Vector2 position)
        {
            // TODO: Refactor this to take an angle parameter. -Casper 2017-08-01
            float angle = 360f / 8f;

            RoadSegment last = null;
            RoadSegment first = null;

            for (int i = 0; i < 8; i++)
            {
                Quaternion rotationA = Quaternion.Euler(0, 0, angle * i);
                Quaternion rotationB = Quaternion.Euler(0, 0, angle * (i + 1));

                Vector2 a = rotationA * (new Vector2(this.Scale / 2.5f, 0) + position);
                Vector2 b = rotationB * (new Vector2(this.Scale / 2.5f, 0) + position);

                RoadSegment segment = new RoadSegment(a, b, 0);

                this.Segments.Add(segment);

                if (first == null)
                {
                    first = segment;
                }

                if (last != null)
                {
                    RoadIntersection intersection = new RoadIntersection(
                        new List<Vector2>(){ segment.PointA, last.PointA });

                    this.Intersections.Add(intersection);
                }

                last = segment;
            }

            RoadIntersection finalIntersection = new RoadIntersection(
                new List<Vector2>(){ first.PointA, last.PointA });

            this.Intersections.Add(finalIntersection);
        }

        public void CreateXShape(Vector2 position, float angle)
        {
            throw new System.NotImplementedException();
        }

        public void CreateYShape(Vector2 position, float angle)
        {
            throw new System.NotImplementedException();
        }

        public void SplitSegments(int level = 0)
        {
            List<RoadSegment> segments = new List<RoadSegment>(this.Segments);

            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i].Level == level)
                {
                    SplitSegment(segments[i]);
                }
            }
        }

        // Protected methods.
        protected void SplitSegment(RoadSegment segment)
        {
            // TODO: Refactor out the cutoff and split distance variables. -Casper 2017-08-01

            const float shortCutoff = 5.5f;
            const float closeCutoff = 7.5f;

            // get split ratio
            float splitRatio = Random.Range(0.33f, 0.66f);

            // get split distance
            Vector3 p1 = new Vector3(segment.PointA.point.x, 0, segment.PointA.point.y);
            Vector3 p2 = new Vector3(segment.PointB.point.x, 0, segment.PointB.point.y);
            float length = Vector3.Distance(p1, p2);
            length *= splitRatio;

            // get direction vector for segment
            Vector3 direction = (p1 - p2).normalized;

            // get new point and patch the segment
            Vector3 newPoint = p2 + (direction * length);

            // calaculate other new point
            Vector3 per = Vector3.Cross(p1 - p2, Vector3.down).normalized;

            float newLength = this.scale / ((segment.Level + 1) * Random.Range(1f, 2f));
            Vector3 newPointEnd = newPoint + (per * newLength);

            // add new segment
            RoadSegment newSegment = new RoadSegment(
                new RoadPoint(new Vector2(newPoint.x, newPoint.z), null),
                new RoadPoint(new Vector2(newPointEnd.x, newPointEnd.z), null), segment.Level + 1);

            // calaculate other new point
            Vector3 perA = Vector3.Cross(p1 - p2, Vector3.down).normalized * -1;
            Vector3 newPointEndOther = newPoint + (perA * newLength);
            RoadSegment newSegmentOther = new RoadSegment(
                new RoadPoint(new Vector2(newPoint.x, newPoint.z), null),
                new RoadPoint(new Vector2(newPointEndOther.x, newPointEndOther.z), null), segment.Level + 1);

            // check what segments to add and add them
            bool seg1 = false;
            bool seg2 = false;

            bool with1 = this.SegmentWithin(newSegment, closeCutoff);
            bool with2 = this.SegmentWithin(newSegmentOther, closeCutoff);

            if (!with1)
            {
                Vector2 intersection = Vector3.zero;
                RoadSegment other = null;

                int iCount = segmentIntersection(newSegment, out intersection, out other, segment);

                if (iCount <= 1)
                {
                    this.Segments.RemoveAll(p => p.IsEqual(newSegment));
                    this.Segments.Add(newSegment);
                    seg1 = true;
                }

                if (iCount == 1)
                {
                    RoadSegment[] segmentsA = this.patchSegment(other, new RoadPoint(intersection, other));
                    RoadSegment[] segmentsB = this.patchSegment(newSegment, new RoadPoint(intersection, newSegment));

                    //kill very short dead-ends
                    bool sa = segmentsA[0].SegmentLength() > shortCutoff;
                    bool sb = segmentsA[1].SegmentLength() > shortCutoff;
                    bool sc = segmentsB[0].SegmentLength() > shortCutoff;
                    bool sd = segmentsB[1].SegmentLength() > shortCutoff;

                    List<RoadPoint> points = new List<RoadPoint>();

                    if (sa)
                    {
                        points.Add(segmentsA[0].PointB);
                    }
                    else
                    {
                        this.Segments.RemoveAll(p => p.IsEqual(segmentsA[0]));
                    }

                    if (sb)
                    {
                        points.Add(segmentsA[1].PointB);
                    }
                    else
                    {
                        this.Segments.RemoveAll(p => p.IsEqual(segmentsA[1]));
                    }

                    if (sc)
                    {
                        points.Add(segmentsB[0].PointB);
                    }
                    else
                    {
                        this.Segments.RemoveAll(p => p.IsEqual(segmentsB[0]));
                    }

                    if (sd)
                    {
                        points.Add(segmentsB[1].PointB);
                    }
                    else
                    {
                        this.Segments.RemoveAll(p => p.IsEqual(segmentsB[1]));
                    }

                    Intersection inter = new Intersection(points);
                    this.RoadIntersections.Add(inter);
                }
            }

            //other side of intersection
            if (!with2)
            {
                Vector2 intersection = Vector3.zero;
                RoadSegment other = null;

                int iCount = segmentIntersection(newSegmentOther, out intersection, out other, segment);

                if (iCount <= 1)
                {
                    this.Segments.RemoveAll(p => p.IsEqual(newSegmentOther));
                    this.Segments.Add(newSegmentOther);
                    seg2 = true;
                }

                if (iCount == 1)
                {
                    RoadSegment[] segmentsA = this.patchSegment(other, new RoadPoint(intersection, other));
                    RoadSegment[] segmentsB = this.patchSegment(newSegmentOther, new RoadPoint(intersection, newSegmentOther));

                    //kill very short dead-ends
                    bool sa = segmentsA[0].SegmentLength() > shortCutoff;
                    bool sb = segmentsA[1].SegmentLength() > shortCutoff;
                    bool sc = segmentsB[0].SegmentLength() > shortCutoff;
                    bool sd = segmentsB[1].SegmentLength() > shortCutoff;

                    List<RoadPoint> points = new List<RoadPoint>();

                    if (sa)
                    {
                        points.Add(segmentsA[0].PointB);
                    }
                    else
                    {
                        this.Segments.RemoveAll(p => p.IsEqual(segmentsA[0]));
                    }

                    if (sb)
                    {
                        points.Add(segmentsA[1].PointB);
                    }
                    else
                    {
                        this.Segments.RemoveAll(p => p.IsEqual(segmentsA[1]));
                    }

                    if (sc)
                    {
                        points.Add(segmentsB[0].PointB);
                    }
                    else
                    {
                        this.Segments.RemoveAll(p => p.IsEqual(segmentsB[0]));
                    }

                    if (sd)
                    {
                        points.Add(segmentsB[1].PointB);
                    }
                    else
                    {
                        this.Segments.RemoveAll(p => p.IsEqual(segmentsB[1]));
                    }

                    Intersection inter = new Intersection(points);
                    this.RoadIntersections.Add(inter);
                }
            }

            if (seg1 || seg2)
            {
                RoadSegment[] segments = this.patchSegment(
                    segment,
                    new RoadPoint(new Vector2(newPoint.x, newPoint.z),
                    segment));

                if (seg1 && seg2)
                {
                    Intersection inter = new Intersection(
                        new List<RoadPoint>{
                            segments[0].PointB,
                            segments[1].PointB,
                            newSegment.PointA,
                            newSegmentOther.PointA });

                    this.RoadIntersections.Add(inter);
                }
                else if (seg1)
                {
                    Intersection inter = new Intersection(
                        new List<RoadPoint>{
                            segments[0].PointB,
                            segments[1].PointB,
                            newSegment.PointA });

                    this.RoadIntersections.Add(inter);
                }
                else if (seg2)
                {
                    Intersection inter = new Intersection(
                        new List<RoadPoint>{
                            segments[0].PointB,
                            segments[1].PointB,
                            newSegmentOther.PointA });

                    this.RoadIntersections.Add(inter);
                }
            }
        }
    }
}
