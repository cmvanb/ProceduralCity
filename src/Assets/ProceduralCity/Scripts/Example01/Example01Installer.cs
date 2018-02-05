using UnityEngine;
using Zenject;
using AltSrc.ProceduralCity.Generation;

namespace AltSrc.ProceduralCity.Example01
{
    public class Example01Installer : MonoInstaller<Example01Installer>
    {
        [SerializeField]
        protected CityGenerator cityGenerator;

        public override void InstallBindings()
        {
            Container.Bind<CityGenerator>().FromInstance(cityGenerator).AsSingle();
        }
    }
}
