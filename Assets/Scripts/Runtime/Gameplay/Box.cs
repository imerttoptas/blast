using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay
{
    public class Box : GameUnit , IAffectedByNeighbour , IFixedUnit
    {
        public int Health { get; private set; } = 2;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<Sprite> sprites;
        
        public override void Pop(Action onBlast)
        {
            Health--;
            spriteRenderer.sprite = sprites[0];
            if (Health <= 0)
            {
                onBlast?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
