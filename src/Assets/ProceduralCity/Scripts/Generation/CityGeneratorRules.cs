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

        [MenuItem("Assets/Create/CityGeneratorRules")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<CityGeneratorRules>();
        }
    }
}
