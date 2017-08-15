using UnityEngine;
using Zenject;

namespace AltSrc.ProceduralCity.Generation
{
    public class ProceduralCityInstaller : MonoInstaller<ProceduralCityInstaller>
    {
        public override void InstallBindings()
        {
            CityGeneratorInstaller.Install(Container);
        }
    }
}
