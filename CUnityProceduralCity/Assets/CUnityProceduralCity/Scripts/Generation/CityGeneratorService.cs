using UnityEngine;

namespace CUnityProceduralCity
{
    public class CityGeneratorService
    {
        public GameObject Generate(GeneratorRules rules)
        {
            GameObject cityObject = new GameObject(rules.CityName);

            cityObject.transform.position = rules.CenterPosition;

            // Generate roads model.
            RoadsModel roadsModel = new RoadsModel(rules.CityScale);

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
            /*
            this.roadRenderer = this.GetComponent<RoadRenderer>();
            this.roadRenderer.ClearData();

            foreach (RoadSegment segment in this.network.RoadSegments)
            {
                this.roadRenderer.AddRoadSegments(segment);
            }

            foreach (Intersection inter in this.network.RoadIntersections)
            {
                this.roadRenderer.AddIntersection(inter);
            }
            */

            // Populate city with building objects.

            return cityObject;
        }
    }
}
