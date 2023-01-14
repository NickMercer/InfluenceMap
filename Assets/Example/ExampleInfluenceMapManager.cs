using System.Collections.Generic;
using Natick.InfluenceMaps;
using UnityEngine;

namespace Test
{
    public class ExampleInfluenceMapManager : InfluenceMapManager
    {
        //MapTypes
        public readonly static MapType Proximity = new MapType("Proximity");
        public readonly static MapType Threat = new MapType("Threat");
        public readonly static MapType Interest = new MapType("Interest");
        
        private void Start()
        {
            Initialize(new Vector2(100, 100), 1f, new Vector2Int(5, 5), new Vector2(0, 0));
            RegisterMapTypes(new List<MapType> {Proximity, Threat, Interest});
            
            CreateInfluenceStamps(Proximity, 12, 1, InfluenceCurve.Linear);
            CreateInfluenceStamps(Threat, 20, 1, InfluenceCurve.InverseFourPoly);
            CreateInfluenceStamps(Interest, 5, 1, InfluenceCurve.Quad);
            
            AddLayerMapCollection(Proximity);
            AddLayerMapCollection(Threat);
            AddLayerMapCollection(Interest);

            UpdateEntities();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                UpdateEntities();
                DrawMap(Proximity);
            }
        }
        
        protected override void ProcessInfluence(EntityInformation entityInfo, float magnitude)
        {
            if (entityInfo is ExampleAgentInformation == false)
                return;

            var influenceInfo = (ExampleAgentInformation)entityInfo;
            
            AddInfluenceSource(influenceInfo.Layer, influenceInfo.LastWorldLocation, Proximity, influenceInfo.Proximity.Value * magnitude,
                influenceInfo.Proximity.Key);
            AddInfluenceSource(influenceInfo.Layer, influenceInfo.LastWorldLocation, Threat, influenceInfo.Threat.Value * magnitude,
                influenceInfo.Threat.Key);
            AddInfluenceSource(influenceInfo.Layer, influenceInfo.LastWorldLocation, Interest, influenceInfo.Interest.Value * magnitude,
                influenceInfo.Interest.Key);
        }
    }
}