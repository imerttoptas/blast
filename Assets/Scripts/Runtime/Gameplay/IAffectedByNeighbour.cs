using System;
using UnityEngine;

namespace Runtime.Gameplay
{
    public interface IAffectedByNeighbour
    {
        public int Health { get; }
    }
}
