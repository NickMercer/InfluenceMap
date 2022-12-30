using System;
using Natick;
using Natick.InfluenceMaps;
using UnityEditor;
using UnityEngine;

namespace Test
{
    public class TestBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Gradient _influenceMapGradient;

        private InfluenceMap _influenceMap;
        
        private void Awake()
        {
            var agents = FindObjectsOfType<TestAgent>();
            _influenceMap = new InfluenceMap(100, 100, xCenter: 50, yCenter: 50);
            foreach (var agent in agents)
            {
                InfluenceService.AddInfluenceSource(agent.transform.position, _influenceMap, agent.InfluenceStrength, agent.InfluenceRadius);
            }
            
            _influenceMap.SetValue(new Vector2Int(30, 30), -10);
        }

        private void OnDrawGizmos()
        {
            if(_influenceMap != null)
                DrawInfluenceMap(_influenceMap);
        }

        private void DrawInfluenceMap(InfluenceMap influenceMap)
        {
            for (var y = 0; y < influenceMap.Height; y++)
            {
                for (var x = 0; x < influenceMap.Width; x++)
                {
                    var worldPosX = (int)influenceMap.Center.x - influenceMap.Width / 2 + x;
                    var worldPosZ = (int)influenceMap.Center.y - influenceMap.Height / 2 + y;
                    var influence = influenceMap.GetValue(new Vector2Int(worldPosX, worldPosZ));
                    Gizmos.color = _influenceMapGradient.Evaluate(influence.Normalize(-10, 10));
                    Gizmos.DrawCube(new Vector3(worldPosX, -1, worldPosZ), new Vector3(influenceMap.CellSize, 0.01f, influenceMap.CellSize));
                }
            }
        }
    }
}