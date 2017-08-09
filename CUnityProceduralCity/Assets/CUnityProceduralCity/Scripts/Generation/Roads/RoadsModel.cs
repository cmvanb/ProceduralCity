using System.Collections.Generic;
using UnityEngine;
using CUnity.Common.Math;

namespace CUnity.ProceduralCity.Generation
{
    // TODO: See if you can refactor this class into a simple List<Vector2>. -Casper 2017-08-09
    public class RoadIntersection
    {
        public List<Vector2> Points { get; private set; }

        public RoadIntersection(List<Vector2> points)
        {
            this.Points = points;
        }
    }

    // TODO: Consider adding safeguards/defensive programming to ensure this class is used properly. -Casper 2017-08-09
    public class RoadsModel
    {
        public float Scale { get; private set; }
        public float RoadTextureTiling { get; private set; }
        public float RoadWidth { get; private set; }
        public float IntersectionOffset { get; private set; }

        public List<RoadSegment> Segments { get; private set; }
        public List<RoadIntersection> Intersections { get; private set; }

        public RoadsModel(GeneratorRules rules)
        {
            this.Segments = new List<RoadSegment>();
            this.Intersections = new List<RoadIntersection>();

            this.Scale = rules.CityScale;
            this.RoadTextureTiling = rules.RoadTextureTiling;

            // TODO: Consider making these configurable? Part of GeneratorRules? -Casper 2017-08-09
            this.RoadWidth = rules.CityScale * 1.0f;
            this.IntersectionOffset = rules.CityScale * 0.5f;
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
            Vector3 p1 = new Vector3(segment.PointA.x, 0, segment.PointA.y);
            Vector3 p2 = new Vector3(segment.PointB.x, 0, segment.PointB.y);
            float length = Vector3.Distance(p1, p2);
            length *= splitRatio;

            // get direction vector for segment
            Vector3 direction = (p1 - p2).normalized;

            // get new point and patch the segment
            Vector3 newPoint = p2 + (direction * length);

            // calaculate other new point
            Vector3 per = Vector3.Cross(p1 - p2, Vector3.down).normalized;

            float newLength = this.Scale / ((segment.Level + 1) * Random.Range(1f, 2f));
            Vector3 newPointEnd = newPoint + (per * newLength);

            // add new segment
            RoadSegment newSegment = new RoadSegment(
                new Vector2(newPoint.x, newPoint.z),
                new Vector2(newPointEnd.x, newPointEnd.z), segment.Level + 1);

            // calculate other new point
            Vector3 perA = Vector3.Cross(p1 - p2, Vector3.down).normalized * -1;
            Vector3 newPointEndOther = newPoint + (perA * newLength);
            RoadSegment newSegmentOther = new RoadSegment(
                new Vector2(newPoint.x, newPoint.z),
                new Vector2(newPointEndOther.x, newPointEndOther.z), segment.Level + 1);

            // check what segments to add and add them
            bool seg1 = false;
            bool seg2 = false;

            bool with1 = this.SegmentWithin(newSegment, closeCutoff);
            bool with2 = this.SegmentWithin(newSegmentOther, closeCutoff);

            if (!with1)
            {
                Vector2 intersectionPoint = Vector3.zero;
                RoadSegment other = null;

                int iCount = SegmentIntersection(newSegment, out intersectionPoint, out other, segment);

                if (iCount <= 1)
                {
                    this.Segments.RemoveAll(p => p.IsEqual(newSegment));
                    this.Segments.Add(newSegment);
                    seg1 = true;
                }

                if (iCount == 1)
                {
                    // NOTE: Before refactoring, this code associated intersectionPoint with the
                    // other segment, for usage in RoadRenderer.cs. -Casper 2017-08-09
                    RoadSegment[] segmentsA = this.PatchSegment(other, intersectionPoint);

                    // NOTE: Before refactoring, this code associated intersectionPoint with the
                    // newSegment segment, for usage in RoadRenderer.cs. -Casper 2017-08-09
                    RoadSegment[] segmentsB = this.PatchSegment(newSegment, intersectionPoint);

                    // kill very short dead-ends
                    bool sa = segmentsA[0].CalculateLength() > shortCutoff;
                    bool sb = segmentsA[1].CalculateLength() > shortCutoff;
                    bool sc = segmentsB[0].CalculateLength() > shortCutoff;
                    bool sd = segmentsB[1].CalculateLength() > shortCutoff;

                    List<Vector2> points = new List<Vector2>();

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

                    RoadIntersection inter = new RoadIntersection(points);

                    this.Intersections.Add(inter);
                }
            }

            //other side of intersection
            if (!with2)
            {
                Vector2 intersectionPoint = Vector3.zero;
                RoadSegment other = null;

                int iCount = SegmentIntersection(newSegmentOther, out intersectionPoint, out other, segment);

                if (iCount <= 1)
                {
                    this.Segments.RemoveAll(p => p.IsEqual(newSegmentOther));
                    this.Segments.Add(newSegmentOther);
                    seg2 = true;
                }

                if (iCount == 1)
                {
                    // NOTE: Before refactoring, this code associated intersectionPoint with the
                    // other segment, for usage in RoadRenderer.cs. -Casper 2017-08-09
                    RoadSegment[] segmentsA = this.PatchSegment(other, intersectionPoint);

                    // NOTE: Before refactoring, this code associated intersectionPoint with the
                    // newSegmentOther segment, for usage in RoadRenderer.cs. -Casper 2017-08-09
                    RoadSegment[] segmentsB = this.PatchSegment(newSegmentOther, intersectionPoint);

                    //kill very short dead-ends
                    bool sa = segmentsA[0].CalculateLength() > shortCutoff;
                    bool sb = segmentsA[1].CalculateLength() > shortCutoff;
                    bool sc = segmentsB[0].CalculateLength() > shortCutoff;
                    bool sd = segmentsB[1].CalculateLength() > shortCutoff;

                    List<Vector2> points = new List<Vector2>();

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

                    RoadIntersection inter = new RoadIntersection(points);
                    this.Intersections.Add(inter);
                }
            }

            if (seg1 || seg2)
            {
                // NOTE: Before refactoring, this code associated newPoint with the segment segment,
                // for usage in RoadRenderer.cs. -Casper 2017-08-09
                RoadSegment[] segments = this.PatchSegment(
                    segment,
                    new Vector2(newPoint.x, newPoint.z));

                if (seg1 && seg2)
                {
                    RoadIntersection inter = new RoadIntersection(
                        new List<Vector2>{
                            segments[0].PointB,
                            segments[1].PointB,
                            newSegment.PointA,
                            newSegmentOther.PointA });

                    this.Intersections.Add(inter);
                }
                else if (seg1)
                {
                    RoadIntersection inter = new RoadIntersection(
                        new List<Vector2>{
                            segments[0].PointB,
                            segments[1].PointB,
                            newSegment.PointA });

                    this.Intersections.Add(inter);
                }
                else if (seg2)
                {
                    RoadIntersection inter = new RoadIntersection(
                        new List<Vector2>{
                            segments[0].PointB,
                            segments[1].PointB,
                            newSegmentOther.PointA });

                    this.Intersections.Add(inter);
                }
            }
        }

        // TODO: Give function a better name. -Casper 2017-08-09
        private bool SegmentWithin(RoadSegment segment, float max)
        {
            foreach (RoadSegment seg in this.Segments)
            {
                bool amax = DistPointSegment(seg.PointA, segment) < max;
                bool bmax = DistPointSegment(seg.PointB, segment) < max;

                bool amin = MinPointDistance(seg, segment, max / 1.0f);

                if (amax || bmax || amin)
                {
                    return true;
                }
            }

            return false;
        }

        // TODO: Give function a better name. -Casper 2017-08-09
        private float DistPointSegment(Vector2 P, RoadSegment S)
        {
            Vector2 v = S.PointB - S.PointA;
            Vector2 w = P - S.PointA;

            float c1 = Vector2.Dot(w, v);

            if (c1 <= 0)
            {
                return Vector2.Distance(P, S.PointA);
            }

            float c2 = Vector2.Dot(v, v);

            if (c2 <= c1)
            {
                return Vector2.Distance(P, S.PointB);
            }

            float b = c1 / c2;
            Vector2 Pb = S.PointA + (v * b);

            return Vector2.Distance(P, Pb);
        }

        // TODO: Give function a better name. -Casper 2017-08-09
        private bool MinPointDistance(RoadSegment a, RoadSegment b, float min)
        {
            if (Vector2.Distance(a.PointA, b.PointA) < min)
            {
                return true;
            }

            if (Vector2.Distance(a.PointA, b.PointB) < min)
            {
                return true;
            }

            if (Vector2.Distance(a.PointB, b.PointA) < min)
            {
                return true;
            }

            if (Vector2.Distance(a.PointB, b.PointB) < min)
            {
                return true;
            }

            return false;
        }

        // TODO: Give function a better name. -Casper 2017-08-09
        private int SegmentIntersection(
            RoadSegment segment,
            out Vector2 intersection,
            out RoadSegment other,
            RoadSegment skip)
        {
            intersection = Vector2.zero;
            other = null;

            Vector2 tmp = Vector2.zero;
            Vector2 interTmp = Vector3.zero;

            int count = 0;

            for (int i = 0; i < this.Segments.Count; i++)
            {
                RoadSegment seg = this.Segments[i];

                if (seg.IsEqual(skip))
                {
                    continue;
                }

                if (Vector2.Distance(seg.PointA, segment.PointA) < 0.01f
                    || Vector2.Distance(seg.PointB, segment.PointB) < 0.01f)
                {
                    continue;
                }

                if (Vector2.Distance(seg.PointA, segment.PointB) < 0.01f
                    || Vector2.Distance(seg.PointB, segment.PointA) < 0.01f)
                {
                    continue;
                }

                int intersectionCheck = LineSegment2D.CheckIntersection(
                    segment.LineSegment2D,
                    seg.LineSegment2D,
                    out interTmp,
                    out tmp);

                if (intersectionCheck != 0)
                {
                    other = seg;
                    intersection = new Vector2(interTmp.x, interTmp.y);
                    count++;
                }
                //else if (inter2Segments(segment, seg, out interTmp, out tmp) != 0)
            }

            return count;
        }

        // TODO: Give function a better name. -Casper 2017-08-09
        private RoadSegment[] PatchSegment(RoadSegment segment, Vector2 newPoint)
        {
            this.Segments.RemoveAll(p => p.IsEqual(segment));

            RoadSegment left = new RoadSegment(segment.PointA, newPoint, segment.Level);
            RoadSegment right = new RoadSegment(segment.PointB, newPoint, segment.Level);

            this.Segments.Add(left);
            this.Segments.Add(right);

            return new RoadSegment[] { left, right };
        }
    }
}
