using System;
using System.Collections.Generic;
using UnityEngine;
using AltSrc.UnityCommon.Math;
using AltSrc.UnityCommon.DataStructures;
using Zenject;

namespace AltSrc.ProceduralCity.Generation.Roads
{
    public class RoadSegmentView : MonoBehaviour
    {
        public RoadSegment Model { get; set; }

/*
        public class Factory : Factory<RoadSegmentView>
        {
        }
*/

        public static RoadSegmentView Build(RoadSegment model, CityModel cityModel)
        {
            // TODO: Build road segment manually from vertices to reduce the weird scaling effects on the markers. -Casper -2018-01-31
            // build road segment
            GameObject viewObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            viewObject.name = model.ToString();
            viewObject.transform.position = model.LineSegment2D.MidPoint.ToVec3XZ(model.RoadYOffset);
            viewObject.transform.localScale = new Vector3(model.LineSegment2D.Length, model.RoadWidth, 1f);
            viewObject.transform.eulerAngles = new Vector3(90f, model.LineSegment2D.DirectionInDegrees, 0f);
            viewObject.transform.parent = viewObject.transform;

            // add view component
            RoadSegmentView view = viewObject.AddComponent<RoadSegmentView>();
            view.Model = model;

            // TODO: these should be bound in a zenject installer
            Material highwayMaterial = new Material(Shader.Find("Unlit/Color"));
            highwayMaterial.color = Color.red;
            Material normalMaterial = new Material(Shader.Find("Unlit/Color"));
            normalMaterial.color = Color.blue;

            if (model.RoadType == RoadType.Highway)
            {
                viewObject.GetComponent<Renderer>().material = highwayMaterial;
            }
            else if (model.RoadType == RoadType.Normal)
            {
                viewObject.GetComponent<Renderer>().material = normalMaterial;
            }

            // add markers for debugging
            GameObject markerA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            markerA.name = model.PointA.ToString() + ", Pop: " 
                + cityModel.GetPopulationAt(model.PointA).ToString();
            markerA.transform.position = model.PointA.ToVec3XZ(model.RoadYOffset);
            markerA.transform.parent = viewObject.transform;

            GameObject markerB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            markerB.name = model.PointB.ToString() + ", Pop: "
                + cityModel.GetPopulationAt(model.PointB).ToString();
            markerB.transform.position = model.PointB.ToVec3XZ(model.RoadYOffset);
            markerB.transform.parent = viewObject.transform;

            return view;
        }
    }
}
