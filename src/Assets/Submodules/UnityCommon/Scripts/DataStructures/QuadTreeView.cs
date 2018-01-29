using System.Collections.Generic;
using UnityEngine;
using AltSrc.UnityCommon.Math;

namespace AltSrc.UnityCommon.DataStructures
{
    public class QuadTreeView : MonoBehaviour
    {
        public List<LineSegment2D> LineSegments = new List<LineSegment2D>();

        /// <summary>
        ///   Retrieves the line segments from the quad tree and passes them to the view object.
        /// </summary>
        public static GameObject Build<T>(QuadTree<T> quadTree) where T : IBounds
        {
            GameObject viewObject = new GameObject("QuadTreeView");

            QuadTreeView viewComponent = viewObject.AddComponent<QuadTreeView>();
            viewComponent.LineSegments = quadTree.GetLineSegments();

            return viewObject;
        }

        /// <summary>
        ///   Draw each line segment in the scene view.
        /// </summary>
        public void Update()
        {
            foreach (LineSegment2D segment in LineSegments)
            {
                Debug.DrawLine(segment.PointA.ToVec3XZ(), segment.PointB.ToVec3XZ(), Color.cyan);
            }
        }
    }
}
