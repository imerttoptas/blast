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
            ObjectPoolManager.instance.ReturnToPool(gameObject);
        }

        public void SetState(int count)
        {
            spriteRenderer.sprite = count switch
            {
                < 5 => CubeInfo.defaultSprite,
                >= 5 and < 8 => CubeInfo.cubeSpriteList[0],
                >= 8 and < 10 => CubeInfo.cubeSpriteList[1],
                _ => CubeInfo.cubeSpriteList[2]
            };
        }

    }
    
    public enum CubeType
    {
        Blue = 0,
        Green = 1,
        Pink = 2,
        Purple = 3,
        Red = 4,
        Yellow = 5
    }
}