using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Runtime.Gameplay
{
    public class Cube : GameUnit , IShiftable
    {
        [SerializeField] public SpriteRenderer spriteRenderer;
        public CubeInfo CubeInfo { get; private set; }
        public GameObject GameObject => gameObject;
        public bool IsClickable { get; set; }
        
        public void Init(CubeInfo cubeInfo)
        {
            IsClickable = true;
            CubeInfo = cubeInfo;
            spriteRenderer.sprite = cubeInfo.defaultSprite;
        }
        
        public override void Pop(Action onBlast)
        {
            onBlast?.Invoke();
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