using UnityEngine;
using Zenject;
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

            // TODO: setup root segment
            var length = this.rules.DefaultRoadLengths[RoadType.Highway];

            var rootSegment = new RoadSegment(
                new Vector2(0f, 0f),
                new Vector2(length, 0f),
                0,
                RoadType.Highway);

            // TODO: generate model roads

            // TODO: generate model buildings

            // TODO: setup view with model

            // TODO: generate view objects
        }
    }
}
