using UnityEngine;
using Zenject;

namespace AltSrc.ProceduralCity.Generation
{
    public class CityModel
    {
        public string CityName { get; private set; }

        [Inject]
        public CityModel()
        {
            Debug.Log("CityModel constructor");
        }
    }
}
