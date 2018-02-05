
using System.Collections.Generic;
using UnityEngine;
using AltSrc.UnityCommon.Math;
using AltSrc.ProceduralCity.Generation.Roads;

namespace AltSrc.ProceduralCity.Generation
{
    public class CityView : MonoBehaviour
    {
        public CityModel Model { get; set; }

        public Dictionary<RoadSegment, RoadSegmentView> RoadSegmentViews = new Dictionary<RoadSegment, RoadSegmentView>();

        public static CityView Build(CityModel model)
        {
            GameObject viewObject = new GameObject("CityView");
            CityView view = viewObject.AddComponent<CityView>();
            view.Model = model;

            // build population heatmap quad
            GameObject heatMapQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            heatMapQuad.name = "PopulationHeatMap";
            heatMapQuad.transform.localScale = new Vector3(model.CityBounds.width, model.CityBounds.height, 1f);
            heatMapQuad.transform.eulerAngles = new Vector3(90f, 0f, -180f);
            heatMapQuad.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Texture"));
            heatMapQuad.GetComponent<Renderer>().material.mainTexture = model.PopulationHeatMap;
            heatMapQuad.transform.parent = viewObject.transform;

            // build road segments
            foreach (RoadSegment segment in model.RoadSegments)
            {
                RoadSegmentView segmentView = RoadSegmentView.Build(segment, model);
                segmentView.transform.parent = viewObject.transform;

                view.RoadSegmentViews[segment] = segmentView;
            }

            return view;
        }
    }
}

