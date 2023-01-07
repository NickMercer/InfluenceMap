using System;
using Natick.InfluenceMaps;
using UnityEngine;

namespace Test
{
    public class TestAgent : MonoBehaviour, IInfluenceEntity
    {
        [field: SerializeField]
        public int ProximityRadius { get; set; } = 0;
     
        [field: SerializeField]
        public int ProximityStrength { get; set; } = 0;

        [field: SerializeField]
        public int ThreatRadius { get; set; } = 0;
     
        [field: SerializeField]
        public int ThreatStrength { get; set; } = 0;

        [field: SerializeField]
        public int InterestRadius { get; set; } = 0;
     
        [field: SerializeField]
        public int InterestStrength { get; set; } = 0;
        
        [SerializeField]
        private bool _isRegisterable;

        [SerializeField]
        private ExampleInfluenceMapManager _exampleMap;
        
        public bool IsRegisterable() => _isRegisterable;

        private void OnEnable()
        {
            _exampleMap.RegisterEntity(this);
        }

        private void OnDisable()
        {
            _exampleMap.UnregisterEntity(this);
        }

        public EntityInformation GetEntityInformation()
        {
            return new ExampleAgentInformation(
                ProximityRadius, ProximityStrength,
                ThreatRadius, ThreatStrength,
                InterestRadius, InterestStrength)
            {
                LastWorldLocation = transform.position,
            };
        }
    }
}