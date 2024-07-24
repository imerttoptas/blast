using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay.Units
{
    [CreateAssetMenu(fileName = "New Cube Type", menuName = "Cube Type")]
    public class CubeInfo : ScriptableObject
    {
        public CubeType type;
        public Sprite defaultSprite;
        public List<Sprite> cubeSpriteList;
    }
}
