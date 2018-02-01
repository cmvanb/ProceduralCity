
using System.Collections.Generic;
using UnityEngine;
using AltSrc.UnityCommon.Math;
using AltSrc.ProceduralCity.Generation.Roads;

namespace AltSrc.ProceduralCity.Generation
{
    public class CityView
    {
        public CityModel Model { get; set; }
        public Dictionary<RoadSegment, RoadSegmentView> RoadViews;

        public CityView(CityModel model)
        {
            this.Model = model;
            this.RoadViews = new Dictionary<RoadSegment, RoadSegmentView>();
        }

        public void Build()
        {
            GameObject debugView = new GameObject("DebugView");

            // build population heatmap quad
            GameObject heatMapQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            heatMapQuad.transform.localScale = new Vector3(Model.CityBounds.width, Model.CityBounds.height, 1f);
            heatMapQuad.transform.eulerAngles = new Vector3(90f, 0f, -180f);
            heatMapQuad.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Texture"));
            heatMapQuad.GetComponent<Renderer>().material.mainTexture = Model.PopulationHeatMap;
            heatMapQuad.transform.parent = debugView.transform;

            // build road segments
            Material highwayMaterial = new Material(Shader.Find("Unlit/Color"));
            highwayMaterial.color = Color.red;
            Material normalMaterial = new Material(Shader.Find("Unlit/Color"));
            normalMaterial.color = Color.blue;

            foreach (RoadSegment segment in Model.RoadSegments)
            {
                // TODO: Build road segment manually from vertices to reduce the weird scaling effects on the markers. -Casper -2018-01-31
                // build road segment
                GameObject segmentQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                segmentQuad.name = segment.ToString();
                var midPoint = segment.LineSegment2D.MidPoint;
                segmentQuad.transform.position = midPoint.ToVec3XZ(segment.RoadYOffset);
                segmentQuad.transform.localScale = new Vector3(segment.LineSegment2D.Length, segment.RoadWidth, 1f);
                segmentQuad.transform.eulerAngles = new Vector3(90f, segment.LineSegment2D.DirectionInDegrees, 0f);
                segmentQuad.transform.parent = debugView.transform;

                if (segment.RoadType == RoadType.Highway)
                {
                    segmentQuad.GetComponent<Renderer>().material = highwayMaterial;
                }
                else if (segment.RoadType == RoadType.Normal)
                {
                    segmentQuad.GetComponent<Renderer>().material = normalMaterial;
                }

                // add markers for debugging
                GameObject markerA = GameObject.CreatePrimitive(PrimitiveType.Cube);
                markerA.name = segment.PointA.ToString() + ", Pop: " 
                    + Model.GetPopulationAt(segment.PointA).ToString();
                markerA.transform.position = segment.PointA.ToVec3XZ(segment.RoadYOffset);
                markerA.transform.parent = segmentQuad.transform;

                GameObject markerB = GameObject.CreatePrimitive(PrimitiveType.Cube);
                markerB.name = segment.PointB.ToString() + ", Pop: "
                    + Model.GetPopulationAt(segment.PointB).ToString();
                markerB.transform.position = segment.PointB.ToVec3XZ(segment.RoadYOffset);
                markerB.transform.parent = segmentQuad.transform;
            }
        }
    }
}

