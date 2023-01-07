using System;
using System.Collections.Generic;
using System.Linq;
using Natick;
using Natick.InfluenceMaps;
using UnityEditor;
using UnityEngine;

namespace Test
{
    public class ExampleInfluenceMapManager : InfluenceMapManager
    {
        private List<InfluenceStamp> _proximityStamps;
        private List<InfluenceStamp> _threatStamps;
        private List<InfluenceStamp> _interestStamps;

        private LayerMapCollection _proximityMaps;
        private LayerMapCollection _threatMaps;
        private LayerMapCollection _interestMaps;
        
        [SerializeField]
        private Gradient _influenceMapGradient;

        [SerializeField]
        private InfluenceMapRenderer _influenceMapRenderer;

        private void Start()
        {
            Initialize(new Vector2(100, 100), 1f, new Vector2Int(5, 5), new Vector2(0, 0));
            _proximityStamps = CreateInfluenceStamps(MapType.Proximity, 12, 1, InfluenceCurve.Linear);
            _threatStamps = CreateInfluenceStamps(MapType.Threat, 20, 1, InfluenceCurve.InverseFourPoly);
            _interestStamps = CreateInfluenceStamps(MapType.Interest, 5, 1, InfluenceCurve.Quad);
            _proximityMaps = new LayerMapCollection(this);
            _threatMaps = new LayerMapCollection(this);
            _interestMaps = new LayerMapCollection(this);
            UpdateEntities();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                UpdateEntities();
                DrawMap();
            }
        }
        
        internal override InfluenceMap GetInfluenceStamp(MapType mapType, int radius)
        {
            switch (mapType)
            {
                case MapType.Proximity:
                    var stamp = _proximityStamps.FirstOrDefault(x => x.Radius == radius);
                    var map = stamp.Map;
                    if(map == null)
                    {
                        stamp = _proximityStamps.OrderByDescending(x => x.Radius).First();
                    }
                    return stamp.Map;
                
                case MapType.Threat:
                    stamp = _threatStamps.FirstOrDefault(x => x.Radius == radius);
                    map = stamp.Map;
                    if(map == null)
                    {
                        stamp = _threatStamps.OrderByDescending(x => x.Radius).First();
                    }
                    return stamp.Map;
                
                case MapType.Interest:
                    stamp = _interestStamps.FirstOrDefault(x => x.Radius == radius);
                    map = stamp.Map;
                    if(map == null)
                    {
                        stamp = _interestStamps.OrderByDescending(x => x.Radius).First();
                    }
                    return stamp.Map;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }
        }
        
        internal override LayerMapCollection GetLayerMapCollection(MapType mapType)
        {
            switch (mapType)
            {
                case MapType.Proximity:
                    return _proximityMaps;
                
                case MapType.Threat:
                    return _threatMaps;
                
                case MapType.Interest:
                    return _interestMaps;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }
        }

        protected override void ProcessInfluence(EntityInformation entityInfo, float magnitude)
        {
            if (entityInfo is ExampleAgentInformation == false)
                return;

            var influenceInfo = (ExampleAgentInformation)entityInfo;
            
            AddInfluenceSource(1, influenceInfo.LastWorldLocation, MapType.Proximity, influenceInfo.Proximity.Value * magnitude,
                influenceInfo.Proximity.Key);
            AddInfluenceSource(1, influenceInfo.LastWorldLocation, MapType.Threat, influenceInfo.Threat.Value * magnitude,
                influenceInfo.Threat.Key);
            AddInfluenceSource(1, influenceInfo.LastWorldLocation, MapType.Interest, influenceInfo.Interest.Value * magnitude,
                influenceInfo.Interest.Key);
        }
        
        
        private void DrawMap()
        {
            var mapToDraw = GetLayerMapCollection(MapType.Proximity);
            var proximityMap = mapToDraw.GetFullMapLayer(1);
            var texture = new Texture2D(WorldWidthInCells, WorldHeightInCells, TextureFormat.RGBA32, false);
            var colorArray = new Color32[WorldWidthInCells * WorldHeightInCells];
            for (int y = 0; y < WorldHeightInCells; y++)
            {
                for (int x = 0; x < WorldWidthInCells; x++)
                {
                    var cell = proximityMap.GetValue(new Vector2Int(x, y));
                    var color = _influenceMapGradient.Evaluate(cell.Normalize(-30, 30));
                    colorArray[(y * WorldWidthInCells) + x] = color;
                }
            }

            texture.SetPixels32(colorArray);
            texture.Apply();
            var influenceSprite = Sprite.Create(texture, new Rect(0, 0, WorldWidthInCells, WorldHeightInCells), Vector2.zero);
            _influenceMapRenderer.SetSprite(influenceSprite, WorldWidthInCells, WorldHeightInCells);

            // var bytes = texture.EncodeToPNG();
            //
            // System.IO.File.WriteAllBytes("Assets/TestGridTexture.png", bytes);
            // AssetDatabase.ImportAsset("Assets/TestGridTexture.png");
        }
    }
}