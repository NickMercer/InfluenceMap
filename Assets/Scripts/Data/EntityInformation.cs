using UnityEngine;

namespace Natick.InfluenceMaps
{
    public abstract class EntityInformation
    {
        public Vector2Int LastMapGrid = new Vector2Int(-1, -1);

        public Vector3 LastWorldLocation =
            new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        
        public void InvalidateLocation()
        {
            LastMapGrid = new Vector2Int(-1, -1);
            LastWorldLocation = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        }
        
        //Returns whether or not the updated entity information changes are significant enough for a repaint.
        public abstract bool UpdateRequiresRepaint(EntityInformation currentInformation);
    }
}