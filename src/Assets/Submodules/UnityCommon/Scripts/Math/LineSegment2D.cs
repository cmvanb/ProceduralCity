using UnityEngine;

namespace AltSrc.UnityCommon.Math
{
    public struct LineSegment2D
    {
        public Vector2 PointA { get; set; }
        public Vector2 PointB { get; set; }

        public LineSegment2D(Vector2 pointA, Vector2 pointB)
        {
            this.PointA = pointA;
            this.PointB = pointB;
        }

        /* -----------------------------------------------------------------------------------------
        Check whether two finite line segments intersect.
        NOTE: The out values may not always exist.

          @param segmentA - Finite line segment.
          @param segmentB - Finite line segment.
          @out i0         - Intersection point (if it exists)
          @out i1         - Intersection end point (if the segments overlap)
          @returns        - 0: Disjointed, no intersection.
                            1: Intersection at unique point 'intersectPoint'.
                            2: Overlapping segments from 'intersectPoint' to 'intersectEndpoint'.
        ----------------------------------------------------------------------------------------- */
        public static int CheckIntersection(
            LineSegment2D segmentA,
            LineSegment2D segmentB,
            out Vector2 i0,
            out Vector2 i1)
        {
            Vector2 u = segmentA.PointB - segmentA.PointA;
            Vector2 v = segmentB.PointB - segmentB.PointA;
            Vector2 w = segmentA.PointA - segmentB.PointA;
            float D = ArePerpindicular(u, v);

            i0 = Vector2.zero;
            i1 = Vector2.zero;

            // test if  they are parallel (includes either being a point)
            if (Mathf.Abs(D) < 0.01f)
            {
                // segmentA and segmentB are parallel

                // test if they are NOT collinear
                if (ArePerpindicular(u, w) != 0 || ArePerpindicular(v, w) != 0)
                {
                    return 0;
                }

                // they are collinear or degenerate
                // check if they are degenerate points
                float du = Vector2.Dot(u, u);
                float dv = Vector2.Dot(v, v);

                // both segments are points
                if (du == 0 && dv == 0)
                {
                    // they are distinct points
                    if (segmentA.PointA != segmentB.PointA)
                    {
                        return 0;
                    }

                    // they are the same point
                    i0 = segmentA.PointA;
                    return 1;
                }

                // test if segmentA is a single point
                if (du == 0)
                {
                    // but is not in segmentB
                    if (ContainsPoint(segmentA.PointA, segmentB) == 0)
                    {
                        return 0;
                    }

                    i0 = segmentA.PointA;
                    return 1;
                }

                // test if segmentB is a single point
                if (dv == 0)
                {
                    // but is not in segmentA
                    if (ContainsPoint(segmentB.PointA, segmentA) == 0)
                    {
                        return 0;
                    }

                    i0 = segmentB.PointA;
                    return 1;
                }

                // they are collinear segments - test for overlap (or not)

                // endpoints of segmentA in eqn for segmentB
                float t0, t1;
                Vector2 w2 = segmentA.PointB - segmentB.PointA;

                if (v.x != 0)
                {
                    t0 = w.x / v.x;
                    t1 = w2.x / v.x;
                }
                else
                {
                    t0 = w.y / v.y;
                    t1 = w2.y / v.y;
                }

                // must have t0 smaller than t1

                // swap if not
                if (t0 > t1)
                {
                    float t = t0;
                    t0 = t1;
                    t1 = t;
                }

                if (t0 > 1 || t1 < 0)
                {
                    // NO overlap
                    return 0;
                }

                t0 = t0 < 0 ? 0 : t0;               // clip to min 0
                t1 = t1 > 1 ? 1 : t1;               // clip to max 1

                if (t0 == t1)
                {
                    // intersect is a point
                    i0 = segmentB.PointA + t0 * v;
                    return 1;
                }

                // they overlap in a valid subsegment
                i0 = segmentB.PointA + t0 * v;
                i1 = segmentB.PointA + t1 * v;

                return 2;
            }

            // the segments are skew and may intersect in a point
            // get the intersect parameter for segmentA
            float sI = ArePerpindicular(v, w) / D;

            // no intersect with segmentA
            if (sI < 0 || sI > 1)
            {
                return 0;
            }

            // get the intersect parameter for segmentB
            float tI = ArePerpindicular(u, w) / D;

            // no intersect with segmentB
            if (tI < 0 || tI > 1)
            {
                return 0;
            }

            // compute segmentA intersect point
            i0 = segmentA.PointA + sI * u;

            return 1;
        }

        // ContainsPoint(): determine if a point is inside a segment
        //    Input:  a point P, and a collinear segment S
        //    Return: 1 = P is inside S
        //            0 = P is  not inside S
        public static int ContainsPoint(Vector2 P, LineSegment2D S)
        {
            // S is not vertical
            if (S.PointA.x != S.PointB.x)
            {
                if ((S.PointA.x <= P.x && P.x <= S.PointB.x)
                    || (S.PointA.x >= P.x && P.x >= S.PointB.x))
                {
                    return 1;
                }
            }
            // S is vertical, so test y coordinate
            else
            {
                if ((S.PointA.y <= P.y && P.y <= S.PointB.y)
                    || (S.PointA.y >= P.y && P.y >= S.PointB.y))
                {
                    return 1;
                }
            }

            return 0;
        }

        private static float ArePerpindicular(Vector2 u, Vector2 v)
        {
            return ((u).x * (v).y - (u).y * (v).x);
        }
    }
}
