using UnityEngine;

namespace Runtime.Gameplay
{
    public class GridBuilder : MonoBehaviour
    {
        [SerializeField] private UnitSpawner unitSpawner;
        [SerializeField] private Cell cellPrefab;
        [SerializeField] private int colorCount;
        
        public void GenerateGrid(Cell[,] grid)
        {
            Vector2 startPoint = Vector2.one / 2f - new Vector2(GridController.instance.ColCount / 2f, GridController.instance.RowCount / 2f);

            for (int rowIndex = 0; rowIndex < GridController.instance.RowCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < GridController.instance.ColCount; colIndex++)
                {
                    Vector2 targetPos = startPoint + new Vector2(colIndex, rowIndex);
                    Cell createdCell = ObjectPoolManager.instance.Get(PoolObjectType.Cell).GetComponent<Cell>();
                    createdCell.Init(parent: transform, targetPos, Vector2.one, rowIndex, colIndex);
                    grid[rowIndex, colIndex] = createdCell;
                    
                    unitSpawner.SpawnRandomUnit(rowIndex, colIndex);
                }
            }
        }
    }
}
