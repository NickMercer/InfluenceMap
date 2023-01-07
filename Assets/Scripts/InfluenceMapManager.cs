using System;
using System.Collections.Generic;
using System.Linq;
using Test;
using UnityEngine;

namespace Natick.InfluenceMaps
{
    public abstract class InfluenceMapManager : MonoBehaviour
    {
        internal float CellSize { get; private set; }

        internal int FactionCount { get; private set; }

        internal int MapWidthInCells { get; private set; }
        internal int MapHeightInCells { get; private set; }
        internal int MapCountWidth { get; private set; }
        internal int MapCountHeight { get; private set; }
        
        
        internal int WorldWidthInCells { get; private set; }
        internal int WorldHeightInCells { get; private set; }
        internal float WorldWidthInMeters { get; private set; }
        internal float WorldHeightInMeters { get; private set; }

        private Vector2 _anchorPoint;

        protected readonly List<EntityNode> EntityNodes = new List<EntityNode>();

        protected void Initialize(Vector2 worldSize, float cellSize, Vector2Int mapCount, Vector2 bottomLeft)
        {
            WorldWidthInMeters = worldSize.x;
            WorldHeightInMeters = worldSize.y;
            WorldWidthInCells = Mathf.FloorToInt(worldSize.x / cellSize);
            WorldHeightInCells = Mathf.FloorToInt(worldSize.y / cellSize);
            MapCountWidth = mapCount.x;
            MapCountHeight = mapCount.y;
            MapWidthInCells = WorldWidthInCells / mapCount.x;
            MapHeightInCells = WorldHeightInCells / mapCount.y;
            CellSize = cellSize;
            _anchorPoint = bottomLeft;
        }

        #region Entity Registration

        public void RegisterEntity(IInfluenceEntity entity)
        {
            if(EntityNodes.Any(x => x.Entity == entity) == false)
                EntityNodes.Add(new EntityNode(entity, this));
        }

        public void UnregisterEntity(IInfluenceEntity entity)
        {
            EntityNodes.RemoveAll(x => x.Entity == entity);
        }
        
        #endregion
        
        #region Influence Stamps

        protected List<InfluenceStamp> CreateInfluenceStamps(MapType stampType, int maxRadius, int increment,
            InfluenceCurve curve)
        {
            var stamps = new List<InfluenceStamp>();
            for (var r = 1; r <= maxRadius; r+= increment)
            {
                var size = (2 * r) + 1;
                var newMap = new InfluenceMap(size, size);
                var newStamp = new InfluenceStamp();

                newMap.PropagateInfluenceFromCenter(curve, 1f);
                newStamp.Radius = r;
                newStamp.StampType = stampType;
                newStamp.Map = newMap;
                
                stamps.Add(newStamp);
            }

            return stamps;
        }

        internal abstract InfluenceMap GetInfluenceStamp(MapType mapType, int radius);
        
        public void AddInfluenceSource(int layerId, Vector3 sourcePosition, MapType mapType, float influence, int radius)
        {
            var mapIndex = GetMapIndex(sourcePosition);
            var stampMap = GetInfluenceStamp(mapType, radius);
            var sourceCell = GetCellPosition(sourcePosition);
            var localOffset = new Vector2Int(mapIndex.x * MapWidthInCells, mapIndex.y * MapHeightInCells);
            var touchedMaps = GetTouchedMaps(layerId, mapType, mapIndex, radius, sourceCell - localOffset);
            foreach (var touchedMap in touchedMaps)
            {
                var localIndex = touchedMap.Item1;
                localOffset =  new Vector2Int(localIndex.x * MapWidthInCells, localIndex.y * MapHeightInCells);
                touchedMap.Item2.AddMap(stampMap, sourceCell - localOffset, influence, new Vector2Int(-radius, -radius));
            }
        }
        
        #endregion
        
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

        public Vector2Int GetCellPosition(Vector3 worldLocation)
        {
            if (WorldPositionInBounds(worldLocation) == false)
                return new Vector2Int(-1, -1);

            var cellX = Mathf.FloorToInt(worldLocation.x / CellSize);
            var cellY = Mathf.FloorToInt(worldLocation.z / CellSize);
            
            return new Vector2Int(cellX, cellY);
        }
        
        internal abstract LayerMapCollection GetLayerMapCollection(MapType mapType);

        #endregion
        
        #region Influence Changes

        public void UpdateEntities()
        {
            for (var i = EntityNodes.Count - 1; i >= 0 ; i--)
            {
                var entityNode = EntityNodes[i];

                if (entityNode.EntityExists() == false)
                {
                    if (entityNode.IsRegistered)
                    {
                        ProcessInfluence(entityNode.EntityInformation, -1f);
                    }
                    EntityNodes.RemoveAt(i);
                    continue;
                }

                if (entityNode.IsRegisterable())
                {
                    var previousInformation = entityNode.UpdateEntityInformation();
                    if (!entityNode.InfoChanged && entityNode.IsRegistered) 
                        continue;
                    
                    if (entityNode.IsRegistered)
                    {
                        ProcessInfluence(previousInformation, -1f);
                    }

                    entityNode.UpdateEntityInformation();
                    ProcessInfluence(entityNode.EntityInformation, 1f);
                    entityNode.IsRegistered = true;
                    continue;
                }
                
                if (entityNode.IsRegistered)
                {
                    ProcessInfluence(entityNode.EntityInformation, -1f);
                }

                entityNode.InvalidateLocation();
                entityNode.IsRegistered = false;
            }
        }

        protected abstract void ProcessInfluence(EntityInformation entityInfo, float magnitude);
        
        //Returns list of references to the maps that will be touched by the template based on the location and the radius
        private IEnumerable<(Vector2Int, InfluenceMap)> GetTouchedMaps(int layerId, MapType mapType, Vector2Int gridLocation,
            int radius, Vector2Int centerCell)
        {
            var currentMap = CheckAndAddMapLayer(layerId, mapType, gridLocation);
            if (currentMap != null)
                yield return (gridLocation, currentMap);
            
            //Northwest Corner
            if ((centerCell.x - radius < 0) && (centerCell.y + radius > MapHeightInCells))
            {
                var localLocation = gridLocation + new Vector2Int(-1, 1);
                currentMap = CheckAndAddMapLayer(layerId, mapType, localLocation);
                if (currentMap != null)
                    yield return (localLocation, currentMap);
            }
            
            //North Corner
            if (centerCell.y + radius > MapHeightInCells)
            {
                var localLocation = gridLocation + new Vector2Int(0, 1);
                currentMap = CheckAndAddMapLayer(layerId, mapType, localLocation);
                if (currentMap != null)
                    yield return (localLocation, currentMap);
            }
            
            //Northeast Corner
            if ((centerCell.x + radius > MapWidthInCells) && (centerCell.y + radius > MapHeightInCells))
            {
                var localLocation = gridLocation + new Vector2Int(1, 1);
                currentMap = CheckAndAddMapLayer(layerId, mapType, localLocation);
                if (currentMap != null)
                    yield return (localLocation, currentMap);
            }
            
            //East Corner
            if (centerCell.x + radius > MapWidthInCells)
            {
                var localLocation = gridLocation + new Vector2Int(1, 0);
                currentMap = CheckAndAddMapLayer(layerId, mapType, localLocation);
                if (currentMap != null)
                    yield return (localLocation, currentMap);
            }
            
            //Southeast Corner
            if ((centerCell.x + radius > MapWidthInCells) && (centerCell.y - radius < 0))
            {
                var localLocation = gridLocation + new Vector2Int(1, -1);
                currentMap = CheckAndAddMapLayer(layerId, mapType, localLocation);
                if (currentMap != null)
                    yield return (localLocation, currentMap);
            }
            
            //South Corner
            if (centerCell.y - radius < 0)
            {
                var localLocation = gridLocation + new Vector2Int(0, -1);
                currentMap = CheckAndAddMapLayer(layerId, mapType, localLocation);
                if (currentMap != null)
                    yield return (localLocation, currentMap);
            }
            
            //Southwest Corner
            if ((centerCell.x - radius < 0) && (centerCell.y - radius < 0))
            {
                var localLocation = gridLocation + new Vector2Int(-1, -1);
                currentMap = CheckAndAddMapLayer(layerId, mapType, localLocation);
                if (currentMap != null)
                    yield return (localLocation, currentMap);
            }
            
            //West Corner
            if (centerCell.x - radius < 0)
            {
                var localLocation = gridLocation + new Vector2Int(-1, 0);
                currentMap = CheckAndAddMapLayer(layerId, mapType, localLocation);
                if (currentMap != null)
                    yield return (localLocation, currentMap);
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
            public IInfluenceEntity Entity => _entity.TryGetTarget(out var entity) ? entity : null;

            private EntityInformation _entityInformation;
            public EntityInformation EntityInformation => _entityInformation;

            private readonly InfluenceMapManager _mapManager;

            public EntityNode(IInfluenceEntity entity, InfluenceMapManager manager)
            {
                _entity = new WeakReference<IInfluenceEntity>(entity);
                _mapManager = manager;
            }
            
            public bool IsRegistered { get; set; }
            public bool InfoChanged { get; private set; }

            public EntityInformation UpdateEntityInformation()
            {
                if (_entity.TryGetTarget(out var entity) == false)
                {
                    throw new NullReferenceException("Somehow entity was registerable while having no current information available");
                } 
                
                var currentInfo = entity.GetEntityInformation();
                currentInfo.LastMapGrid = _mapManager.GetMapIndex(currentInfo.LastWorldLocation);
                if (currentInfo == null)
                {
                    throw new NullReferenceException("Somehow entity was registerable while having no current information available");
                }

                if (_entityInformation == null)
                    InfoChanged = true;
                else if (_entityInformation.LastMapGrid != currentInfo.LastMapGrid)
                    InfoChanged = true;
                else if (_entityInformation.LastWorldLocation != currentInfo.LastWorldLocation)
                    InfoChanged = true;
                else
                    InfoChanged = _entityInformation.UpdateRequiresRepaint(currentInfo);

                var previousInformation = _entityInformation;
                _entityInformation = currentInfo;
                return previousInformation;
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