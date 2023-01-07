using UnityEngine;

namespace Natick.InfluenceMaps
{
    public class InfluenceService : MonoBehaviour
    {
        [SerializeField]
        private InfluenceCurveDefinitions _curveDefinitions;
        
        private static InfluenceService _inst;
        
        private void Awake()
        {
            _inst = this;
        }

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
        
        public static InfluenceMap CreateSubMap(int width, int height, InfluenceMap largerMap, int startX, int startY)
        {
            var subMap = new InfluenceMap(width, height, startX, startY);
            largerMap.AddIntoMap(subMap, new Vector2Int(startX, startY));
            return subMap;
        }
    }
}