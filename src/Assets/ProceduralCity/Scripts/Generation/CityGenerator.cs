using UnityEngine;
using Zenject;

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

            // TODO: setup root segment

            // TODO: generate model roads

            // TODO: generate model buildings

            // TODO: setup view with model

            // TODO: generate view objects
        }
    }
}
