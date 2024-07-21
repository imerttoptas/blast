using UnityEngine;

namespace Runtime.Gameplay
{
    public class GameUnit : MonoBehaviour
    {
        public GameUnitType type;
    }

    public enum GameUnitType
    {
        Default = 0,
        Cube = 1,
        Box = 2
    }
}
