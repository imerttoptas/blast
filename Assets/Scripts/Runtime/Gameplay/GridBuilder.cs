using UnityEngine;

namespace Runtime.Gameplay
{
    public class GridBuilder : MonoBehaviour
    {
        [SerializeField] private UnitSpawner unitSpawner;
        [SerializeField] private Cell cellPrefab;
        
        public void GenerateGrid(Cell[,] grid, CustomBoardInfo boardInfo)
        {
            Vector2 startPoint = Vector2.one / 2f - new Vector2(boardInfo.colCount / 2f, boardInfo.rowCount / 2f);

            for (int rowIndex = 0; rowIndex < boardInfo.rowCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < boardInfo.colCount; colIndex++)
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
