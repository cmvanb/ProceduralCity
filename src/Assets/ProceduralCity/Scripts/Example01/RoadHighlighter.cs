using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using AltSrc.UnityCommon.DataStructures;
using AltSrc.UnityCommon.Debugging;
using AltSrc.UnityCommon.Math;
using AltSrc.ProceduralCity.Generation;
using AltSrc.ProceduralCity.Generation.Roads;

namespace AltSrc.ProceduralCity.Example01
{
    public class RoadHighlighter : MonoBehaviour
    {
        [Inject]
        protected CityGenerator cityGenerator;

        protected float crossSize = 3f;
        protected float yOffset = 0.2f;
        protected float scaleFactor = 1.05f;

        protected void Update()
        {
            // calculate mouse position and draw cross
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition.SetZ(Camera.main.transform.position.y));

            Debug.DrawLine(worldPosition + new Vector3(-crossSize, yOffset, -crossSize), worldPosition + new Vector3(crossSize, yOffset, crossSize), Color.red);
            Debug.DrawLine(worldPosition + new Vector3(-crossSize, yOffset, crossSize), worldPosition + new Vector3(crossSize, yOffset, -crossSize), Color.red);

            Rect mousePointerBounds = new Rect(worldPosition.x, worldPosition.z, 0f, 0f);

            if (cityGenerator.CityModel != null)
            {
                List<RoadSegment> matches = new List<RoadSegment>();

                // de-highlight all road segments in quad tree
                cityGenerator.CityModel.QuadTree
                    .GetAllNodes()
                    .ForEach(n => matches.AddRange(n.Objects));
                foreach (RoadSegment match in matches)
                {
                    RoadSegmentView rv = cityGenerator.CityView.RoadSegmentViews[match];
                    rv.GetComponent<Renderer>().material.color = Color.blue;
                }

                if (cityGenerator.CityModel.CityBounds.Contains(worldPosition.ToVec2XZ()))
                {
                    // highlight selected leaf node
                    QuadTree<RoadSegment> qt = cityGenerator.CityModel.QuadTree.GetLeafNodeAt(mousePointerBounds);
                    DebugUtils.DrawRect(qt.Bounds.width * scaleFactor, qt.Bounds.height * scaleFactor, qt.Bounds.center.ToVec3XZ(yOffset), Vector3.up, Color.magenta);

                    // highlight all road segments in node
                    matches = cityGenerator.CityModel.QuadTree.Retrieve(mousePointerBounds);
                    foreach (RoadSegment match in matches)
                    {
                        RoadSegmentView rv = cityGenerator.CityView.RoadSegmentViews[match];
                        rv.GetComponent<Renderer>().material.color = Color.white;
                    }
                }
            }
        }
    }
}

