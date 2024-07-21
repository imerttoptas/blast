using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Gameplay
{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField] private CubeTypeCatalogue cubeTypeCatalogue;
        [SerializeField] private Cube cubePrefab;
        [SerializeField] private List<GameUnitInfo> gameUnitInfoList;
        
        public void SpawnUnit(GameUnitType unitType)
        {
            Instantiate(gameUnitInfoList.Find(x => x.type == unitType).prefab);
        }

        public void SpawnRandomUnit(int row, int col)
        {
            GameUnit createdUnit = Instantiate(GetRandomGameUnit());
            createdUnit.transform.localScale = Vector2.one;
            GridController.instance.GetCell(row, col).Fill(createdUnit);
            createdUnit.transform.position = GridController.instance.GetCell(row, col).transform.position;
            if (GridController.instance.GetCell(row,col).GetUnit<Cube>())
            {
                GridController.instance.GetCell(row, col).GetUnit<Cube>()
                    .Init(cubeTypeCatalogue.GetRandomCubeTypeFromRange());
            }
        }
        
        public void SpawnCubeAt(int row, int col)
        {
            Cube createdCube = Instantiate(cubePrefab);
            createdCube.Init(cubeTypeCatalogue.GetRandomCubeTypeFromRange());
            createdCube.transform.localScale = Vector2.one;
            GridController.instance.GetCell(row, col).Fill(createdCube);
            createdCube.transform.position = GridController.instance.GetCell(row, col).transform.position;
        }
        
        public Cube SpawnNewCube(int colIndex, int index)
        {
            Cube createdCube = Instantiate(cubePrefab);
            createdCube.transform.position =
                GridController.instance.GetCell(GridController.instance.RowCount - 1, colIndex).transform.position +
                Vector3.up * ((index + 1) * 2f); // TODO: Change the upValue
            createdCube.Init(cubeTypeCatalogue.GetRandomCubeTypeFromRange());
            return createdCube;
        }

        private GameUnit GetRandomGameUnit()
        {
            return Random.Range(0, 1f) < 0.8f ? gameUnitInfoList[0].prefab : gameUnitInfoList[1].prefab;
        }
        
    }
    
    [Serializable]
    public class GameUnitInfo
    {
        public GameUnitType type;
        public GameUnit prefab;
        
    }
}
