using System;

namespace Natick.InfluenceMaps
{
    [Serializable]
    public record MapType
    {
        internal int Id { get; }
        
        public string Name { get; }
        
        public MapType(string name)
        {
            Id = name.GetHashCode();
            Name = name;
        }
    }
}