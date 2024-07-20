using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Runtime.Gameplay
{
    public class GridController : Singleton<GridController>
    {
        [SerializeField] private UnitSpawner unitSpawner;
        [SerializeField] private GridBuilder gridBuilder;
        [SerializeField] private InputManager inputManager;

        private Cell[,] grid = new Cell[12, 12];
        private List<Cell> tempCellList = new();
        [SerializeField] private int rowCount;
        [SerializeField] private int columnCount;
        public int RowCount => rowCount;
        public int ColCount => columnCount;
        private void OnEnable()
        {
            inputManager.OnClickedToClickableObject += TryMakeMove;
        }
        
        private void OnDisable()
        {
            inputManager.OnClickedToClickableObject -= TryMakeMove;

        }
        
        void Start()
        {
            gridBuilder.GenerateGrid(grid);
        }
        
        private void TryMakeMove(Cell cell)
        {
            if (CheckValidMove(cell))
            {
                BlastAdjacentCubes(cell);
            }
        }
        
        public Cell GetCell(int row, int col)
        {
            if (row >= 0 && row < rowCount && col >= 0 && col < columnCount)
            {
                return grid[row, col];
            }
            return null;
        }
    
        public List<Cell> GetColumn(int colIndex)
        {
            tempCellList.Clear();
    
            for (int i = 0; i < rowCount; i++)
            {
                tempCellList.Add(GetCell(i, colIndex));
            }
    
            return tempCellList;
        }
        
        public List<Cell> GetRow(int rowIndex)
        {
            tempCellList.Clear();
    
            for (int i = 0; i < columnCount; i++)
            {
                tempCellList.Add(GetCell(rowIndex, i));
            }
    
            return tempCellList;
        }

        private bool CheckValidMove(Cell cell)
        {
            return GetNeighboursWithSameType(cell).Count > 0;
        }
        
        private List<Cell> GetNeighboursWithSameType(Cell cell)
        {
            tempCellList.Clear();
            CubeType target = cell.Cube.CubeInfo.type;
            if (GetCell(cell.RowIndex + 1, cell.ColIndex)?.Cube?.CubeInfo.type == target)
                tempCellList.Add(GetCell(cell.RowIndex + 1, cell.ColIndex));
            if (GetCell(cell.RowIndex - 1, cell.ColIndex)?.Cube?.CubeInfo.type == target)
                tempCellList.Add(GetCell(cell.RowIndex - 1, cell.ColIndex));
            if (GetCell(cell.RowIndex, cell.ColIndex + 1)?.Cube?.CubeInfo.type == target)
                tempCellList.Add(GetCell(cell.RowIndex, cell.ColIndex + 1));
            if (GetCell(cell.RowIndex, cell.ColIndex - 1)?.Cube?.CubeInfo.type == target)
                tempCellList.Add(GetCell(cell.RowIndex, cell.ColIndex + 1));
            
            return tempCellList;
        }
        
        private List<Cell> FindMatchingCells(Cell cell)
        {
            CubeType type = cell.Cube.CubeInfo.type;
            tempCellList.Clear();
            Queue<(int row, int col)> cellsToVisit = new();
            HashSet<(int row, int col)> visitedCells = new();
            
            cellsToVisit.Enqueue((cell.RowIndex, cell.ColIndex));
            visitedCells.Add((cell.RowIndex, cell.ColIndex));
            
            while (cellsToVisit.Count > 0)
            {
                var (currentRow, currentCol) = cellsToVisit.Dequeue();
                Cube currentCube = GetCell(currentRow, currentCol)?.Cube;
                
                if (currentCube == null || currentCube.CubeInfo.type != type)
                {
                    continue;
                }

                tempCellList.Add(GetCell(currentRow, currentCol));
                
                if (currentRow > 0 && visitedCells.Add((currentRow - 1, currentCol)))
                {
                    cellsToVisit.Enqueue((currentRow - 1, currentCol));
                }
                if (currentRow < rowCount - 1 && visitedCells.Add((currentRow + 1, currentCol)))
                {
                    cellsToVisit.Enqueue((currentRow + 1, currentCol));
                }
                if (currentCol > 0 && visitedCells.Add((currentRow, currentCol - 1)))
                {
                    cellsToVisit.Enqueue((currentRow, currentCol - 1));
                }
                if (currentCol < columnCount - 1 && visitedCells.Add((currentRow, currentCol + 1)))
                {
                    cellsToVisit.Enqueue((currentRow, currentCol + 1));
                }
            }
            
            return tempCellList;
        }
        
        private void BlastAdjacentCubes(Cell clickedCell)
        {
            List<Cell> matchGroup = FindMatchingCells(clickedCell);
            foreach (var cell in matchGroup)
            {
                cell.Cube.OnClick();
                cell.Cube = null;
            }
            
            DropColumns(matchGroup);
        }
        
        private void DropColumns(List<Cell> matchGroup)
        {
            HashSet<int> colIndexes = new HashSet<int>();

            foreach (var cell in matchGroup)
            {
                if (colIndexes.Add(cell.ColIndex))
                {
                    DropColumn(cell.ColIndex);
                }
            }
        }
        
        private async void DropColumn(int colIndex)
        {
            int dropCount = GetDropCountOfColumn(colIndex);
            int startToDropFromRow = GetDropStartingRow(colIndex);
            
            List<UniTask> tasks = new List<UniTask>();
            int spawnedCubeIndex = 0;
            for (int i = startToDropFromRow; i < rowCount; i++)
            {
                if (i + dropCount < rowCount)
                {
                    grid[i, colIndex].Fill(grid[i + dropCount, colIndex].Cube);
                }
                else
                {
                    spawnedCubeIndex++;
                    grid[i, colIndex].Cube = unitSpawner.SpawnNewCubeAtColumn(i, colIndex, spawnedCubeIndex);
                }

                tasks.Add(grid[i, colIndex].Cube.transform.DOLocalMove(Vector2.zero, 5f).SetSpeedBased().ToUniTask());
            }
           
            await UniTask.WhenAll(tasks);
        }
        
        private int GetDropCountOfColumn(int colIndex)
        {
            int dropCount = 0;
            for (int i = 0 ; i < rowCount; i++) // TODO: Start from the highest blasted cell on the column 
            {
                if (GetCell(i, colIndex).Cube == null)
                {
                    dropCount++;
                }
            }

            return dropCount;
        }
        
        private int GetDropStartingRow(int colIndex)
        {
            int startToDropFromRow = 0;
            
            for (int i = 0 ; i < rowCount; i++) // TODO: Start from the highest blasted cell on the column 
            {
                if (GetCell(i, colIndex).Cube == null)
                {
                    startToDropFromRow = i;
                    break;
                }
            }

            return startToDropFromRow;
        }
        
    }
}
