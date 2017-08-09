using UnityEngine;

namespace CUnity.ProceduralCity.Generation
{
    public class CityGeneratorService
    {
        public GameObject Generate(GeneratorRules rules)
        {
            // Generator city view (a game object).
            GameObject cityViewObject = new GameObject(rules.CityName);

            cityViewObject.transform.position = rules.CenterPosition;

            // Generate roads model.
            RoadsModel roadsModel = new RoadsModel(rules);

            roadsModel.CreateCenter(
                rules.CenterShape,
                rules.CenterPosition,
                rules.CenterAngle);

            roadsModel.SplitSegments(0);
            roadsModel.SplitSegments(0);
            roadsModel.SplitSegments(1);
            roadsModel.SplitSegments(1);
            roadsModel.SplitSegments(2);
            roadsModel.SplitSegments(3);

            // Generate roads view (a game object).
            GameObject roadsViewObject = new GameObject("RoadsView");

            roadsViewObject.transform.parent = cityViewObject.transform;
            roadsViewObject.transform.localPosition = Vector3.zero;

            RoadsView roadsView = roadsViewObject.AddComponent<RoadsView>();

            roadsView.Initialize(roadsModel);

            // Populate city with building objects.

            return cityViewObject;
        }
    }
}
