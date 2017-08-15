using UnityEngine;
using UnityEditor;
using AltSrc.UnityCommon.Utils;

namespace AltSrc.ProceduralCity.Generation
{
    public class CityGeneratorRules : ScriptableObject
    {
        // TODO: Implement. -Casper 2017-08-15

        public string CityName = "City01";

        [MenuItem("Assets/Create/CityGeneratorRules")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<CityGeneratorRules>();
        }
    }
}
