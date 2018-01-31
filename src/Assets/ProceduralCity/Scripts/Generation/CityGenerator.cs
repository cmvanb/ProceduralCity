using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CatlikeCoding.SimplexNoise;
using Zenject;
using AltSrc.UnityCommon.DataStructures;
using AltSrc.UnityCommon.Math;
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

            // pass rules data to model
            this.model.CityName = this.rules.CityName;

            // retrieve population heat map from rules (or if it is not provided, generate one)
            // and pass to model
            this.model.PopulationHeatMap = GeneratePopulationHeatMap(this.rules);

            // generate road segments and pass to model
            this.model.RoadSegments = GenerateRoads(this.rules, this.model.PopulationHeatMap);

            // TODO: Implement. -Casper 2017-09-14
            //GenerateBuildings();

            // TODO: Implement. -Casper 2017-09-14
            //BuildView();

            BuildDebugView(this.rules, this.model);
        }

        protected Texture2D GeneratePopulationHeatMap(CityGeneratorRules rules)
        {
            if (rules.PopulationHeatMap != null)
            {
                return rules.PopulationHeatMap;
            }

            int width = MathUtils.RoundUpToNextPow2((int)(rules.CityBounds.width / 100));
            int height = MathUtils.RoundUpToNextPow2((int)(rules.CityBounds.height / 100));
            int resolution = Mathf.Max(width, height);

            // NOTE: seed the noise library's hash function, otherwise TextureCreator will always
            // return the same texture.
            Noise.SeedHashFunction();

            Texture2D result = TextureCreator.Create(resolution);

            return result;
        }

        // TODO: Consider extracting road generation algorithm to a class. -Casper 2017-09-15
        protected List<RoadSegment> GenerateRoads(CityGeneratorRules rules, Texture2D populationHeatMap)
        {
            // create priority queue
            List<RoadSegment> priorityQueue = new List<RoadSegment>();

            // setup root segments in opposing directions
            var length = rules.DefaultRoadLengths[RoadType.Highway];
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

            // list tracks segments that are generated and have passed local constraints function
            List<RoadSegment> generatedSegments = new List<RoadSegment>();

            // quadtree is used by local contraints function to quickly find nearby segments 
            QuadTree<RoadSegment> quadTree = new QuadTree<RoadSegment>(
                0,
                rules.CityBounds,
                rules.QuadTreeMaxObjectsPerNode,
                rules.QuadTreeMaxDepth);

            // loop through priority queue until we hit a limit
            while (priorityQueue.Count > 0
                && generatedSegments.Count < rules.MaxRoadSegments)
            {
                RoadSegment nextSegment = null;

                // find highest priority (lowest value) segment and remove from queue
                foreach (RoadSegment s in priorityQueue)
                {
                    if (nextSegment == null
                        || s.Priority < nextSegment.Priority)
                    {
                        nextSegment = s;
                    }
                }

                priorityQueue.Remove(nextSegment);

                // validate segment passes local constraints
                bool validSegment = CheckLocalConstraints(rules, nextSegment, generatedSegments, quadTree);

                if (validSegment)
                {
                    // add segment to list and quad tree
                    generatedSegments.Add(nextSegment);
                    quadTree.Insert(nextSegment);

                    // propose new segments based on global goals and add to priority queue
                    List<RoadSegment> newSegments = ProposeNewSegmentsFromGlobalGoals(rules, nextSegment, populationHeatMap);

                    foreach (RoadSegment s in newSegments)
                    {
                        s.Priority += nextSegment.Priority + 1;
                        priorityQueue.Add(s);
                    }
                }
                else
                {
                    RoadSegment.ClearAllLinksToRoadSegment(nextSegment);
                }
            }

            Debug.Log(generatedSegments.Count + " segments generated.");

            QuadTreeView.Build(quadTree);

            return generatedSegments;
        }

        protected void GenerateBuildings()
        {
            // TODO: generate building rects

            // TODO: add building rects to model

            throw new NotImplementedException();
        }

        protected void BuildView()
        {
            // TODO: hookup view with model

            // TODO: generate view objects

            throw new NotImplementedException();
        }

        protected void BuildDebugView(CityGeneratorRules rules, CityModel model)
        {
            GameObject debugView = new GameObject("DebugView");

            // build population heatmap quad
            GameObject heatMapQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            heatMapQuad.transform.localScale = new Vector3(rules.CityBounds.width, rules.CityBounds.height, 1f);
            heatMapQuad.transform.eulerAngles = new Vector3(90f, 0f, -180f);
            heatMapQuad.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Texture"));
            heatMapQuad.GetComponent<Renderer>().material.mainTexture = model.PopulationHeatMap;
            heatMapQuad.transform.parent = debugView.transform;

            // build road segments
            Material highwayMaterial = new Material(Shader.Find("Unlit/Color"));
            highwayMaterial.color = Color.red;
            Material normalMaterial = new Material(Shader.Find("Unlit/Color"));
            normalMaterial.color = Color.blue;

            foreach (RoadSegment segment in model.RoadSegments)
            {
                float roadYOffset = rules.DefaultRoadYOffset[segment.RoadType];

                // TODO: Build road segment manually from vertices to reduce the weird scaling effects on the markers. -Casper -2018-01-31
                // build road segment
                GameObject segmentQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                segmentQuad.name = segment.ToString();
                var midPoint = segment.LineSegment2D.MidPoint;
                segmentQuad.transform.position = midPoint.ToVec3XZ(roadYOffset);
                segmentQuad.transform.localScale = new Vector3(segment.LineSegment2D.Length, rules.DefaultRoadWidths[segment.RoadType], 1f);
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
                    + GetPopulationAt(model.PopulationHeatMap, rules.CityBounds, segment.PointA).ToString();
                markerA.transform.position = segment.PointA.ToVec3XZ(roadYOffset);
                markerA.transform.parent = segmentQuad.transform;

                GameObject markerB = GameObject.CreatePrimitive(PrimitiveType.Cube);
                markerB.name = segment.PointB.ToString() + ", Pop: "
                    + GetPopulationAt(model.PopulationHeatMap, rules.CityBounds, segment.PointB).ToString();
                markerB.transform.position = segment.PointB.ToVec3XZ(roadYOffset);
                markerB.transform.parent = segmentQuad.transform;
            }
        }

        protected bool CheckLocalConstraints(
            CityGeneratorRules rules,
            RoadSegment segment,
            List<RoadSegment> generatedSegments,
            QuadTree<RoadSegment> quadTree)
        {
            // don't allow segments to escape city bounds
            if (segment.Bounds.xMin < rules.CityBounds.xMin
                || segment.Bounds.yMin < rules.CityBounds.yMin
                || segment.Bounds.xMax > rules.CityBounds.xMax
                || segment.Bounds.yMax > rules.CityBounds.yMax)
            {
                return false;
            }

            List<RoadSegment> matches = quadTree.Retrieve(segment);

            Debug.LogWarning(segment.ToString() + " has " + matches.Count);

            foreach (RoadSegment match in matches)
            {
                Vector2 intersectionPoint0, intersectionPoint1;

                int intersecting = LineSegment2D.CheckIntersection(
                    segment.LineSegment2D,
                    match.LineSegment2D,
                    out intersectionPoint0,
                    out intersectionPoint1);

                float differenceInDegrees = LineSegment2D.MinimumAngleDifferenceInDegrees(
                    segment.LineSegment2D,
                    match.LineSegment2D);

                // TODO: Consider factoring out the various checks into separate functions. -Casper 2017-09-18
                // intersection overlap check
                if (intersecting == 2)
                {
                    Debug.LogWarning("#0");
                    return false;
                }

                // TODO: Consider factoring out the various checks into separate functions. -Casper 2017-09-18
                // intersection non-overlap check
                if (intersecting == 1
                    && intersectionPoint0 != segment.PointA
                    && intersectionPoint0 != segment.PointB)
                {
                    // if intersecting lines are too similar don't continue
                    if (differenceInDegrees < rules.MinimumIntersectionAngleDifference)
                    {
                        Debug.LogWarning("#1");
                        return false;
                    }

                    // perform split of intersecting segments
                    RoadSegment splitSegment = match.Split(intersectionPoint0, segment);
                    segment.LineSegment2D = new LineSegment2D(
                        segment.PointA,
                        intersectionPoint0);

                    segment.HasBeenSplit = true;

                    // accept split segment into list and quad tree
                    generatedSegments.Add(splitSegment);
                    quadTree.Insert(splitSegment);
                }

                // TODO: Consider factoring out the various checks into separate functions. -Casper 2017-09-18
                // snap to crossing within radius check
                float distanceBetweenSegmentAndMatchPointB = Vector2.Distance(segment.PointB, match.PointB);

                if (distanceBetweenSegmentAndMatchPointB <= rules.RoadSnapDistance)
                {
                    segment.LineSegment2D = new LineSegment2D(segment.PointA, match.PointB);
                    segment.HasBeenSplit = true;

                    // update links of match corresponding to match.PointB
                    var links = match.StartIsBackwards() ? match.LinksForward : match.LinksBackward;

                    // check for duplicate lines, don't add if it exists
                    // this is done before links are setup, to avoid having to undo that step
                    if (links.Any(x =>
                        (x.PointA == segment.PointB && x.PointB == segment.PointA) ||
                        (x.PointA == segment.PointA && x.PointB == segment.PointB)))
                    {
                        Debug.LogWarning("#2");
                        return false;
                    }

                    foreach (RoadSegment link in links)
                    {
                        // TODO: Figure out whether this code is useful. -Casper 2017-09-18
                        /*
                        // pick links of remaining segments at junction corresponding to match.PointB
                        var linksContainingMatch = link.GetListOfLinksContainingSegment(match);

                        if (linksContainingMatch != null)
                        {
                            // TODO: Verify this works as expected (pass by ref). -Casper 2017-09-11
                            linksContainingMatch.Add(segment);
                        }
                        */

                        // add junction segments to snapped segment
                        segment.LinksForward.Add(link);
                    }

                    links.Add(segment);
                    segment.LinksForward.Add(match);
                }

                // TODO: Consider factoring out the various checks into separate functions. -Casper 2017-09-18
                // intersection within radius check
                Vector2 projectedPoint = Vector2.zero;

                float projectedLineLength = Mathf.Infinity;

                float distanceToSegmentPointA = LineSegment2D.FindDistanceToPoint(
                    match.LineSegment2D,
                    segment.PointA,
                    out projectedPoint,
                    out projectedLineLength);

                float distanceToSegmentPointB = LineSegment2D.FindDistanceToPoint(
                    match.LineSegment2D,
                    segment.PointB,
                    out projectedPoint,
                    out projectedLineLength);

                float distanceToSegmentPoint = Mathf.Min(distanceToSegmentPointA, distanceToSegmentPointB);

                if (distanceToSegmentPoint < rules.RoadSnapDistance
                    && projectedLineLength >= 0f
                    && projectedLineLength <= match.LineSegment2D.Length)
                {
                    Vector2 point = distanceToSegmentPoint == distanceToSegmentPointA ? segment.PointA : segment.PointB;

                    segment.LineSegment2D = new LineSegment2D(point, projectedPoint);
                    segment.HasBeenSplit = true;

                    // if intersecting lines are too similar don't continue
                    if (differenceInDegrees < rules.MinimumIntersectionAngleDifference)
                    {
                        Debug.LogWarning("#3");
                        return false;
                    }

                    match.Split(projectedPoint, segment);
                }

                // TODO: Consider factoring out the various checks into separate functions. -Casper 2017-09-18
                // too parallel check
/* 
                if (differenceInDegrees > 3f && differenceInDegrees < 15f)
                {
                    float shortestDistanceBetweenRoads = LineSegment2D.ShortestDistanceBetweenLineSegments(
                        segment.LineSegment2D, match.LineSegment2D);

                    if (shortestDistanceBetweenRoads < rules.MinimumDistanceBetweenRoads)
                    {
                        Debug.LogWarning("#4");
                        return false;
                    }
                }
 */
            }

            return true;
        }

        protected List<RoadSegment> ProposeNewSegmentsFromGlobalGoals(
            CityGeneratorRules rules, 
            RoadSegment previousSegment, 
            Texture2D populationHeatMap)
        {
            List<RoadSegment> newSegments = new List<RoadSegment>();

            if (!previousSegment.HasBeenSplit)
            {
                // template function for generating new segments based on previous segment
                Func<float, float, int, RoadType, RoadSegment> template = 
                    (float angle, float length, int priority, RoadType roadType) => 
                    {
                        return RoadSegment.UsingDirection(previousSegment.PointB, angle, length, priority, roadType, false);
                    };

                // template function for highways or going straight on a normal branch
                Func<float, RoadSegment> templateContinue =
                    (float angle) => 
                    {
                        return template(angle, previousSegment.LineSegment2D.Length, 0, previousSegment.RoadType);
                    };

                // template function for branching roads - highway branches have a lower priority (higher value)
                Func<float, RoadSegment> templateBranch =
                    (float angle) => 
                    {
                        int priority = 
                            previousSegment.RoadType == RoadType.Highway ? 
                            rules.HighwayBranchPriority 
                            : 0;

                        return template(angle, rules.DefaultRoadLengths[RoadType.Normal], priority, RoadType.Normal);
                    };

                // basic continuing road
                RoadSegment continueStraight = templateContinue(previousSegment.LineSegment2D.DirectionInDegrees);

                float continueStraightPopulation = CalculatePopulationForRoad(populationHeatMap, rules.CityBounds, continueStraight);

                // highway logic is more complex, can veer off by an angle and generate branches in 
                // high population areas
                if (previousSegment.RoadType == RoadType.Highway)
                {
                    float randomStraightAngle = GetRandomStraightAngle(rules);

                    // basic road with random offset angle
                    RoadSegment randomStraight = templateContinue(
                        previousSegment.LineSegment2D.DirectionInDegrees + randomStraightAngle);

                    float randomStraightPopulation = CalculatePopulationForRoad(populationHeatMap, rules.CityBounds, randomStraight);

                    // choose between continuing straight and veering off by a random amount, based on
                    // which road has access to more of the population
                    float roadPopulation = 0f;

                    if (randomStraightPopulation >= continueStraightPopulation)
                    {
                        newSegments.Add(randomStraight);
                        roadPopulation = randomStraightPopulation;
                    }
                    else
                    {
                        newSegments.Add(continueStraight);
                        roadPopulation = continueStraightPopulation;
                    }

                    // if the road population exceeds the necessary population threshold, generate 
                    // a branch
                    if (roadPopulation > rules.HighwayBranchPopulationThreshold
                        && UnityEngine.Random.value < rules.HighwayBranchProbability)
                    {
                        float randomBranchAngle = GetRandomBranchAngle(rules);

                        if (UnityEngine.Random.value < 0.5f)
                        {
                            RoadSegment leftHighwayBranch = templateContinue(
                                previousSegment.LineSegment2D.DirectionInDegrees - 90f + randomBranchAngle);
                            newSegments.Add(leftHighwayBranch);
                        }
                        else
                        {
                            RoadSegment rightHighwayBranch = templateContinue(
                                previousSegment.LineSegment2D.DirectionInDegrees + 90f + randomBranchAngle);
                            newSegments.Add(rightHighwayBranch);
                        }
                    }
                }
                // non-highways are much simpler, just continue straight as long as the population 
                // threshold is passed
                else if (continueStraightPopulation > rules.NormalBranchPopulationThreshold)
                {
                    newSegments.Add(continueStraight);
                }

                if (continueStraightPopulation > rules.NormalBranchPopulationThreshold)
                {
                    if (UnityEngine.Random.value < rules.NormalBranchProbability)
                    {
                        float randomBranchAngle = GetRandomBranchAngle(rules);

                        if (UnityEngine.Random.value < 0.5f)
                        {
                            RoadSegment leftBranch = templateBranch(
                                previousSegment.LineSegment2D.DirectionInDegrees - 90f + randomBranchAngle);
                            newSegments.Add(leftBranch);
                        }
                        else
                        {
                            RoadSegment rightBranch = templateBranch(
                                previousSegment.LineSegment2D.DirectionInDegrees + 90f + randomBranchAngle);
                            newSegments.Add(rightBranch);
                        }
                    }
                }
            }

            // pre-emptively link the new segment with the previous segment
            // NOTE: these links might be removed in the main algorithm if the new segment doesn't
            // pass local constraints.
            foreach (RoadSegment newSegment in newSegments)
            {
                foreach (RoadSegment link in previousSegment.LinksForward)
                {
                    newSegment.LinksBackward.Add(link);
                    link.LinksBackward.Add(newSegment);
                }

                previousSegment.LinksForward.Add(newSegment);
                newSegment.LinksBackward.Add(previousSegment);
            }

            return newSegments;
        }

        protected float CalculatePopulationForRoad(Texture2D heatMap, Rect cityBounds, RoadSegment segment)
        {
            return (GetPopulationAt(heatMap, cityBounds, segment.PointA) + GetPopulationAt(heatMap, cityBounds, segment.PointB)) / 2f;
        }

        protected float GetPopulationAt(Texture2D heatMap, Rect cityBounds, Vector2 position)
        {
            if (cityBounds.Contains(position))
            {
                Vector2 texturePosition = new Vector2(
                    (position.x / cityBounds.width) * heatMap.width,
                    (position.y / cityBounds.height) * heatMap.height);

                return heatMap.GetPixel((int)texturePosition.x, (int)texturePosition.y).r;
            }

            return 0f;
        }

        protected float GetRandomStraightAngle(CityGeneratorRules rules)
        {
            return UnityEngine.Random.Range(-rules.StraightRoadMaxDeviationAngle, rules.StraightRoadMaxDeviationAngle);
        }

        protected float GetRandomBranchAngle(CityGeneratorRules rules)
        {
            return UnityEngine.Random.Range(-rules.BranchRoadMaxDeviationAngle, rules.BranchRoadMaxDeviationAngle);
        }
    }
}
