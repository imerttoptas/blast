using UnityEngine;

namespace Runtime.Gameplay
{
    public class Box : GameUnit , IAffectedByNeighbour , IFixed
    {
        public int Health { get; private set; } = 2;
        
        public void OnNeighbourBlast()
        {
            Health--;
            if (Health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
