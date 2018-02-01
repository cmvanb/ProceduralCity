using UnityEngine;
using Zenject;

namespace AltSrc.ProceduralCity.Generation
{
    public class CityGeneratorInstaller : Installer<CityGeneratorInstaller>
    {
        public override void InstallBindings()
        {
            // TODO: bind materials here
            //Container.Bind<Material>().AsTransient();
        }
    }
}
