using System.Collections.Generic;
using UnityEngine;
using AltSrc.UnityCommon.Math;

namespace AltSrc.UnityCommon.DataStructures
{
    public interface IBounds
    {
        Rect Bounds { get; }
    }

    /// <summary>
    ///   A quadtree is a tree data structure in which each internal node has exactly four children.
    /// </summary>
    public class QuadTree<T> where T : IBounds
    {
        public int Depth { get; private set; }
        public Rect Bounds { get; private set; }
        public int MaxObjectsPerNode { get; private set; }
        public int MaxDepth { get; private set; }
        public List<T> Objects;
        public QuadTree<T>[] Nodes;

        public QuadTree(int depth, Rect bounds, int maxObjectsPerNode = 8, int maxDepth = 8)
        {
            this.Depth = depth;
            this.Bounds = bounds;
            this.MaxObjectsPerNode = maxObjectsPerNode;
            this.MaxDepth = maxDepth;

            this.Objects = new List<T>();
            this.Nodes = new QuadTree<T>[4];
        }

        /// <summary>
        ///   Insert an object into the quad tree. If this node exceeds its maximum it will split
        ///   into subnodes and insert the Objects that fit into those subnodes.
        /// </summary>
        public void Insert(T o)
        {
            if (this.Nodes[0] != null)
            {
                int index = GetIndex(o);

                if (index != -1)
                {
                    this.Nodes[index].Insert(o);
                    return;
                }
            }

            this.Objects.Add(o);

            if (this.Objects.Count > this.MaxObjectsPerNode
                && this.Depth < this.MaxDepth)
            {
                if (this.Nodes[0] == null)
                {
                    Split();
                }

                int i = 0;

                while (i < this.Objects.Count)
                {
                    T obj = this.Objects[i];
                    int index = GetIndex(obj);

                    if (index != -1)
                    {
                        this.Nodes[index].Insert(obj);
                        this.Objects.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        /// <summary>
        ///   Retrieve a list of Objects in the same node/subnodes as object o.
        /// </summary>
        public List<T> Retrieve(T o)
        {
            List<T> retrieved = new List<T>();
            int index = GetIndex(o);

            if (index != -1
                && this.Nodes[0] != null)
            {
                retrieved.AddRange(this.Nodes[index].Retrieve(o));
            }

            retrieved.AddRange(this.Objects);
            return retrieved;
        }

        /// <summary>
        ///   Clear the quadtree of all Objects and subnodes.
        /// </summary>
        public void Clear()
        {
            this.Objects.Clear();

            for (int i = 0; i < this.Nodes.Length; i++)
            {
                if (this.Nodes[i] != null)
                {
                    this.Nodes[i].Clear();
                    this.Nodes[i] = null;
                }
            }
        }

        /// <summary>
        ///   Derive line segments from the bounds of each quadtree node. Useful for debugging.
        /// </summary>
        public List<LineSegment2D> GetLineSegments()
        {
            List<LineSegment2D> lineSegments = new List<LineSegment2D>();

            for (int i = 0; i < this.Nodes.Length; i++)
            {
                if (this.Nodes[i] != null)
                {
                    lineSegments.AddRange(this.Nodes[i].GetLineSegments());
                }
            }

            Vector2 topLeft = new Vector2(this.Bounds.xMin, this.Bounds.yMin);
            Vector2 topRight = new Vector2(this.Bounds.xMax, this.Bounds.yMin);
            Vector2 bottomLeft = new Vector2(this.Bounds.xMin, this.Bounds.yMax);
            Vector2 bottomRight = new Vector2(this.Bounds.xMax, this.Bounds.yMax);

            lineSegments.Add(new LineSegment2D(topLeft, topRight));
            lineSegments.Add(new LineSegment2D(topRight, bottomRight));
            lineSegments.Add(new LineSegment2D(bottomRight, bottomLeft));
            lineSegments.Add(new LineSegment2D(bottomLeft, topLeft));

            return lineSegments;
        }

        /// <summary>
        ///   Split the quadtree into 4 subnodes.
        /// </summary>
        protected void Split()
        {
            float splitWidth = this.Bounds.width / 2;
            float splitHeight = this.Bounds.height / 2;
            float x = this.Bounds.x;
            float y = this.Bounds.y;

            Nodes[0] = new QuadTree<T>(
                this.Depth + 1,
                new Rect(x + splitWidth, y, splitWidth, splitHeight),
                this.MaxObjectsPerNode,
                this.MaxDepth);
            Nodes[1] = new QuadTree<T>(
                this.Depth + 1,
                new Rect(x, y, splitWidth, splitHeight),
                this.MaxObjectsPerNode,
                this.MaxDepth);
            Nodes[2] = new QuadTree<T>(
                this.Depth + 1,
                new Rect(x, y + splitHeight, splitWidth, splitHeight),
                this.MaxObjectsPerNode,
                this.MaxDepth);
            Nodes[3] = new QuadTree<T>(
                this.Depth + 1,
                new Rect(x + splitWidth, y + splitHeight, splitWidth, splitHeight),
                this.MaxObjectsPerNode,
                this.MaxDepth);
        }

        /// <summary>
        ///   Determine which node the object belongs in. A return value of -1 means the object
        ///   can't fit within a subnode and must belong to the parent node.
        /// </summary>
        protected int GetIndex(T o)
        {
            float xMiddle = this.Bounds.x + (this.Bounds.width / 2);
            float yMiddle = this.Bounds.y + (this.Bounds.height / 2);

            // Object can completely fit within the top quadrants.
            bool topQuadrants =
                o.Bounds.y >= this.Bounds.y
                && o.Bounds.y + o.Bounds.height < yMiddle;

            // Object can completely fit within the bottom quadrants.
            bool bottomQuadrants =
                o.Bounds.y > yMiddle
                && o.Bounds.y + o.Bounds.height <= this.Bounds.y + this.Bounds.width;

            // Object can completely fit within the left quadrants.
            if (o.Bounds.x >= this.Bounds.x
                && o.Bounds.x + o.Bounds.width < xMiddle)
            {
                if (topQuadrants)
                {
                    return 1;
                }
                else if (bottomQuadrants)
                {
                    return 2;
                }
            }
            // Object can completely fit within the right quadrants.
            else if (o.Bounds.x > xMiddle
                && o.Bounds.x + o.Bounds.width <= this.Bounds.x + this.Bounds.width)
            {
                if (topQuadrants)
                {
                    return 0;
                }
                else if (bottomQuadrants)
                {
                    return 3;
                }
            }

            return -1;

            // NOTE: If this function doesn't work as expected, try the following code. -Casper 2017-08-16
            /*
            // Object can completely fit within the top quadrants
            bool topQuadrants = (o.Bounds.y < yMiddle && o.Bounds.y + o.Bounds.height < yMiddle);
            // Object can completely fit within the bottom quadrants
            bool bottomQuadrants = (o.Bounds.y > yMiddle);

            // Object can completely fit within the left quadrants
            if (o.Bounds.x < xMiddle && o.Bounds.x + o.Bounds.width < xMiddle)
            {
                // ... same here
            }
            // Object can completely fit within the right quadrants
            else if (o.Bounds.x > xMiddle)
            {
                // ... same here
            }
            */
        }
    }
}
