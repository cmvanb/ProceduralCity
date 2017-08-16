using System.Collections.Generic;
using UnityEngine;
using Zenject;
using AltSrc.UnityCommon.DataStructures;
using AltSrc.ProceduralCity.Generation.Roads;

namespace AltSrc.ProceduralCity.Generation
{
    public class CityGenerator : MonoBehaviour
    {
        [SerializeField]
        protected CityGeneratorRules rules;

        protected CityModel model;

        [Inject]
        public void Initialize(CityModel model)
        {
            this.model = model;
        }

        public void Generate()
        {
            Debug.Log("CityGenerator.Generate");

            // TODO: setup model with rules data

            // TODO: random seed

            // create priority queue
            List<RoadSegment> priorityQueue = new List<RoadSegment>();

            // setup root segments in opposing directions
            var length = this.rules.DefaultRoadLengths[RoadType.Highway];
            var rootSegment1 = new RoadSegment(
                new Vector2(0f, 0f),
                new Vector2(length, 0f),
                0,
                RoadType.Highway);
            var rootSegment2 = new RoadSegment(
                new Vector2(0f, 0f),
                new Vector2(-length, 0f),
                0,
                RoadType.Highway);

            // link root segments to each other
            rootSegment1.LinksBackward.Add(rootSegment2);
            rootSegment2.LinksBackward.Add(rootSegment1);

            // add root segments to priority queue
            priorityQueue.Add(rootSegment1);
            priorityQueue.Add(rootSegment2);

            // TODO: generate road segments
            List<RoadSegment> segments = new List<RoadSegment>();
            QuadTree quadTree = new QuadTree(
                0,
                this.rules.QuadTreeRect,
                this.rules.QuadTreeMaxObjectsPerNode,
                this.rules.QuadTreeMaxDepth);

            while (priorityQueue.Count > 0
                && segments.Count < this.rules.MaxRoadSegments)
            {
                // TODO: implement
                throw new System.NotImplementedException();
            }

            // TODO: add segments to model

            // TODO: generate building rects

            // TODO: add building rects to model

            // TODO: hookup view with model

            // TODO: generate view objects
        }
    }
}
