using System;
using System.Collections.Generic;
using Natick.InfluenceMaps;

namespace Test
{
    public class ExampleInfluenceMapManager : InfluenceMapManager
    {
        private List<InfluenceStamp> _proximityStamps;
        private List<InfluenceStamp> _threatStamps;
        private List<InfluenceStamp> _interestStamps;

        private LayerMapCollection _proximityMaps;
        private LayerMapCollection _threatMaps;
        private LayerMapCollection _interestMaps;
        internal override LayerMapCollection GetLayerMapCollection(MapType mapType)
        {
            switch (mapType)
            {
                case MapType.Proximity:
                    return _proximityMaps;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }
        }

        protected override void ProcessInfluence(EntityNode entity, float magnitude)
        {
            
        }
    }
}