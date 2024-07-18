using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay
{
    public class GridController : MonoBehaviour
    {
        [SerializeField] private GridBuilder gridBuilder;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private int rowCount;
        [SerializeField] private int columnCount;
        private Cube[,] grid = new Cube[10, 10];
        private List<Cube> tempCubeList = new();
        
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
            gridBuilder.GenerateGrid(grid, rowCount, columnCount);
        }
        
        private void TryMakeMove(Cube clickedCube)
        {
            if (CheckValidMove(clickedCube))
            {
                BlastAdjacentCubes(clickedCube);
            }
        }
        
        private Cube GetCube(int row, int col)
        {
            if ((row >= 0 && row < rowCount) && (col >= 0 && col < columnCount))
            {
                return grid[row, col];
            }
            return null;
        }
    
        public List<Cube> GetColumn(int colIndex)
        {
            tempCubeList.Clear();
    
            for (int i = 0; i < rowCount; i++)
            {
                tempCubeList.Add(GetCube(i, colIndex));
            }
    
            return tempCubeList;
        }
        
        public List<Cube> GetRow(int rowIndex)
        {
            tempCubeList.Clear();
    
            for (int i = 0; i < columnCount; i++)
            {
                tempCubeList.Add(GetCube(rowIndex, i));
            }
    
            return tempCubeList;
        }

        private bool CheckValidMove(Cube cube)
        {
            CubeType targetType = cube.CubeInfo.type;
            if (GetCube(cube.RowIndex + 1, cube.ColIndex)?.CubeInfo.type == targetType) return true;
            if (GetCube(cube.RowIndex - 1, cube.ColIndex)?.CubeInfo.type == targetType) return true;
            if (GetCube(cube.RowIndex, cube.ColIndex + 1)?.CubeInfo.type == targetType) return true;
            if (GetCube(cube.RowIndex, cube.ColIndex - 1)?.CubeInfo.type == targetType) return true;
            return false;
        }
        
        private List<Cube> FindAdjacentCubes(Cube cube)
        {
            CubeType type = cube.CubeInfo.type;
            tempCubeList.Clear();
            Queue<(int row, int col)> cellsToVisit = new();
            cellsToVisit.Enqueue((cube.RowIndex, cube.ColIndex));

            while (cellsToVisit.Count > 0)
            {
                var (currentRow, currentCol) = cellsToVisit.Dequeue();
                Cube currentCube = GetCube(currentRow, currentCol);

                if (currentCube?.CubeInfo.type != type || cellsToVisit.Contains((currentCube.RowIndex,currentCube.ColIndex)))
                {
                    continue;
                }
                
                tempCubeList.Add(currentCube);
                
                if (currentRow > 0) cellsToVisit.Enqueue((currentRow - 1, currentCol));
                if (currentRow < rowCount - 1) cellsToVisit.Enqueue((currentRow + 1, currentCol));
                if (currentCol > 0) cellsToVisit.Enqueue((currentRow, currentCol - 1));
                if (currentCol < columnCount - 1) cellsToVisit.Enqueue((currentRow, currentCol + 1));
            }

            return tempCubeList;
        }
        
        private void BlastAdjacentCubes(Cube cube)
        {
            List<Cube> adjacentCubes = FindAdjacentCubes(cube);
            
            foreach (var c in adjacentCubes)
            {
                grid[c.RowIndex, c.ColIndex] = null;
                c.OnClick();
            }
        }
    }
}
