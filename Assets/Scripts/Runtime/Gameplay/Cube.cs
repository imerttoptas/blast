using UnityEngine;

namespace Runtime.Gameplay
{
    public class Cube : MonoBehaviour, IClickable
    {
        [SerializeField] public SpriteRenderer spriteRenderer;
        public CubeInfo CubeInfo { get; private set; }
        public int RowIndex { get; private set; }
        public int ColIndex { get; private set; }
        public void Init(int rowIndex, int colIndex, Vector2 scale,Vector2 position, Transform parent, CubeInfo cubeInfo)
        {
            IsClickable = true;
            RowIndex = rowIndex;
            ColIndex = colIndex;
            transform.SetParent(parent);
            transform.localScale = Vector2.one;
            transform.position = position;
            CubeInfo = cubeInfo;
            spriteRenderer.sprite = cubeInfo.defaultSprite;
        }

        public GameObject GameObject => gameObject;
        public bool IsClickable { get; set; }
        public void OnClick()
        {
            Destroy(gameObject);
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