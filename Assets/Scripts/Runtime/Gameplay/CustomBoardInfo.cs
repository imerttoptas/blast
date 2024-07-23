using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay
{
    [CreateAssetMenu(fileName = "New Board Info", menuName = "Board Info")]
    public class CustomBoardInfo : ScriptableObject
    {
        [Range(2, 10)]
        public int rowCount;

        [Range(2, 10)]
        public int colCount;

        [Tooltip("List of colors on the game.")]
        public List<CubeType> cubeTypes;
        
        public CubeType GetRandomCubeType()
        {
            return cubeTypes[Random.Range(0, cubeTypes.Count)];
        }
    }
}
