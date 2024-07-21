using System.Collections.Generic;
using System.Linq;
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
        private HashSet<Cell> tempCellHashSet = new();
        [SerializeField] private int rowCount;
        [SerializeField] private int columnCount;
        public int RowCount => rowCount;
        public int ColCount => columnCount;
        private List<UniTask> tasks = new();
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
    
        public HashSet<Cell> GetColumn(int colIndex)
        {
            tempCellHashSet.Clear();
    
            for (int i = 0; i < rowCount; i++)
            {
                tempCellHashSet.Add(GetCell(i, colIndex));
            }
    
            return tempCellHashSet;
        }
        
        public HashSet<Cell> GetRow(int rowIndex)
        {
            tempCellHashSet.Clear();
    
            for (int i = 0; i < columnCount; i++)
            {
                tempCellHashSet.Add(GetCell(rowIndex, i));
            }
    
            return tempCellHashSet;
        }

        private bool CheckValidMove(Cell cell)
        {
            return GetCubeNeighboursWithSameType(cell).Count > 0;
        }
        
        private HashSet<Cell> GetCubeNeighboursWithSameType(Cell cell)
        {
            HashSet<Cell> cubeNeighbours = GetNeighboursWithType(cell, GameUnitType.Cube);

            HashSet<Cell> cubeNeighboursWithSameType = new HashSet<Cell>();
            
            foreach (Cell neighbourCell in cubeNeighbours)
            {
                if (cell.GetUnit<Cube>() && 
                    cell.GetUnit<Cube>().CubeInfo.type == neighbourCell.GetUnit<Cube>().CubeInfo.type)
                {
                    cubeNeighboursWithSameType.Add(neighbourCell);
                }
            }
            
            return cubeNeighboursWithSameType;
        }        
        
        private HashSet<Cell> GetNeighboursWithType(Cell cell, GameUnitType type)
        {
            tempCellHashSet.Clear();
            
            if (GetCell(cell.RowIndex + 1, cell.ColIndex)?.Unit?.type == type)
                tempCellHashSet.Add(GetCell(cell.RowIndex + 1, cell.ColIndex));
            if (GetCell(cell.RowIndex - 1, cell.ColIndex)?.Unit?.type == type)
                tempCellHashSet.Add(GetCell(cell.RowIndex - 1, cell.ColIndex));
            if (GetCell(cell.RowIndex, cell.ColIndex + 1)?.Unit?.type == type)
                tempCellHashSet.Add(GetCell(cell.RowIndex, cell.ColIndex + 1));
            if (GetCell(cell.RowIndex, cell.ColIndex - 1)?.Unit?.type == type)
                tempCellHashSet.Add(GetCell(cell.RowIndex, cell.ColIndex - 1));
            
            return tempCellHashSet;
        }
        
        private HashSet<Cell> FindMatchingCells(Cell cell)
        {
            CubeType type = cell.GetUnit<Cube>().CubeInfo.type;
            tempCellHashSet.Clear();
            Queue<(int row, int col)> cellsToVisit = new();
            HashSet<(int row, int col)> visitedCells = new();
            cellsToVisit.Enqueue((cell.RowIndex, cell.ColIndex));
            visitedCells.Add((cell.RowIndex, cell.ColIndex));
            
            while (cellsToVisit.Count > 0)
            {
                var (currentRow, currentCol) = cellsToVisit.Dequeue();
                Cube currentCube = GetCell(currentRow, currentCol)?.GetUnit<Cube>();

                if (currentCube == null || currentCube.CubeInfo.type != type)
                {
                    continue;
                }

                tempCellHashSet.Add(GetCell(currentRow, currentCol));
                
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
            
            return tempCellHashSet;
        }
        
        private void BlastAdjacentCubes(Cell clickedCell)
        {
            HashSet<Cell> matchGroup = FindMatchingCells(clickedCell);
            foreach (var cell in matchGroup)
            {
                cell.state = CellState.Empty;
                cell.GetUnit<Cube>().Blast();
                cell.Unit = null;
            }
            
            ShiftGrid(matchGroup);
        }
        
        private void ShiftGrid(HashSet<Cell> matchGroup)
        {
            HashSet<int> colIndexes = new HashSet<int>();
            
            foreach (var cell in matchGroup)
            {
                if (colIndexes.Add(cell.ColIndex))
                {
                    ShiftColumn(cell.ColIndex);
                }
            }
        }
        
        private async void ShiftColumn(int colIndex)
        {
            tasks.Clear();
            int dropCount = GetEmptyCellCount(colIndex);
            Dictionary<int, int> cellShiftInfo = GetShiftableUnitsInfo(colIndex);
            ShiftExistingUnits(cellShiftInfo, colIndex);
            SpawnNewUnits(colIndex, dropCount);
            
            await UniTask.WhenAll(tasks);
        }

        private void SpawnNewUnits(int colIndex, int dropCount)
        {
            for (int i = 0; i < dropCount; i++)
            {
                GetCell(rowCount - dropCount + i, colIndex).Fill(unitSpawner.SpawnNewCube(colIndex, i));
                tasks.Add(GetCell(rowCount - dropCount + i, colIndex).Unit.transform.DOLocalMove(Vector2.zero, 5f)
                    .SetSpeedBased().ToUniTask());
            }
        }

        private void ShiftExistingUnits(Dictionary<int, int> cellShiftInfo, int colIndex)
        {
            foreach (var info in cellShiftInfo.Reverse())
            {
                GetCell(info.Key - info.Value, colIndex).Fill(GetCell(info.Key, colIndex).Unit);
                tasks.Add(GetCell(info.Key - info.Value, colIndex).Unit.transform.DOLocalMove(Vector2.zero, 5f)
                    .SetSpeedBased().ToUniTask());
            }
        }

        private Dictionary<int, int> GetShiftableUnitsInfo(int colIndex)
        {
            Dictionary<int, int> cellShiftInfo = new(columnCount);
            
            for (int i = RowCount - 1; i > 0 ; i--)
            {
                if (GetCell(i, colIndex)?.state != 
                    CellState.Empty && GetCell(i, colIndex)?.Unit?.type == GameUnitType.Cube) // TODO: Change condition to -> if unit is IShiftable
                {
                    int cellShiftCount = GetCellShiftCount(GetCell(i, colIndex));
                    if (cellShiftCount > 0 )
                    {
                        cellShiftInfo.Add(i, cellShiftCount);
                    }
                }
            }

            return cellShiftInfo;
        }
        
        private int GetEmptyCellCount(int colIndex)
        {
            int dropCount = 0;

            for (int i = 0; i < rowCount; i++)
            {
                if (GetCell(i, colIndex)?.state == CellState.Empty)
                {
                    dropCount++;
                }
            }
            
            return dropCount;
        }

        private int GetCellShiftCount(Cell cell)
        {
            int cellShiftCount = 0;
            for (int i = cell.RowIndex; i >= 0; i--)
            {
                if (GetCell(i, cell.ColIndex)?.state == CellState.Empty)
                {
                    cellShiftCount++;
                }
            }

            return cellShiftCount;
        }
    }
}
