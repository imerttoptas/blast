using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Gameplay
{
    public class GridController : Singleton<GridController>
    {
        [SerializeField] private UnitSpawner unitSpawner;
        [SerializeField] private GridBuilder gridBuilder;
        [SerializeField] private InputManager inputManager;
        
        private HashSet<Cell> tempCellHashSet = new();
        private List<UniTask> tasks = new();

        private Cell[,] grid = new Cell[12, 12];
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
        
        private void Start()
        {
            gridBuilder.GenerateGrid(grid);
            ArrangeCubeStates();
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
        
        private void TryMakeMove(Cell cell)
        {
            if (CheckValidMove(cell))
            {
                BlastMatchGroup(cell);
            }
        }
        
        private bool CheckValidMove(Cell cell)
        {
            return GetPositionsAdjacentCubes(cell).Count > 0;
        }
        
        private HashSet<Vector2> GetPositionsAdjacentCubes(Cell cell)
        {
            HashSet<Vector2> positions = new();
            
            if (!cell.GetUnit<Cube>())
            {
                return positions;
            }
            
            CubeType type = cell.GetUnit<Cube>().CubeInfo.type;
            foreach (var neighbourCell in GetAdjacentCells(cell))
            {
                if (neighbourCell.GetUnit<Cube>()?.CubeInfo.type == type)
                {
                    positions.Add(new Vector2(neighbourCell.RowIndex, neighbourCell.ColIndex));
                }
            }

            return positions;
        }
        
        private HashSet<Cell> GetAdjacentCells(Cell cell) // TODO: Change Cell to Vector2
        {
            HashSet<Cell> adjacentCells = new HashSet<Cell>();

            Vector2[] directions = {
                Vector2.up,
                Vector2.down,
                Vector2.right,
                Vector2.left
            };

            foreach (var direction in directions)
            {
                Cell neighbour = GetCell((int)(cell.RowIndex + direction.y), (int)(cell.ColIndex + direction.x));
                if (neighbour != null)
                {
                    adjacentCells.Add(neighbour);
                }
            }
    
            return adjacentCells;
        }
        
        private HashSet<Cell> FindCellsToPop(Cell cell, bool checkBoxes = true) // Change tuple to Vector2 
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
                
                if (checkBoxes && GetCell(currentRow,currentCol).Unit is IAffectedByNeighbour)
                {
                    tempCellHashSet.Add(GetCell(currentRow, currentCol));
                    continue;
                }
                
                Cube currentCube = GetCell(currentRow, currentCol)?.GetUnit<Cube>();
                
                if (currentCube == null || currentCube.CubeInfo.type != type)
                {
                    continue;
                }
                
                tempCellHashSet.Add(GetCell(currentRow, currentCol));
                
                foreach (var adjacentCell in GetAdjacentCells(GetCell(currentRow, currentCol)))
                {
                    if (visitedCells.Add((adjacentCell.RowIndex,adjacentCell.ColIndex)))
                    {
                        cellsToVisit.Enqueue((adjacentCell.RowIndex, adjacentCell.ColIndex));
                    }
                }
            }

            return tempCellHashSet;
        }
        
        private void BlastMatchGroup(Cell clickedCell)
        {
            HashSet<Cell> blastGroup = new HashSet<Cell>();
            foreach (var cell in FindCellsToPop(clickedCell))
            {
                cell.Unit.Pop(() =>
                {
                    _ = cell.RemoveUnit();
                    blastGroup.Add(cell);
                });
            }
            
            ShiftGrid(blastGroup);
   
        }
        
        private async void ShiftGrid(HashSet<Cell> matchGroup)
        {
            HashSet<int> colIndexes = new HashSet<int>();
            List<UniTask> shiftTasks = new();

            foreach (var cell in matchGroup)
            {
                if (colIndexes.Add(cell.ColIndex))
                {
                    shiftTasks.Add(ShiftColumn(cell.ColIndex));
                }
            }
            
            await UniTask.WhenAll(shiftTasks);
            ArrangeCubeStates();
            if (!CheckValidGrid())
            {
                ShuffleGrid();
            }
        }
        
        private void ArrangeCubeStates()
        {
            HashSet<Cell> checkedCells = new HashSet<Cell>();
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColCount; j++)
                {
                    Cell cell = GetCell(i, j);
                    if (cell && !checkedCells.Contains(cell)&& cell.GetUnit<Cube>())
                    {
                        HashSet<Cell> cubeGroup = FindCellsToPop(cell, false);
                        SetCubeGroupState(cubeGroup);
                        checkedCells.UnionWith(cubeGroup);
                    }
                }
            }
        }

        private void SetCubeGroupState(HashSet<Cell> cells)
        {
            foreach (var cell in cells)
            {
                cell.GetUnit<Cube>()?.SetState(cells.Count);
            }
        }
        
        private async UniTask ShiftColumn(int colIndex)
        {
            tasks.Clear();
            int spawnCount = GetEmptyCellCount(colIndex);
            ShiftExistingUnits(colIndex);
            SpawnNewUnits(colIndex, spawnCount);
            
            await UniTask.WhenAll(tasks);
        }
        
        private void SpawnNewUnits(int colIndex, int dropCount)
        {
            for (int i = 0; i < dropCount; i++)
            {
                GetCell(rowCount - dropCount + i, colIndex).Fill(unitSpawner.SpawnNewCube(colIndex, i));
                tasks.Add(GetCell(rowCount - dropCount + i, colIndex).Unit.transform.DOLocalMove(Vector2.zero, 10f)
                    .SetSpeedBased().ToUniTask());
            }
        }

        private void ShiftExistingUnits(int colIndex)
        {
            foreach (var info in GetShiftableUnitsInfo(colIndex).Reverse())
            {
                GetCell(info.Key - info.Value, colIndex).Fill(GetCell(info.Key, colIndex).RemoveUnit());
                tasks.Add(GetCell(info.Key - info.Value, colIndex).Unit.transform.DOLocalMove(Vector2.zero, 10f)
                    .SetSpeedBased().ToUniTask());
            }
        }
        
        private Dictionary<int, int> GetShiftableUnitsInfo(int colIndex)
        {
            Dictionary<int, int> cellShiftInfo = new(columnCount);
            
            for (int i = RowCount - 1; i > 0 ; i--)
            {
                if (GetCell(i, colIndex)?.state != 
                    CellState.Empty && GetCell(i, colIndex)?.Unit is IShiftable)
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
        
        private int GetCellShiftCount(Cell cell)
        {
            int cellShiftCount = 0;
            for (int i = cell.RowIndex; i >= 0; i--)
            {
                if (GetCell(i,cell.ColIndex)?.Unit is IFixedUnit)
                {
                    break;
                }
                if(GetCell(i, cell.ColIndex)?.state == CellState.Empty)
                {
                    cellShiftCount++;
                }

            }

            return cellShiftCount;
        }
        
        private int GetEmptyCellCount(int colIndex)
        {
            int dropCount = 0;
            
            for (int i = rowCount -1 ; i > -1; i--)
            {
                if (GetCell(i,colIndex)?.Unit is IFixedUnit)
                {
                    break;
                }
                if (GetCell(i, colIndex)?.state == CellState.Empty)
                {
                    dropCount++;
                }
            }
            
            return dropCount;
        }
        
        private bool CheckValidGrid()
        {
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColCount; j++)
                {
                    Cell currentCell = GetCell(i, j);
                    if (currentCell != null && currentCell.GetUnit<Cube>() && CheckValidMove(currentCell))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private async void ShuffleGrid()
        {
            var shuffleUnits = new List<GameUnit>();
            var shuffleCells = new List<Cell>();
            
            int retryCount = 0;
            while (!CheckValidGrid())
            {
                shuffleCells.Clear();
                shuffleUnits.Clear();
                GetShuffleCellsAndUnits();
                ShuffleList(shuffleUnits);
                for (int i = 0; i < shuffleCells.Count; i++)
                {
                    shuffleCells[i].Fill(shuffleUnits[i]);
                }

                retryCount++;
            }
            
            await ShuffleAnimation(shuffleCells);
            Debug.Log("Found the valid grid at " + retryCount + " try");
            return;

            void GetShuffleCellsAndUnits()
            {
                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < ColCount; j++)
                    {
                        Cell cell = grid[i, j];
                        if (cell != null && cell.state != CellState.Empty && cell.Unit is not IFixedUnit)
                        {
                            shuffleUnits.Add(cell.RemoveUnit());
                            shuffleCells.Add(cell);
                            cell.IsClickable = false;
                        }
                    }
                }
            }
        }
        
        private void ShuffleList<T>(List<T> list) // TODO: Move to Extensions class.
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }

        private async UniTask ShuffleAnimation(List<Cell> shuffleCells) // TODO: Split shuffle operations and animations
        {
            List<UniTask> shuffleTasks = new List<UniTask>();
            foreach (var cell in shuffleCells)
            {
                shuffleTasks.Add(cell.Unit.transform.DOLocalMove(Vector2.zero, 1f).SetEase(Ease.InOutBack).OnComplete(
                    () =>
                    {
                        cell.IsClickable = true;
                    }).ToUniTask());
            }
            await UniTask.WhenAll(shuffleTasks); 
        }
    }
}
