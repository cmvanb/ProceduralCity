using UnityEngine;

namespace CUnity.ProceduralCity.Generation
{
    public class CityGenerator : MonoBehaviour
    {
        [SerializeField]
        protected GeneratorRules Rules;

        public void Generate()
        {
            CityGeneratorService service = new CityGeneratorService();

            service.Generate(this.Rules);
        }
    }
}
