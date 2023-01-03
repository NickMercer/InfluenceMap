using UnityEngine;

namespace Natick.InfluenceMaps
{
    [CreateAssetMenu(fileName = "Influence Curve Definitions", menuName = "Influence Maps/Curve Definitions", order = 0)]
    public class InfluenceCurveDefinitions : ScriptableObject
    {
        [SerializeField]
        private UDictionary<InfluenceCurve, AnimationCurve> _curveDefinitions;
        public UDictionary<InfluenceCurve, AnimationCurve> CurveDefinitions => _curveDefinitions;
    }
}