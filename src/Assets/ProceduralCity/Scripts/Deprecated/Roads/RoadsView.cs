using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CUnity.ProceduralCity.Deprecated
{
    public class RoadsView : MonoBehaviour
    {
        // Local vars.
        protected RoadsModel model;

        protected MeshFilter roadsMeshFilter;
        protected MeshRenderer roadsMeshRenderer;
        protected MeshFilter intersectionsMeshFilter;
        protected MeshRenderer intersectionsMeshRenderer;

        // Public methods.
        public void Initialize(RoadsModel model, Material roadMaterial)
        {
            this.model = model;

            // TODO: Implement override ToString() on RoadsModel. -Casper 2017-08-09
            Debug.Log(model.ToString());

            // Create roads.
            this.roadsMeshFilter = this.gameObject.AddComponent<MeshFilter>();
            this.roadsMeshFilter.mesh = new Mesh();

            this.roadsMeshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            this.roadsMeshRenderer.sharedMaterial = roadMaterial;

            foreach (RoadSegment segment in model.Segments)
            {
                AddSegmentQuad(segment);
            }

            // Create intersections.
            GameObject intersectionsObject = new GameObject("Intersections");

            intersectionsObject.transform.parent = transform;
            intersectionsObject.transform.localPosition = Vector3.zero;

            this.intersectionsMeshFilter = intersectionsObject.AddComponent<MeshFilter>();
            this.intersectionsMeshFilter.mesh = new Mesh();

            this.intersectionsMeshRenderer = intersectionsObject.gameObject.AddComponent<MeshRenderer>();

            foreach (RoadIntersection intersection in model.Intersections)
            {
                AddIntersection(intersection);
            }
        }

        public void CleanUp()
        {
            throw new System.NotImplementedException();

            // TODO: Implement. -Casper 2017-08-09
            // dereference model

            // destroy roadsMeshFilter

            // destroy roadsMeshRenderer
        }

        // Local methods.
        protected void AddSegmentQuad(RoadSegment segment)
        {
            Mesh mesh = this.roadsMeshFilter.mesh;

            // get mesh details
            List<int> triangles = mesh.vertexCount == 0 ? new List<int>() : new List<int>(mesh.triangles);
            List<Vector3> vertices = new List<Vector3>(mesh.vertices);
            List<Vector3> normals = new List<Vector3>(mesh.normals);
            List<Vector2> uvs = new List<Vector2>(mesh.uv);

            // get last triangle
            int last = vertices.Count;

            // get the road start and end
            Vector3 pointA = new Vector3(segment.PointA.x, 0, segment.PointA.y);
            Vector3 pointB = new Vector3(segment.PointB.x, 0, segment.PointB.y);

            // adjust for intersection
            Vector3 segvec = (pointA - pointB).normalized;

            pointA -= segvec * this.model.IntersectionOffset;
            pointB += segvec * this.model.IntersectionOffset;

            // get perpendicular vector
            Vector3 per = Vector3.Cross(pointA - pointB, Vector3.down).normalized;

            // create vertices
            Vector3 vertTL = pointA + per * (0.5f * this.model.RoadWidth);
            Vector3 vertTR = pointA - per * (0.5f * this.model.RoadWidth);
            Vector3 vertBL = pointB + per * (0.5f * this.model.RoadWidth);
            Vector3 vertBR = pointB - per * (0.5f * this.model.RoadWidth);

            // add vertices
            vertices.AddRange(new Vector3[]{ vertTL, vertTR, vertBL, vertBR });

            // add triangles
            triangles.AddRange(new int[]{ last, last + 2, last + 1 });
            triangles.AddRange(new int[]{ last + 1, last + 2, last + 3 });

            // add normals
            normals.AddRange(new Vector3[]{ Vector3.up, Vector3.up, Vector3.up, Vector3.up });

            // add uvs
            float length = Vector3.Distance(pointA, pointB) * this.model.RoadTextureTiling;

            uvs.AddRange(new Vector2[] {
                new Vector2(0, length),
                new Vector2(1, length),
                new Vector2(0, 0),
                new Vector2(1, 0) });

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
        }

        protected void AddIntersection(RoadIntersection intersection)
        {
            // Populate intersectionPoints list.
            List<Vector3[]> intersectionPoints = new List<Vector3[]>();

            for (int i = 0; i < intersection.SegmentsCount; ++i)
            {
                Vector2 point = intersection.GetSegmentPoint(i);

                Vector2 otherPoint = intersection.GetSegmentPoint(i, true);

                Vector3[] vertexOffsets = GetVertexOffsets(point, otherPoint);

                intersectionPoints.Add(vertexOffsets);
            }

            // Pull out vec3s from intersectionPoints into intersectionVectors.
            List<Vector3> intersectionVectors = new List<Vector3>();

            foreach (Vector3[] points in intersectionPoints)
            {
                intersectionVectors.AddRange(points);
            }

            Vector3 center = new Vector3(intersectionVectors.Average(p => p.x), 0, intersectionVectors.Average(p => p.z));

            intersectionVectors.Sort((a, b) =>
            {
                float a1 = Mathf.Atan2(a.x - center.x, a.z - center.z) * Mathf.Rad2Deg;
                float a2 = Mathf.Atan2(b.x - center.x, b.z - center.z) * Mathf.Rad2Deg;

                a1 += a1 < 0 ? 360 : 0;
                a2 += a2 < 0 ? 360 : 0;

                return a1 > a2 ? -1 : 1;
            });

            intersectionVectors.Reverse();
            intersectionVectors.Add(intersectionVectors[0]);

            Mesh mesh = this.intersectionsMeshFilter.mesh;

            // get mesh details
            List<int> triangles = mesh.vertexCount == 0 ? new List<int>() : new List<int>(mesh.triangles);
            List<Vector3> vertices = new List<Vector3>(mesh.vertices);
            List<Vector3> normals = new List<Vector3>(mesh.normals);
            List<Vector2> uvs = new List<Vector2>(mesh.uv);

            // get last triangle
            int last = vertices.Count - 1;

            for (int i = 0; i < intersectionVectors.Count - 1; i++)
            {
                // create vertices
                Vector3 vertA = intersectionVectors[i];
                Vector3 vertB = intersectionVectors[i + 1];
                Vector3 vertC = center;

                // add vertices
                vertices.AddRange(new Vector3[]{ vertA, vertB, vertC });

                // add triangles
                triangles.AddRange(new int[]{ ++last, ++last, ++last });

                // add normals
                normals.AddRange(new Vector3[]{ Vector3.up, Vector3.up, Vector3.up });

                // add uvs
                uvs.AddRange(new Vector2[]{ new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 0.5f) });
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
        }

        protected Vector3[] GetVertexOffsets(Vector2 main, Vector2 other)
        {
            Vector3[] result = new Vector3[2];

            // get the road start and end
            Vector3 pointA = new Vector3(main.x, 0, main.y);
            Vector3 pointB = new Vector3(other.x, 0, other.y);

            // adjust for intersection
            Vector3 segvec = (pointA - pointB).normalized;

            pointA -= segvec * this.model.IntersectionOffset;
            pointB += segvec * this.model.IntersectionOffset;

            // get perpendicular vector
            Vector3 per = Vector3.Cross(pointA - pointB, Vector3.down).normalized;

            // create result
            result[0] = pointA + per * (0.5f * this.model.RoadWidth);
            result[1] = pointA - per * (0.5f * this.model.RoadWidth);

            return result;
        }
    }
}
