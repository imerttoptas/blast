using System;
using System.Collections.Generic;
using Runtime.Gameplay.Pooling;
using UnityEngine;

namespace Runtime.Gameplay.Units
{
    public class Box : GameUnit , IAffectedByNeighbour , IFixedUnit
    {
        public int Health { get; private set; } = 2;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<Sprite> sprites;
        
        public void Init()
        {
            Health = 2;
            SetState();
        }

        public override void Pop(Action onBlast)
        {
            Health--;
            SetState();
            if (Health <= 0)
            {
                onBlast?.Invoke();
                ObjectPoolManager.instance.ReturnToPool(gameObject);
            }
        }
        
        private void SetState()
        {
            spriteRenderer.sprite = Health switch
            {
                1 => sprites[0],
                2 => sprites[1],
                _ => spriteRenderer.sprite
            };
        }
    }
}
