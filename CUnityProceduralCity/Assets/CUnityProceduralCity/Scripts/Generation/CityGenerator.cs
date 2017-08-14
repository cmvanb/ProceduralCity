using UnityEngine;

namespace CUnity.ProceduralCity.Generation
{
    public class CityGenerator : MonoBehaviour
    {
        [SerializeField]
        protected GeneratorRules Rules;

        [SerializeField]
        protected bool generateOnStart = false;

        public void Start()
        {
            if (generateOnStart)
            {
                Generate();
            }
        }

        public void Generate()
        {
            CityGeneratorService service = new CityGeneratorService();

            service.Generate(this.Rules);
        }
    }
}
