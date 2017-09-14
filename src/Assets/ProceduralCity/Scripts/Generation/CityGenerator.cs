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

            Texture2D result = TextureCreator.Create(resolution);

            return result;
        }

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
                // find highest priority (lowest value) segment and remove from queue
                RoadSegment nextSegment = null;

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
                    // TODO: Implement RoadSegment.SetupBranchLinks
                    //nextSegment.SetupBranchLinks();

                    // add segment to list and quad tree
                    generatedSegments.Add(nextSegment);
                    quadTree.Insert(nextSegment);

                    // propose new segments based on global goals and add to priority queue
                    List<RoadSegment> newSegments = ProposeNewSegmentsFromGlobalGoals(rules, nextSegment, populationHeatMap);

                    foreach (RoadSegment s in newSegments)
                    {
                        s.Priority += nextSegment.Priority + 1;
                        priorityQueue.Add(nextSegment);
                    }
                }
            }

            Debug.Log(generatedSegments.Count + " segments generated.");

            return generatedSegments;
        }

        protected void GenerateBuildings()
        {
            // TODO: generate building rects

            // TODO: add building rects to model

            throw new System.NotImplementedException();
        }

        protected void BuildView()
        {
            // TODO: hookup view with model

            // TODO: generate view objects

            throw new System.NotImplementedException();
        }

        protected bool CheckLocalConstraints(
            CityGeneratorRules rules,
            RoadSegment segment,
            List<RoadSegment> generatedSegments,
            QuadTree<RoadSegment> quadTree)
        {
            int actionPriority = 0;

            Func<bool> actionFunc = null;

            List<RoadSegment> matches = quadTree.Retrieve(segment);

            foreach (RoadSegment match in matches)
            {
                // intersection check
                if (actionPriority <= 3)
                {
                    Vector2 intersectionPoint0, intersectionPoint1;

                    int intersecting = LineSegment2D.CheckIntersection(
                        segment.LineSegment2D,
                        match.LineSegment2D,
                        out intersectionPoint0,
                        out intersectionPoint1);

                    // 1 means the line segments intersect but don't overlap
                    if (intersecting == 1)
                    {
                        actionPriority = 3;
                        actionFunc = () =>
                        {
                            // if intersecting lines are too similar don't continue
                            float differenceInDegrees = LineSegment2D.MinimumAngleDifferenceInDegrees(
                                segment.LineSegment2D,
                                match.LineSegment2D);

                            if (differenceInDegrees < rules.MinimumIntersectionAngleDifference)
                            {
                                return false;
                            }

                            // perform split of intersecting segments
                            RoadSegment splitSegment = match.Split(intersectionPoint0, segment);
                            segment.LineSegment2D = new LineSegment2D(
                                segment.LineSegment2D.PointA,
                                intersectionPoint0);

                            segment.HasBeenSplit = true;

                            // accept split segment into list and quad tree
                            generatedSegments.Add(splitSegment);
                            quadTree.Insert(splitSegment);

                            return true;
                        };
                    }
                }
                // snap to crossing within radius check
                if (actionPriority <= 2)
                {
                    float distance = Vector2.Distance(segment.PointB, match.PointB);

                    if (distance <= rules.RoadSnapDistance)
                    {
                        actionPriority = 2;
                        actionFunc = () =>
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
                                return false;
                            }

                            foreach (RoadSegment link in links)
                            {
                                var linksContainingMatch = link.GetListOfLinksContainingSegment(match);

                                // pick links of remaining segments at junction corresponding to other.r.end
                                if (linksContainingMatch != null)
                                {
                                    // TODO: Verify this works as expected (pass by ref). -Casper 2017-09-11
                                    linksContainingMatch.Add(match);
                                }

                                // add junction segments to snapped segment
                                segment.LinksForward.Add(link);
                            }

                            links.Add(segment);
                            segment.LinksForward.Add(match);

                            return true;
                        };
                    }
                }
                // intersection within radius check
                if (actionPriority <= 1)
                {
                    Vector2 projectedPoint = Vector2.zero;

                    float projectedLineLength = Mathf.Infinity;

                    float distance = LineSegment2D.FindDistanceToPoint(
                        match.LineSegment2D,
                        segment.PointB,
                        out projectedPoint,
                        out projectedLineLength);

                    if (distance < rules.RoadSnapDistance
                        && projectedLineLength >= 0f
                        && projectedLineLength <= match.LineSegment2D.Length)
                    {
                        actionPriority = 1;
                        actionFunc = () =>
                        {
                            segment.LineSegment2D = new LineSegment2D(segment.PointA, projectedPoint);
                            segment.HasBeenSplit = true;

                            // if intersecting lines are too similar don't continue
                            float differenceInDegrees = LineSegment2D.MinimumAngleDifferenceInDegrees(
                                segment.LineSegment2D,
                                match.LineSegment2D);

                            if (differenceInDegrees < rules.MinimumIntersectionAngleDifference)
                            {
                                return false;
                            }

                            match.Split(projectedPoint, segment);

                            return true;
                        };
                    }
                }
            }

            if (actionFunc != null)
            {
                return actionFunc();
            }

            return true;
        }

        protected List<RoadSegment> ProposeNewSegmentsFromGlobalGoals(
            CityGeneratorRules rules, 
            RoadSegment previousSegment, 
            Texture2D populationHeatMap)
        {
            List<RoadSegment> newBranches = new List<RoadSegment>();

            if (!previousSegment.HasBeenSplit)
            {
                // template function for generating new segments based on previous segment
                Func<float, float, int, RoadType, RoadSegment> template = 
                    (float angle, float length, int priority, RoadType roadType) => 
                    {
                        return RoadSegment.UsingDirection(previousSegment.PointB, angle, length, priority, roadType, false);
                    };

                // used for highways or going straight on a normal branch
                Func<float, RoadSegment> templateContinue =
                    (float angle) => 
                    {
                        return template(angle, previousSegment.LineSegment2D.Length, 0, previousSegment.RoadType);
                    };

                // used for branching roads - highway branches have a lower priority (higher value)
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

                // NOTE: previously named straightPop
                float continueStraightPopulation = CalculatePopulationForRoad(populationHeatMap, rules.CityBounds, continueStraight);

                // highway logic is more complex, can veer off by an angle and generate branches in 
                // high population areas
                if (previousSegment.RoadType == RoadType.Highway)
                {
                    // TODO: Calculate randomStraightAngle. See citygen config.coffee. -Casper 2017-09-14
                    float randomStraightAngle = 0f;

                    // basic road with random offset angle
                    RoadSegment randomStraight = templateContinue(
                        previousSegment.LineSegment2D.DirectionInDegrees + randomStraightAngle);

                    float randomStraightPopulation = CalculatePopulationForRoad(populationHeatMap, rules.CityBounds, randomStraight);

                    // choose between continuing straight and veering off by a random amount, based on
                    // which road has access to more of the population
                    float roadPopulation = 0f;

                    if (randomStraightPopulation > continueStraightPopulation)
                    {
                        newBranches.Add(randomStraight);
                        roadPopulation = randomStraightPopulation;
                    }
                    else
                    {
                        newBranches.Add(continueStraight);
                        roadPopulation = continueStraightPopulation;
                    }

                    // if the road population exceeds the necessary population threshold, generate 
                    // a branch
                    if (roadPopulation > rules.HighwayBranchPopulationThreshold
                        && UnityEngine.Random.value < rules.HighwayBranchProbability)
                    {
                        // TODO: Calculate randomBranchAngle. See citygen config.coffee. -Casper 2017-09-14
                        float randomBranchAngle = 0f;

                        if (UnityEngine.Random.value < 0.5f)
                        {
                            RoadSegment leftHighwayBranch = templateContinue(
                                previousSegment.LineSegment2D.DirectionInDegrees - 90f + randomBranchAngle);
                            newBranches.Add(leftHighwayBranch);
                        }
                        else
                        {
                            RoadSegment rightHighwayBranch = templateContinue(
                                previousSegment.LineSegment2D.DirectionInDegrees + 90f + randomBranchAngle);
                            newBranches.Add(rightHighwayBranch);
                        }
                    }
                }
                // non-highways are much simpler, just continue straight as long as the population 
                // threshold is passed
                else if (continueStraightPopulation > rules.NormalBranchPopulationThreshold)
                {
                    newBranches.Add(continueStraight);

                    if (UnityEngine.Random.value < rules.NormalBranchProbability)
                    {
                        // TODO: Calculate randomBranchAngle. See citygen config.coffee. -Casper 2017-09-14
                        float randomBranchAngle = 0f;

                        if (UnityEngine.Random.value < 0.5f)
                        {
                            RoadSegment leftBranch = templateBranch(
                                previousSegment.LineSegment2D.DirectionInDegrees - 90f + randomBranchAngle);
                            newBranches.Add(leftBranch);
                        }
                        else
                        {
                            RoadSegment rightBranch = templateBranch(
                                previousSegment.LineSegment2D.DirectionInDegrees + 90f + randomBranchAngle);
                            newBranches.Add(rightBranch);
                        }
                    }
                }
            }

            // TODO: Implement. -Casper 2017-09-14
            /*
          for i in [0..newBranches.length-1] by 1
            do (branch = newBranches[i]) ->
              branch.setupBranchLinks = ->
                # setup links between each current branch and each existing branch stemming from the previous segment
                _.each(previousSegment.links.f, (link) ->
                  @links.b.push(link)
                  link.linksForEndContaining(previousSegment).push(this)
                , @)

                previousSegment.links.f.push(@)
                @links.b.push(previousSegment)
               */

            return newBranches;
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
                    (position.x - cityBounds.x) / cityBounds.width,
                    (position.y - cityBounds.y) / cityBounds.height);

                return heatMap.GetPixel((int)texturePosition.x, (int)texturePosition.y).r;
            }

            return 0f;
        }
    }
}
