using System;
using System.Collections.Generic;
using Natick.InfluenceMaps;
using UnityEngine;

namespace Test
{
    internal class LayerMapCollection
    {
        private Dictionary<Vector2Int, Dictionary<int, InfluenceMap>> _worldMaps;
        
        private readonly InfluenceMapManager _manager;

        public LayerMapCollection(InfluenceMapManager manager)
        {
            _manager = manager;
        }
        
        public void AddMapLayer(int layerId, Vector2Int gridPosition, InfluenceMap map)
        {
            if (_worldMaps.TryGetValue(gridPosition, out var layer) == false)
            {
                _worldMaps[gridPosition] = new Dictionary<int, InfluenceMap> {{layerId, map}};
                return;
            }
            
            if (layer.ContainsKey(layerId) == false)
            {
                layer[layerId] = map;
            }
        }
        
        public IEnumerable<KeyValuePair<int, InfluenceMap>> GetMapLayers(Vector2Int gridLocation,
            Func<KeyValuePair<int, InfluenceMap>, bool> filter = null)
        {
            if(_worldMaps.TryGetValue(gridLocation, out var layer))
            {
                foreach (var pair in layer)
                {
                    if (filter == null || filter(pair))
                    {
                        yield return pair;
                    }
                }
            }
        }
        
        internal InfluenceMap GetMapLayer(int layerId, Vector2Int gridPosition)
        {
            if (_worldMaps.TryGetValue(gridPosition, out var layer) == false)
                return null;

            return layer[layerId];
        }
    }
}