using UnityEngine;

namespace Natick.InfluenceMaps
{
    public class InfluenceMap
    {
        public Vector2 Center { get; }
        public Vector2 BottomLeft { get; }
        public float CellSize { get; }
        public int Height { get; }
        public int Width { get; }
        private float[,] _grid;

        public InfluenceMap(int width, int height, float startX = 0f,
            float startY = 0f, float cellSize = 1f)
        {
            Width = width;
            Height = height;
            BottomLeft = new Vector2(startX, startY);
            Center = new Vector2(startX + Width/2f, startY + Height/2f);
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
                    var percent = Mathf.InverseLerp(BottomLeft.x, Center.x, distance);
                    if (InfluenceService.TryGetCurve(curveType, out var curve))
                    {
                        _grid[x, y] = curve.Evaluate(percent) * maxValue;
                    }
                }
            }
        }

        #endregion
        
        #region Map Combination

        internal void AddMap(InfluenceMap sourceMap, Vector2Int bottomLeft, float magnitude = 1f,
            Vector2Int offset = default)
        {
            if (sourceMap == null)
            {
                Debug.LogError("sourceMap was null");
                return;
            }

            var startX = bottomLeft.x + offset.x;
            var startY = bottomLeft.y + offset.y;

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

        internal void AddIntoMap(InfluenceMap targetMap, Vector2Int bottomLeft, float magnitude = 1f,
            Vector2Int offset = default)
        {
            if (targetMap == null)
            {
                Debug.LogError("targetMap was null");
                return;
            }

            var startX = bottomLeft.x + offset.x;
            var startY = bottomLeft.y + offset.y;

            var minX = Mathf.Max(0, startX);
            var minY = Mathf.Max(0, startY);
            var maxX = Mathf.Min(Width, startX + targetMap.Width);
            var maxY = Mathf.Min(Height, startY + targetMap.Height);
            
            for (var y = minY; y < maxY; y++)
            {
                for (var x = minX; x < maxX; x++)
                {
                    var sourceCell = new Vector2Int(x, y);
                    var sourceValue = GetValue(sourceCell);
                    targetMap.AddValue(new Vector2Int(x - startX, y - startY), sourceValue * magnitude);
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

