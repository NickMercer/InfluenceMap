using Natick;
using Natick.InfluenceMaps;
using UnityEngine;

namespace Test
{
    public class TestBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Gradient _influenceMapGradient;

        private InfluenceMap _influenceMap;

        private void Start()
        {
            var agents = FindObjectsOfType<TestAgent>();
            _influenceMap = new InfluenceMap(100, 100);
            foreach (var agent in agents)
            {
                var agentPos = agent.transform.position;
                InfluenceService.AddInfluenceSource(new Vector2Int((int)agentPos.x, (int)agentPos.z), _influenceMap, agent.InfluenceStrength, agent.InfluenceRadius);
            }

            var subMap = InfluenceService.CreateSubMap(20, 20, _influenceMap, 60, 50);
            //InfluenceService.AddInfluenceSource(new Vector2Int(10, 10), subMap, 30, 10);
            //_influenceMap = subMap;
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
                    var worldPosX = (int)influenceMap.BottomLeft.x + x;
                    var worldPosZ = (int)influenceMap.BottomLeft.y + y;
                    var influence = influenceMap.GetValue(new Vector2Int(x, y));
                    Gizmos.color = _influenceMapGradient.Evaluate(influence.Normalize(-30, 30));
                    Gizmos.DrawCube(new Vector3(worldPosX, -1, worldPosZ), new Vector3(influenceMap.CellSize, 0.01f, influenceMap.CellSize));
                }
            }
        }
    }
}