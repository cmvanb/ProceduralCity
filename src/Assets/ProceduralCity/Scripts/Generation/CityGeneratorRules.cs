using System;
using UnityEngine;
using UnityEditor;
using AltSrc.UnityCommon.Collections;
using AltSrc.UnityCommon.Data;
using AltSrc.ProceduralCity.Generation.Roads;

namespace AltSrc.ProceduralCity.Generation
{
    [Serializable]
    public class CityGeneratorRules : ScriptableObject
    {
        // NOTE: 'Fun' hack, because Unity3D doesn't support serializable dictionaries. -Casper 2017-08-16
        [Serializable]
        public class DictionaryRoadTypeFloat : SerializableDictionaryBase<RoadType, float> {}

        [SerializeField]
        public string CityName;

        [SerializeField]
        public Rect CityBounds;

        // TODO: Consider renaming var to QuadTreeMaxSegmentsPerNode. -Casper 2017-08-17
        [SerializeField]
        public int QuadTreeMaxObjectsPerNode;

        [SerializeField]
        public int QuadTreeMaxDepth;

        [SerializeField]
        public int MaxRoadSegments;

        [SerializeField]
        public float MinimumIntersectionAngleDifference;

        [SerializeField]
        public float MinimumDistanceBetweenRoads;

        [SerializeField]
        public float RoadSnapDistance;

        [SerializeField]
        public Texture2D PopulationHeatMap;

        [SerializeField]
        public int HighwayBranchPriority;

        [SerializeField]
        public float HighwayBranchPopulationThreshold;

        [SerializeField]
        public float HighwayBranchProbability;

        [SerializeField]
        public float NormalBranchPopulationThreshold;

        [SerializeField]
        public float NormalBranchProbability;

        [SerializeField]
        public float StraightRoadMaxDeviationAngle;

        [SerializeField]
        public float BranchRoadMaxDeviationAngle;

        [SerializeField]
        public DictionaryRoadTypeFloat DefaultRoadWidths = new DictionaryRoadTypeFloat();

        [SerializeField]
        public DictionaryRoadTypeFloat DefaultRoadLengths = new DictionaryRoadTypeFloat();

        [MenuItem("Assets/Create/CityGeneratorRules")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<CityGeneratorRules>();
        }
    }
}
