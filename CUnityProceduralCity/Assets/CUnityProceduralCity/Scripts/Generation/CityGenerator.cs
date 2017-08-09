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

            GameObject cityObject = service.Generate(this.Rules);
        }
    }
}
