﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

            // TODO: setup model with rules data

            // TODO: random seed

            // TODO: Consider splitting out road generation into seperate function. -Casper 2017-08-17

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

                // remove next segment from queue
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

        protected delegate bool ActionDelegate();

        protected bool CheckLocalConstraints(
            RoadSegment segment,
            List<RoadSegment> generatedSegments,
            QuadTree<RoadSegment> quadTree)
        {
            var actionPriority = 0;

            ActionDelegate actionFunc = null;

            List<RoadSegment> matches = quadTree.Retrieve(segment);

            foreach (RoadSegment match in matches)
            {
                // intersection check
                if (actionPriority <= 4)
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
                        actionPriority = 4;
                        actionFunc = () =>
                        {
                            // if intersecting lines are too similar don't continue
                            float differenceInDegrees = LineSegment2D.MinimumAngleDifferenceInDegrees(
                                segment.LineSegment2D,
                                match.LineSegment2D);

                            if (differenceInDegrees < this.rules.MinimumIntersectionAngleDifference)
                            {
                                // TODO: This won't function correctly as it is based on coffeescript
                                // functionality that differs from C#. -Casper 2017-09-11
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
                if (actionPriority <= 3)
                {
                    float distance = Vector2.Distance(segment.PointB, match.PointB);

                    if (distance <= this.rules.RoadSnapDistance)
                    {
                        actionPriority = 3;
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
                                // TODO: This won't function correctly as it is based on coffeescript
                                // functionality that differs from C#. -Casper 2017-09-11
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
                if (actionPriority <= 2)
                {
                    Vector2 projectedPoint = Vector2.zero;

                    float projectedLineLength = Mathf.Infinity;

                    float distance = LineSegment2D.FindDistanceToPoint(
                        match.LineSegment2D,
                        segment.PointB,
                        out projectedPoint,
                        out projectedLineLength);

                    if (distance < this.rules.RoadSnapDistance
                        && projectedLineLength >= 0f
                        && projectedLineLength <= match.LineSegment2D.Length)
                    {
                        actionPriority = 2;
                        actionFunc = () =>
                        {
                            segment.LineSegment2D = new LineSegment2D(segment.PointA, projectedPoint);
                            segment.HasBeenSplit = true;

                            // if intersecting lines are too similar don't continue
                            float differenceInDegrees = LineSegment2D.MinimumAngleDifferenceInDegrees(
                                segment.LineSegment2D,
                                match.LineSegment2D);

                            if (differenceInDegrees < this.rules.MinimumIntersectionAngleDifference)
                            {
                                // TODO: This won't function correctly as it is based on coffeescript
                                // functionality that differs from C#. -Casper 2017-09-11
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
                actionFunc();
            }

            return true;
        }

        protected List<RoadSegment> GenerateNewSegmentsFromGlobalGoals(RoadSegment previousSegment)
        {
            // TODO: implement

            return new List<RoadSegment>();
        }
    }
}
