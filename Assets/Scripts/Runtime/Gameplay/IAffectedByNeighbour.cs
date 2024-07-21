using UnityEngine;

namespace Runtime.Gameplay
{
    public interface IAffectedByNeighbour
    {
        public void OnNeighbourBlast();
        public int Health { get; }
    }
}
