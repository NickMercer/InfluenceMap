using UnityEngine;

namespace Test
{
    public class TestAgent : MonoBehaviour
    {
        [field: SerializeField]
        public int InfluenceStrength = 10;

        [field: SerializeField]
        public int InfluenceRadius = 6;
    }
}