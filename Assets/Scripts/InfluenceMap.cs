using UnityEngine;

namespace Natick.InfluenceMaps
{
    public class InfluenceMap
    {
        public Vector2 Center { get; }
        public float CellSize { get; }
        public int Height { get; }
        public int Width { get; }
        private float[,] _grid;

        public InfluenceMap(int width, int height, float xCenter = 0f,
            float yCenter = 0f, float cellSize = 1f)
        {
            Width = width;
            Height = height;
            Center = new Vector2(xCenter, yCenter);
            CellSize = cellSize;
            _grid = new float[Width, Height];
        }
        
        #region Value Manipulation
        
        internal void SetValue(Vector2Int cell, float value)
        {
            if (InBounds(cell) == false)
                return;
            
            _grid[cell.x, cell.y] = value;
        }
    
        internal float GetValue(Vector2Int cell)
        {
            if (InBounds(cell) == false)
                return 0f;
    
            return _grid[cell.x, cell.y];
        }
    
        internal void AddValue(Vector2Int cell, float value)
        {
            if (InBounds(cell) == false)
                return;
    
            _grid[cell.x, cell.y] += value;
        }
        
        internal void SubtractValue(Vector2Int cell, float value)
        {
            if (InBounds(cell) == false)
                return;
    
            _grid[cell.x, cell.y] -= value;
        } 
        
        #endregion
        
        #region Value Propagation
        
        internal void PropagateInfluenceFromCenter(InfluenceCurve curveType, float maxValue)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var distance = Vector2.Distance(Center, new Vector2(x, y));
                    var percent = Mathf.InverseLerp(Width / 2 - Center.x, Center.x, distance);
                    if (InfluenceService.TryGetCurve(curveType, out var curve))
                    {
                        _grid[x, y] = curve.Evaluate(percent) * maxValue;
                    }
                }
            }
        }

        #endregion
        
        #region Map Combination

        internal void AddMap(InfluenceMap sourceMap, Vector2Int center, float magnitude = 1f,
            Vector2Int offset = default)
        {
            if (sourceMap == null)
            {
                Debug.LogError("sourceMap was null");
                return;
            }

            var startX = center.x + offset.x - (sourceMap.Width / 2);
            var startY = center.y + offset.y - (sourceMap.Height / 2);

            for (var y = 0; y < sourceMap.Height; y++)
            {
                for (var x = 0; x < sourceMap.Width; x++)
                {
                    var targetX = x + startX;
                    var targetY = y + startY;

                    if (InBounds(new Vector2Int(targetX, targetY)))
                    {
                        _grid[targetX, targetY] += sourceMap.GetValue(new Vector2Int(x, y)) * magnitude;
                    }
                }
            }
        }
        
        #endregion
        
        #region Helpers
        
        private bool InBounds(Vector2Int cell)
        {
            var x = cell.x;
            var y = cell.y; 
            return (x >= 0 && x < Width && y >= 0 && y < Height);
        }
        
        #endregion
    }
}

