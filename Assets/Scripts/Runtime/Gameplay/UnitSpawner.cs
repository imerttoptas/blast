using UnityEngine;

namespace Runtime.Gameplay
{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField] private CubeTypeCatalogue cubeTypeCatalogue;
        [SerializeField] private Cube cubePrefab;
        
        public void SpawnCubeAt(int row, int col)
        {
            Cube createdCube = Instantiate(cubePrefab);
            createdCube.Init(cubeTypeCatalogue.GetRandomCubeTypeFromRange());
            createdCube.transform.localScale = Vector2.one;
            GridController.instance.GetCell(row, col).Fill(createdCube);
            createdCube.transform.position = GridController.instance.GetCell(row, col).transform.position;
        }
        
        public Cube SpawnNewCubeAtColumn(int rowIndex, int colIndex, int spawnedCubeIndex)
        {
            Cube createdCube = Instantiate(cubePrefab);
            createdCube.transform.position =
                GridController.instance.GetCell(GridController.instance.RowCount - 1, colIndex).transform.position +
                Vector3.up * (spawnedCubeIndex * 2f);
            createdCube.Init(cubeTypeCatalogue.GetRandomCubeTypeFromRange());
            GridController.instance.GetCell(rowIndex, colIndex).Fill(createdCube);
            return createdCube;
        }
    }
}
