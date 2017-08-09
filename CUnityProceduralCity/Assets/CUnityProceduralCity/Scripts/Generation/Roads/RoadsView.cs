using System.Collections.Generic;
using UnityEngine;

namespace CUnity.ProceduralCity.Generation
{
    public class RoadsView : MonoBehaviour
    {
        // Local vars.
        protected RoadsModel model;

        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;

        // Public methods.
        public void Initialize(RoadsModel model, Material roadMaterial)
        {
            this.model = model;

            // TODO: Implement override ToString() on RoadsModel. -Casper 2017-08-09
            Debug.Log(model.ToString());

            this.meshFilter = this.gameObject.AddComponent<MeshFilter>();
            this.meshFilter.mesh = new Mesh();

            this.meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            this.meshRenderer.sharedMaterial = roadMaterial;

            foreach (RoadSegment segment in model.Segments)
            {
                AddSegmentQuad(segment);
            }

            foreach (RoadIntersection intersection in model.Intersections)
            {
                // TODO: Implement. -Casper 2017-08-09
                //AddIntersection(intersection);
            }
        }

        public void CleanUp()
        {
            throw new System.NotImplementedException();

            // TODO: Implement. -Casper 2017-08-09
            // dereference model

            // destroy meshFilter

            // destroy meshRenderer
        }

        // Local methods.
        protected void AddSegmentQuad(RoadSegment segment)
        {
            Mesh mesh = this.meshFilter.mesh;

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

            foreach (Vector2 point in intersection.Points)
            {
                // TODO: Here's a doozy: you refactored away mySegement as it looks dirty, but here
                // you need it! How do you access the data cleanly? -Casper 2017-08-09
                intersectionPoints.Add(GetVerticeOffset(point, point.mySegement.GetOther(point)));
            }

            Mesh mesh = this.Intersections.GetComponent<MeshFilter>().sharedMesh;

            //get mesh details
            List<int> triangles = mesh.vertexCount == 0 ? new List<int>() : new List<int>(mesh.triangles);
            List<Vector3> vertices = new List<Vector3>(mesh.vertices);
            List<Vector3> normals = new List<Vector3>(mesh.normals);
            List<Vector2> uvs = new List<Vector2>(mesh.uv);

            //get last triangle
            int last = vertices.Count - 1;

            List<Vector3> interVecs = new List<Vector3>();

            foreach (Vector3[] points in intersectionPoints)
            {
                interVecs.AddRange(points);
            }

            Vector3 center = new Vector3(interVecs.Average(p => p.x), 0, interVecs.Average(p => p.z));

            IComparer<Vector3> comparer = new CircleSort(center);

            interVecs.Sort(comparer);

            interVecs.Reverse();

            interVecs.Add(interVecs[0]);

            for (int i = 0; i < interVecs.Count - 1; i++)
            {
                //create vertices
                Vector3 vertA = interVecs[i];
                Vector3 vertB = interVecs[i + 1];
                Vector3 vertC = center;

                //add vertices
                vertices.AddRange(new Vector3[]{ vertA, vertB, vertC });

                //add triangles
                triangles.AddRange(new int[]{ ++last, ++last, ++last });

                //add normals
                normals.AddRange(new Vector3[]{ Vector3.up, Vector3.up, Vector3.up });

                //add uvs
                uvs.AddRange(new Vector2[]{ new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 0.5f) });
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            //mesh.normals = normals.ToArray();
            mesh.RecalculateNormals();
        }

        protected Vector3[] GetVerticeOffset(RoadPoint main, RoadPoint other)
        {
            Vector3[] result = new Vector3[2];

            //get the road start and end
            Vector3 pointA = new Vector3(main.point.x, 0, main.point.y);
            Vector3 pointB = new Vector3(other.point.x, 0, other.point.y);

            //adjust for intersection
            Vector3 segvec = (pointA - pointB).normalized;
            pointA -= segvec * this.intersectionOffset;
            pointB += segvec * this.intersectionOffset;

            //get perpendicular vector
            Vector3 per = Vector3.Cross(pointA - pointB, Vector3.down).normalized;

            //create result
            result[0] = pointA + per * (0.5f * this.roadWidth);
            result[1] = pointA - per * (0.5f * this.roadWidth);

            return result;
        }

    }
}
