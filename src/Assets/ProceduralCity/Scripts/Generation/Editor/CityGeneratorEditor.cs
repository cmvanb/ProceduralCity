using UnityEngine;
using UnityEditor;
using Zenject;

namespace AltSrc.ProceduralCity.Generation
{
    [CustomEditor(typeof(CityGenerator))]
    public class CityGeneratorEditor : Editor
    {
        protected DiContainer container;

        public void OnEnable()
        {
            this.container = new DiContainer();

            CityGeneratorInstaller.Install(container);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Generate"))
            {
                CityGenerator generator = (CityGenerator)target;

                this.container.Inject(generator);

                generator.Generate();
            }
        }
    }
}
