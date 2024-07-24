using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Runtime.Gameplay.Units;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Gameplay.Board
{
    public class BoardController : Singleton<BoardController>
    {
        [SerializeField] private UnitSpawner unitSpawner;
        [FormerlySerializedAs("gridBuilder")] [SerializeField] private BoardBuilder boardBuilder;
        [SerializeField] private InputManager inputManager;
        [SerializeField] public CustomBoardInfo boardInfo;

        private HashSet<Cell> tempCellHashSet = new();
        private List<UniTask> tasks = new();

        private Cell[,] grid = new Cell[12, 12];
        public int RowCount => boardInfo.rowCount;
        public int ColCount => boardInfo.colCount;
        
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
            boardBuilder.GenerateGrid(grid, boardInfo);
            ArrangeCubeStates();
        }
        
        public Cell GetCell(int row, int col)
        {
            if (row >= 0 && row < RowCount && col >= 0 && col < ColCount)
            {
                return grid[row, col];
            }
            return null;
        }
    
        public HashSet<Cell> GetColumn(int colIndex)
        {
            tempCellHashSet.Clear();
    
            for (int i = 0; i < RowCount; i++)
            {
                tempCellHashSet.Add(GetCell(i, colIndex));
            }
    
            return tempCellHashSet;
        }
        
        public HashSet<Cell> GetRow(int rowIndex)
        {
            tempCellHashSet.Clear();
    
            for (int i = 0; i < ColCount; i++)
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
                if (neighbourCell.IsClickable && neighbourCell.GetUnit<Cube>()?.CubeInfo.type == type)
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
        
        private HashSet<Cell> FindCellsToPop(Cell cell)
        {
            var cubeGroup = FindCubeGroup(cell);
            
            foreach (var currentCell in cubeGroup.ToList())
            {
                if (!currentCell.IsClickable)
                {
                    cubeGroup.Remove(currentCell);
                }
                CheckNeighbour(currentCell);
            }
            
            return cubeGroup;

            void CheckNeighbour(Cell currentCell)
            {
                foreach (var affectedByNeighbourCells in GetAdjacentCells(currentCell))
                {
                    if (affectedByNeighbourCells.Unit is IAffectedByNeighbour)
                    {
                        cubeGroup.Add(affectedByNeighbourCells);
                    }
                }
            }
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
                        HashSet<Cell> cubeGroup = FindCubeGroup(cell);
                        SetCubeGroupState(cubeGroup);
                        checkedCells.UnionWith(cubeGroup);
                    }
                }
            }
        }
        
        private HashSet<Cell> FindCubeGroup(Cell cell)
        {
            HashSet<Cell> group = new HashSet<Cell>();
            HashSet<Vector2> visitedCells = new HashSet<Vector2>();
            CubeType type = cell.GetUnit<Cube>().CubeInfo.type;
            
            FindCubeGroupRecursive(cell, type, visitedCells, group);
            
            return group;
        }
        
        private void FindCubeGroupRecursive(Cell cell, CubeType type, HashSet<Vector2> visitedCells, HashSet<Cell> group)
        {
            if (cell?.GetUnit<Cube>()?.CubeInfo.type != type || !visitedCells.Add(new Vector2(cell.RowIndex, cell.ColIndex)))
            {
                return;
            }
            
            group.Add(cell);

            foreach (var adjacentCell in GetAdjacentCells(cell))
            {
                FindCubeGroupRecursive(adjacentCell, type, visitedCells, group);
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
                Cell cell = GetCell(RowCount - dropCount + i, colIndex);
                cell.IsClickable = false;
                cell.Fill(unitSpawner.SpawnNewCube(colIndex, i));
                cell.Unit.transform.DOKill();
                tasks.Add(cell.Unit.transform.DOLocalMove(Vector2.zero, Constants.SHIFT_ANIMATION_SPEED)
                    .OnComplete(
                        () =>
                        {
                            cell.IsClickable = true;
                        })
                    .SetSpeedBased().ToUniTask());
            }
        }
        
        private void ShiftExistingUnits(int colIndex)
        {
            foreach (var info in GetShiftableUnitsInfo(colIndex).Reverse())
            {
                Cell cell = GetCell(info.Key - info.Value, colIndex);
                cell.IsClickable = false;
                cell.Fill(GetCell(info.Key, colIndex).RemoveUnit());
                cell.Unit.transform.DOKill();
                tasks.Add(cell.Unit.transform.DOLocalMove(Vector2.zero, Constants.SHIFT_ANIMATION_SPEED).OnComplete(() =>
                    {
                        cell.IsClickable = true;
                    })
                    .SetSpeedBased().ToUniTask());
            }
        }
        
        private Dictionary<int, int> GetShiftableUnitsInfo(int colIndex)
        {
            Dictionary<int, int> cellShiftInfo = new(ColCount);
            
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
            
            for (int i = RowCount -1 ; i > -1; i--)
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
            while (!CheckValidGrid() && retryCount < 100)
            {
                shuffleCells.Clear();
                shuffleUnits.Clear();
                GetShuffleCellsAndUnits();
                shuffleUnits.Shuffle();
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

        private async UniTask ShuffleAnimation(List<Cell> shuffleCells) // TODO: Split shuffle operations and animations
        {
            List<UniTask> shuffleTasks = new List<UniTask>();
            foreach (var cell in shuffleCells)
            {
                cell.Unit.transform.DOKill();
                shuffleTasks.Add(cell.Unit.transform.DOLocalMove(Vector2.zero, Constants.SHUFFLE_ANIMATION_DURATION).SetEase(Ease.InOutBack).OnComplete(
                    () =>
                    {
                        cell.IsClickable = true;
                    }).ToUniTask());
            }
            await UniTask.WhenAll(shuffleTasks); 
        }
    }
}
