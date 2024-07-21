using System;
using UnityEngine;

namespace Runtime.Gameplay
{
    public class GameUnit : MonoBehaviour
    {
        public GameUnitType type;
        
        public virtual void Pop(Action onBlast)
        {
            
        }
    }

    public enum GameUnitType
    {
        Default = 0,
        Cube = 1,
        Box = 2
    }
}
