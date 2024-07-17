using System;
using UnityEngine;

namespace Runtime.Gameplay
{
    public class GridBuilder : MonoBehaviour
    {
        [SerializeField] private CubeTypeCatalogue cubeTypeCatalogue;
        [SerializeField] private int rowCount;
        [SerializeField] private int columnCount;
        [SerializeField] private Cube cubePrefab;
        [SerializeField] private int colorCount;
        private void Start()
        {
            GenerateGrid();
        }

        private void GenerateGrid()
        {
            Vector2 startPoint = Vector2.one / 2 - new Vector2(columnCount / 2f, rowCount / 2f);

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < columnCount; colIndex++)
                {
                    Cube createdCube = Instantiate(cubePrefab);
                    Vector2 targetPos = startPoint + new Vector2(colIndex, rowIndex);
                    createdCube.Init(Vector2.one, targetPos, transform,
                        cubeTypeCatalogue.GetRandomCubeTypeFromRange(colorCount - 1));
                }
            }
        }
    }
}
