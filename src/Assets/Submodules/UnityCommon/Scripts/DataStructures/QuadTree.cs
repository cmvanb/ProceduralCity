using System.Collections.Generic;
using UnityEngine;

namespace AltSrc.UnityCommon.DataStructures
{
    /// <summary>
    ///   A quadtree is a tree data structure in which each internal node has exactly four children.
    /// </summary>
    public class QuadTree
    {
        public int Depth { get; private set; }
        public Rect Bounds { get; private set; }
        public int MaxObjectsPerNode { get; private set; }
        public int MaxDepth { get; private set; }

        protected List<Rect> objects;
        protected QuadTree[] nodes;

        public QuadTree(int depth, Rect bounds, int maxObjectsPerNode = 8, int maxDepth = 8)
        {
            this.Depth = depth;
            this.Bounds = bounds;
            this.MaxObjectsPerNode = maxObjectsPerNode;
            this.MaxDepth = maxDepth;

            this.objects = new List<Rect>();
            this.nodes = new QuadTree[4];
        }

        /// <summary>
        ///   Insert an object into the quad tree. If this node exceeds its maximum it will split
        ///   into subnodes and insert the objects that fit into those subnodes.
        /// </summary>
        public void Insert(Rect rect)
        {
            if (this.nodes[0] != null)
            {
                int index = GetIndex(rect);

                if (index != -1)
                {
                    this.nodes[index].Insert(rect);
                    return;
                }
            }

            this.objects.Add(rect);

            if (this.objects.Count > this.MaxObjectsPerNode
                && this.Depth < this.MaxDepth)
            {
                if (this.nodes[0] == null)
                {
                    Split();
                }

                int i = 0;

                while (i < this.objects.Count)
                {
                    Rect r = this.objects[i];
                    int index = GetIndex(r);

                    if (index != -1)
                    {
                        this.nodes[index].Insert(r);
                        this.objects.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        /// <summary>
        ///   Retrieve a list of objects that fall within a rectangle.
        /// </summary>
        public List<Rect> Retrieve(Rect rect)
        {
            List<Rect> retrieved = new List<Rect>();
            int index = GetIndex(rect);

            if (index != -1
                && this.nodes[0] == null)
            {
                retrieved.AddRange(this.nodes[index].Retrieve(rect));
            }

            retrieved.AddRange(this.objects);
            return retrieved;
        }

        /// <summary>
        ///   Clear the quadtree of all objects and subnodes.
        /// </summary>
        public void Clear()
        {
            this.objects.Clear();

            for (int i = 0; i < this.nodes.Length; i++)
            {
                if (this.nodes[i] != null)
                {
                    this.nodes[i].Clear();
                    this.nodes[i] = null;
                }
            }
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

            nodes[0] = new QuadTree(
                this.Depth + 1,
                new Rect(x + splitWidth, y, splitWidth, splitHeight),
                this.MaxObjectsPerNode,
                this.MaxDepth);
            nodes[1] = new QuadTree(
                this.Depth + 1,
                new Rect(x, y, splitWidth, splitHeight),
                this.MaxObjectsPerNode,
                this.MaxDepth);
            nodes[2] = new QuadTree(
                this.Depth + 1,
                new Rect(x, y + splitHeight, splitWidth, splitHeight),
                this.MaxObjectsPerNode,
                this.MaxDepth);
            nodes[3] = new QuadTree(
                this.Depth + 1,
                new Rect(x + splitWidth, y + splitHeight, splitWidth, splitHeight),
                this.MaxObjectsPerNode,
                this.MaxDepth);
        }

        /// <summary>
        ///   Determine which node the object belongs in. A return value of -1 means the object
        ///   can't fit within a subnode and must belong to the parent node.
        /// </summary>
        protected int GetIndex(Rect rect)
        {
            float xMiddle = this.Bounds.x + (this.Bounds.width / 2);
            float yMiddle = this.Bounds.y + (this.Bounds.height / 2);

            // Object can completely fit within the top quadrants.
            bool topQuadrants =
                rect.y >= this.Bounds.y
                && rect.y + rect.height < yMiddle;

            // Object can completely fit within the bottom quadrants.
            bool bottomQuadrants =
                rect.y > yMiddle
                && rect.y + rect.height <= this.Bounds.y + this.Bounds.width;

            // Object can completely fit within the left quadrants.
            if (rect.x >= this.Bounds.x
                && rect.x + rect.width < xMiddle)
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
            else if (rect.x > xMiddle
                && rect.x + rect.width <= this.Bounds.x + this.Bounds.width)
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
            bool topQuadrants = (rect.y < yMiddle && rect.y + rect.height < yMiddle);
            // Object can completely fit within the bottom quadrants
            bool bottomQuadrants = (rect.y > yMiddle);

            // Object can completely fit within the left quadrants
            if (rect.x < xMiddle && rect.x + rect.width < xMiddle)
            {
                // ... same here
            }
            // Object can completely fit within the right quadrants
            else if (rect.x > xMiddle)
            {
                // ... same here
            }
            */
        }
    }
}
