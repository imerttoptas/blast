using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay
{
    [CreateAssetMenu(fileName = "New Cube Info Catalog", menuName = "Cube Info Catalog")]
    public class CubeInfoCatalogue : ScriptableObject
    {
        public List<CubeInfo> cubeInfoList;
        
        public CubeInfo GetRandomCubeTypeFromRange()
        {
            return cubeInfoList[Random.Range(0, cubeInfoList.Count - 1)];
        }        
        
        public CubeInfo GetCubeInfoOfType(CubeType cubeType)
        {
            return cubeInfoList.Find(x => x.type == cubeType);
        }
    }
}
