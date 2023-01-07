using UnityEngine;

namespace Natick.InfluenceMaps
{
    public class InfluenceMapRenderer : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _shaderSpriteRenderer;
        
        [SerializeField]
        private SpriteRenderer _mapSpriteRenderer;

        private static readonly int _tiles = Shader.PropertyToID("_Tiles");

        public void SetSprite(Sprite influenceSprite, int width, int height)
        {
            _mapSpriteRenderer.sprite = influenceSprite;

            var shaderTex = new Texture2D(width, height);
            shaderTex.Apply();
            var shaderSprite = Sprite.Create(shaderTex, new Rect(0, 0, width, height), Vector2.zero);
            _shaderSpriteRenderer.sprite = shaderSprite;
            
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            _shaderSpriteRenderer.GetPropertyBlock(mpb);
            mpb.SetVector(_tiles, new Vector2(width, height));
            _shaderSpriteRenderer.SetPropertyBlock(mpb);
            transform.localScale = new Vector3(width, height, 1);
        }
    }
}