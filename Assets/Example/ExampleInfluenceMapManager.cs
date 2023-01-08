using Natick;
using Natick.InfluenceMaps;
using UnityEngine;

namespace Test
{
    public class ExampleInfluenceMapManager : InfluenceMapManager
    {
        [SerializeField]
        private Gradient _influenceMapGradient;

        [SerializeField]
        private InfluenceMapRenderer _influenceMapRenderer;
        
        [SerializeField]
        private MapType _mapTypeToRender;

        [SerializeField]
        private int _layerToRender = 1;
        
        private void Start()
        {
            Initialize(new Vector2(100, 100), 1f, new Vector2Int(5, 5), new Vector2(0, 0));
            
            CreateInfluenceStamps(MapType.Proximity, 12, 1, InfluenceCurve.Linear);
            CreateInfluenceStamps(MapType.Threat, 20, 1, InfluenceCurve.InverseFourPoly);
            CreateInfluenceStamps(MapType.Interest, 5, 1, InfluenceCurve.Quad);
            
            AddLayerMapCollection(MapType.Proximity);
            AddLayerMapCollection(MapType.Threat);
            AddLayerMapCollection(MapType.Interest);

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
        
        protected override void ProcessInfluence(EntityInformation entityInfo, float magnitude)
        {
            if (entityInfo is ExampleAgentInformation == false)
                return;

            var influenceInfo = (ExampleAgentInformation)entityInfo;
            
            AddInfluenceSource(influenceInfo.Layer, influenceInfo.LastWorldLocation, MapType.Proximity, influenceInfo.Proximity.Value * magnitude,
                influenceInfo.Proximity.Key);
            AddInfluenceSource(influenceInfo.Layer, influenceInfo.LastWorldLocation, MapType.Threat, influenceInfo.Threat.Value * magnitude,
                influenceInfo.Threat.Key);
            AddInfluenceSource(influenceInfo.Layer, influenceInfo.LastWorldLocation, MapType.Interest, influenceInfo.Interest.Value * magnitude,
                influenceInfo.Interest.Key);
        }
        
        private void DrawMap()
        {
            var mapToDraw = GetLayerMapCollection(_mapTypeToRender);
            var proximityMap = mapToDraw.GetFullMapLayer(_layerToRender);
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

        }
    }
}