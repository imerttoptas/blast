using System;
using UnityEngine;

namespace Runtime.Gameplay
{
    public class GridBuilder : MonoBehaviour
    {
        [SerializeField] private CubeTypeCatalogue cubeTypeCatalogue;

        [SerializeField] private Cube cubePrefab;
        [SerializeField] private int colorCount;

        public void GenerateGrid(Cube[,] cubes, int rowCount, int columnCount)
        {
            Vector2 startPoint = Vector2.one / 2f - new Vector2(columnCount / 2f, rowCount / 2f);

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < columnCount; colIndex++)
                {
                    Cube createdCube = Instantiate(cubePrefab);
                    Vector2 targetPos = startPoint + new Vector2(colIndex, rowIndex);
                    createdCube.Init(rowIndex: rowIndex, colIndex: colIndex, Vector2.one, targetPos, transform,
                        cubeTypeCatalogue.GetRandomCubeTypeFromRange(colorCount - 1));
                    cubes[rowIndex, colIndex] = createdCube;
                }
            }
        }
    }
}
