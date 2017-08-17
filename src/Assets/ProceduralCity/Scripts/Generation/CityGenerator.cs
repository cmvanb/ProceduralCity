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

            // create list and quadtree used to generate road segments
            List<RoadSegment> generatedSegments = new List<RoadSegment>();
            QuadTree<RoadSegment> quadTree = new QuadTree<RoadSegment>(
                0,
                this.rules.QuadTreeRect,
                this.rules.QuadTreeMaxObjectsPerNode,
                this.rules.QuadTreeMaxDepth);

            // loop through priority queue until we hit a limit
            while (priorityQueue.Count > 0
                && generatedSegments.Count < this.rules.MaxRoadSegments)
            {
                // find highest priority (lowest value) segment
                RoadSegment nextSegment = null;

                foreach (RoadSegment s in priorityQueue)
                {
                    if (nextSegment == null
                        || s.Priority < nextSegment.Priority)
                    {
                        nextSegment = s;
                    }
                }

                // remove next segment from of queue
                priorityQueue.Remove(nextSegment);

                // validate segment passes local constraints
                bool validSegment = CheckLocalConstraints(nextSegment, generatedSegments, quadTree);

                if (validSegment)
                {
                    // TODO: Implement RoadSegment.SetupBranchLinks
                    //nextSegment.SetupBranchLinks();

                    // accept segment into list and quad tree
                    generatedSegments.Add(nextSegment);
                    quadTree.Insert(nextSegment);

                    // generate new segments and add to priority queue
                    List<RoadSegment> newSegments = GenerateNewSegmentsFromGlobalGoals(nextSegment);

                    foreach (RoadSegment s in newSegments)
                    {
                        s.Priority += nextSegment.Priority + 1;
                        priorityQueue.Add(nextSegment);
                    }
                }
            }

            Debug.Log(generatedSegments.Count + " segments generated.");

            // TODO: add segments to model

            // TODO: generate building rects

            // TODO: add building rects to model

            // TODO: hookup view with model

            // TODO: generate view objects
        }

        protected bool CheckLocalConstraints(
            RoadSegment segment,
            List<RoadSegment> segments,
            QuadTree<RoadSegment> quadTree)
        {
            List<RoadSegment> quadTreeMatches = quadTree.Retrieve(segment);

            // TODO: implement

            return false;
        }

        protected List<RoadSegment> GenerateNewSegmentsFromGlobalGoals(RoadSegment previousSegment)
        {
            // TODO: implement

            return new List<RoadSegment>();
        }
    }
}
