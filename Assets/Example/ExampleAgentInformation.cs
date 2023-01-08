using System;
using System.Collections.Generic;
using Natick.InfluenceMaps;

namespace Test
{
    public class ExampleAgentInformation : EntityInformation<ExampleAgentInformation>
    {
        public int Layer { get; set; }
        
        public KeyValuePair<int, float> Proximity { get; set; }
        
        public KeyValuePair<int, float> Threat { get; set; }
        
        public KeyValuePair<int, float> Interest { get; set; }
        
        public ExampleAgentInformation(int layer, int proximityRadius, float proximityStrength, int threatRadius, float threatStrength, int interestRadius, float interestStrength)
        {
            Layer = layer;
            Proximity = new KeyValuePair<int, float>(proximityRadius, proximityStrength);
            Threat = new KeyValuePair<int, float>(threatRadius, threatStrength);
            Interest = new KeyValuePair<int, float>(interestRadius, interestStrength);
        }
        
        public override bool UpdateRequiresRepaint(EntityInformation currentInformation)
        {
            var convertedInfo = ConvertInfo(currentInformation);

            if (Proximity.Key != convertedInfo.Proximity.Key)
                return true;

            if (Math.Abs(Proximity.Value - convertedInfo.Proximity.Value) > 0.1f)
                return true;
            
            if (Threat.Key != convertedInfo.Threat.Key)
                return true;
            
            if (Math.Abs(Threat.Value - convertedInfo.Threat.Value) > 0.1f)
                return true;
            
            if (Interest.Key != convertedInfo.Interest.Key)
                return true;

            if (Math.Abs(Interest.Value - convertedInfo.Interest.Value) > 0.1f)
                return true;

            return false;
        }
    }
}