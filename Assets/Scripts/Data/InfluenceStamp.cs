using System;

namespace Natick.InfluenceMaps
{
    [Serializable]
    public struct InfluenceStamp
    {
        public int Radius;
        public MapType StampType;
        [NonSerialized]
        public InfluenceMap Map;
    }
}