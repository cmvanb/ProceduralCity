using UnityEngine;

namespace CUnityProceduralCity
{
    public class CityGenerator : MonoBehaviour
    {
        public GeneratorRules Rules = new GeneratorRules();

        public void Generate()
        {
            CityGeneratorService service = new CityGeneratorService();

            GameObject cityObject = service.Generate(this.Rules);
        }
    }
}
