using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Natick.InfluenceMaps
{
    public class InfluenceService : MonoBehaviour
    {
        [SerializeField]
        private InfluenceCurveDefinitions _curveDefinitions;
        
        private static InfluenceService _inst;

        private static List<InfluenceStamp> _proximityStamps;
        
        private void Awake()
        {
            _inst = this;
            InitializeStamps();
        }
        
        #region Stamps
        
        private void InitializeStamps()
        {
            InitializeProximityStamps(12, 1);
        }

        private void InitializeProximityStamps(int maxRadius, int increment)
        {
            _proximityStamps = new List<InfluenceStamp>();
            for (var r = 1; r <= maxRadius; r+= increment)
            {
                var size = (2 * r) + 1;
                var newMap = new InfluenceMap(size, size, size/2, size/2);
                var newStamp = new InfluenceStamp();

                newMap.PropagateInfluenceFromCenter(InfluenceCurve.Linear, 1f);
                newStamp.Radius = r;
                newStamp.StampType = MapType.Proximity;
                newStamp.Map = newMap;
                
                _proximityStamps.Add(newStamp);
            }
        }

        private static InfluenceMap RetrieveInfluenceStamp(MapType mapType, int radius)
        {
            switch (mapType)
            {
                case MapType.Proximity:
                    var stamp = _proximityStamps.FirstOrDefault(x => x.Radius == radius);
                    var map = stamp.Map;
                    if(map == null)
                    {
                        stamp = _proximityStamps.OrderByDescending(x => x.Radius).First();
                    }
                    return stamp.Map;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }
        }

        #endregion
        
        #region Influence Curves
        
        public static bool TryGetCurve(InfluenceCurve curveType, out AnimationCurve curve)
        {
            if (_inst._curveDefinitions.CurveDefinitions.TryGetValue(curveType, out curve))
                return true;
            
            curve = AnimationCurve.Linear(1, 1, 0,0);
            Debug.LogError($"Tried to find Influence Curve of type {curveType} but one was not set up in the Influence Curve Definitions Scriptable Object.");
            return false;
        }
        
        #endregion

        public static void AddInfluenceSource(Vector3 worldPosition, InfluenceMap map, int influence, int radius)
        {
            var gridPos = new Vector2Int((int) worldPosition.x, (int) worldPosition.z);
            var stampMap = RetrieveInfluenceStamp(MapType.Proximity, radius);
            map.AddMap(stampMap, gridPos, influence);
        }
    }
}