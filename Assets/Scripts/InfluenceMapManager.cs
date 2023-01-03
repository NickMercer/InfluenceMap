using System;
using System.Collections.Generic;
using System.Threading;
using Test;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Natick.InfluenceMaps
{
    public abstract class InfluenceMapManager
    {
        internal float CellSize { get; private set; }

        internal int FactionCount { get; private set; }

        internal int MapWidthInCells { get; private set; }
        internal int MapHeightInCells { get; private set; }
        internal int MapCountWidth { get; private set; }
        internal int MapCountHeight { get; private set; }
        
        
        internal float WorldWidthInMeters { get; private set; }
        internal float WorldHeightInMeters { get; private set; }

        private Vector2 _anchorPoint;

        protected readonly List<EntityNode> EntityNodes;
        
        #region Map/Layer Selection
        public Vector2Int GetMapIndex(Vector3 worldLocation)
        {
            if (WorldPositionInBounds(worldLocation) == false)
                return new Vector2Int(-1, -1);
            
            var cellX = worldLocation.x / CellSize;
            var cellY = worldLocation.z / CellSize;

            var mapX = Mathf.FloorToInt(cellX / MapWidthInCells);
            var mapY = Mathf.FloorToInt(cellY / MapHeightInCells);

            return new Vector2Int(mapX, mapY);
        }
        
        internal abstract LayerMapCollection GetLayerMapCollection(MapType mapType);

        #endregion
        
        #region Influence Changes

        public void UpdateEntities()
        {
            for (var i = EntityNodes.Count - 1; i > 0 ; i--)
            {
                var entityNode = EntityNodes[i];

                if (entityNode.EntityExists() == false)
                {
                    if (entityNode.IsRegistered)
                    {
                        ProcessInfluence(entityNode, -1f);
                    }
                    EntityNodes.RemoveAt(i);
                    return;
                }

                if (entityNode.IsRegisterable())
                {
                    if (!entityNode.InfoChanged && entityNode.IsRegistered) 
                        return;
                    
                    if (entityNode.IsRegistered)
                    {
                        ProcessInfluence(entityNode, -1f);
                    }

                    entityNode.UpdateEntityInformation();
                    ProcessInfluence(entityNode, 1f);
                    return;
                }
                
                if (entityNode.IsRegistered)
                {
                    ProcessInfluence(entityNode, -1f);
                }

                entityNode.InvalidateLocation();
                entityNode.IsRegistered = false;
            }
        }

        protected abstract void ProcessInfluence(EntityNode entity, float magnitude);
        
        //Returns list of references to the maps that will be touched by the template based on the location and the radius
        private IEnumerable<InfluenceMap> GetTouchedMaps(int layerId, MapType mapType, Vector2Int gridLocation,
            int radius, Vector2Int centerCell)
        {
            var currentMap = CheckAndAddMapLayer(layerId, mapType, gridLocation);
            if (currentMap != null)
                yield return currentMap;
            
            //Northwest Corner
            if ((centerCell.x - radius < 0) && (centerCell.y + radius > MapHeightInCells))
            {
                currentMap = CheckAndAddMapLayer(layerId, mapType, gridLocation + new Vector2Int(-1, 1));
                if (currentMap != null)
                    yield return currentMap;
            }
            
            //North Corner
            if (centerCell.y + radius > MapHeightInCells)
            {
                currentMap = CheckAndAddMapLayer(layerId, mapType, gridLocation + new Vector2Int(0, 1));
                if (currentMap != null)
                    yield return currentMap;
            }
            
            //Northeast Corner
            if ((centerCell.x + radius > MapWidthInCells) && (centerCell.y + radius > MapHeightInCells))
            {
                currentMap = CheckAndAddMapLayer(layerId, mapType, gridLocation + new Vector2Int(1, 1));
                if (currentMap != null)
                    yield return currentMap;
            }
            
            //East Corner
            if (centerCell.x + radius > MapWidthInCells)
            {
                currentMap = CheckAndAddMapLayer(layerId, mapType, gridLocation + new Vector2Int(1, 0));
                if (currentMap != null)
                    yield return currentMap;
            }
            
            //Southeast Corner
            if ((centerCell.x + radius > MapWidthInCells) && (centerCell.y - radius < 0))
            {
                currentMap = CheckAndAddMapLayer(layerId, mapType, gridLocation + new Vector2Int(1, -1));
                if (currentMap != null)
                    yield return currentMap;
            }
            
            //South Corner
            if (centerCell.y - radius < 0)
            {
                currentMap = CheckAndAddMapLayer(layerId, mapType, gridLocation + new Vector2Int(0, -1));
                if (currentMap != null)
                    yield return currentMap;
            }
            
            //Southwest Corner
            if ((centerCell.x - radius < 0) && (centerCell.y - radius < 0))
            {
                currentMap = CheckAndAddMapLayer(layerId, mapType, gridLocation + new Vector2Int(-1, -1));
                if (currentMap != null)
                    yield return currentMap;
            }
            
            //West Corner
            if (centerCell.x - radius < 0)
            {
                currentMap = CheckAndAddMapLayer(layerId, mapType, gridLocation + new Vector2Int(-1, 0));
                if (currentMap != null)
                    yield return currentMap;
            }
        }

        private InfluenceMap CheckAndAddMapLayer(int layerId, MapType mapType, Vector2Int gridPosition)
        {
            if (MapInBounds(gridPosition))
            {
                var layerCollection = GetLayerMapCollection(mapType);
                var map = layerCollection.GetMapLayer(layerId, gridPosition);
                if (map == null)
                {
                    var newMap = new InfluenceMap(MapWidthInCells, MapHeightInCells, cellSize: CellSize);
                    layerCollection.AddMapLayer(layerId, gridPosition, newMap);
                    map = newMap;
                }

                return map;
            }

            return null;
        }

        #endregion
        
        #region Location Checks
        
        private bool MapInBounds(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.y >= 0 && gridPosition.x < MapCountWidth &&
                   gridPosition.y < MapCountHeight;
        }

        private bool WorldPositionInBounds(Vector3 worldPosition)
        {
            return worldPosition.x >= _anchorPoint.x
                   && worldPosition.x < _anchorPoint.x + WorldWidthInMeters
                   && worldPosition.z >= _anchorPoint.y
                   && worldPosition.z < _anchorPoint.y + WorldHeightInMeters;
        }
        
        #endregion
        
        protected class EntityNode
        {
            private readonly WeakReference<IInfluenceEntity> _entity;

            private EntityInformation _entityInformation;

            public bool IsRegistered { get; set; }
            public bool InfoChanged { get; private set; }

            public void UpdateEntityInformation()
            {
                EntityInformation currentInfo = null;
                if (_entity.TryGetTarget(out var entity))
                    currentInfo = entity.GetEntityInformation();

                if (currentInfo == null)
                {
                    throw new NullReferenceException("Somehow entity was registerable while having no current information available");
                }
                
                InfoChanged = _entityInformation.UpdateRequiresRepaint(currentInfo);
                _entityInformation = currentInfo;
            }

            public void InvalidateLocation() => _entityInformation.InvalidateLocation();

            public bool EntityExists()
            {
                return _entity != null;
            }

            public bool IsRegisterable()
            {
                if (_entity.TryGetTarget(out var entity))
                    return entity.IsRegisterable();

                return false;
            }
        }
    }

}