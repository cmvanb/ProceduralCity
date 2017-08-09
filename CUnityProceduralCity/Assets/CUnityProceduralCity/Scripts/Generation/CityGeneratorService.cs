using UnityEngine;

namespace CUnity.ProceduralCity.Generation
{
    public class CityGeneratorService
    {
        public GameObject Generate(GeneratorRules rules)
        {
            GameObject cityObject = new GameObject(rules.CityName);

            cityObject.transform.position = rules.CenterPosition;

            // Generate roads model.
            RoadsModel roadsModel = new RoadsModel();

            roadsModel.SetScale(rules.CityScale);

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

            roadsViewObject.transform.parent = cityObject.transform;
            roadsViewObject.transform.localPosition = Vector3.zero;

            RoadsView roadsView = roadsViewObject.AddComponent<RoadsView>();

            roadsView.Initialize(roadsModel);

            // Populate city with building objects.

            return cityObject;
        }
    }
}
