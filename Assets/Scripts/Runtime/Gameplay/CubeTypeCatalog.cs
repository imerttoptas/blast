using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay
{
    [CreateAssetMenu(fileName = "New Cube Type Catalog", menuName = "Cube Type Catalog")]
    public class CubeTypeCatalogue : ScriptableObject
    {
        public List<CubeInfo> cubeInfoList;

        public CubeInfo GetRandomCubeTypeFromRange(int rangeHigh)
        {
            return cubeInfoList[Random.Range(0, rangeHigh)];
        }
    }
}
