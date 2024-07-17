using UnityEngine;

namespace Runtime.Gameplay
{
    public class Cube : MonoBehaviour
    {
        [SerializeField] public SpriteRenderer spriteRenderer;
        public CubeInfo CubeInfo { get; private set; }
        public void Init(Vector2 scale, Vector2 position, Transform parent, CubeInfo cubeInfo)
        {
            transform.SetParent(parent);
            transform.localScale = Vector2.one;
            transform.position = position;
            CubeInfo = cubeInfo;
            spriteRenderer.sprite = cubeInfo.defaultSprite;
        }
    }
    public enum CubeType
    {
        Empty = 0,
        Blue = 1,
        Green = 2,
        Pink = 3,
        Purple = 4,
        Red = 5,
        Yellow = 6,
    }
}