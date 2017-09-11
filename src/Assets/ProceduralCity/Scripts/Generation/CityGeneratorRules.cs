using System;
using UnityEngine;
using UnityEditor;
using AltSrc.UnityCommon.Collections;
using AltSrc.UnityCommon.Utils;
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
        public DictionaryRoadTypeFloat DefaultRoadLengths = new DictionaryRoadTypeFloat();

        [SerializeField]
        public Rect QuadTreeRect;

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
        public float RoadSnapDistance;

        [MenuItem("Assets/Create/CityGeneratorRules")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<CityGeneratorRules>();
        }
    }
}
