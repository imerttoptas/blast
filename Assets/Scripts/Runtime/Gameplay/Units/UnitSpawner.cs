using System;
using System.Collections.Generic;
using Runtime.Gameplay.Board;
using Runtime.Gameplay.Pooling;
using Runtime.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Gameplay.Units
{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField] private CubeInfoCatalogue cubeInfoCatalogue;
        [SerializeField] private Cube cubePrefab;
        [SerializeField] private List<GameUnitInfo> gameUnitInfoList;
        
        public void SpawnRandomUnit(int row, int col)
        {
            GameUnit createdUnit = GetRandomGameUnit();
            createdUnit.transform.localScale = Vector2.one;
            Cell cell = BoardController.instance.GetCell(row, col);
            cell.Fill(createdUnit);
            createdUnit.transform.position = BoardController.instance.GetCell(row, col).transform.position;
            InitializeUnitOnCell(cell);
        }

        private void InitializeUnitOnCell(Cell cell)
        {
            switch (cell.Unit.type)
            {
                case GameUnitType.Cube:
                    cell.GetUnit<Cube>()
                        .Init(
                            cubeInfoCatalogue.GetCubeInfoOfType(BoardController.instance.boardInfo.GetRandomCubeType()));
                    break;
                case GameUnitType.Box:
                    cell.GetUnit<Box>().Init();
                    break;
                case GameUnitType.Default:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public Cube SpawnNewCube(int colIndex, int index)
        {
            Cube createdCube = ObjectPoolManager.instance.Get(PoolObjectType.Cube).GetComponent<Cube>();
            createdCube.transform.position =
                BoardController.instance.GetCell(BoardController.instance.RowCount - 1, colIndex).transform.position +
                Vector3.up * ((index + 1) * Constants.CUBE_SPAWN_UP_VALUE);
            createdCube.Init(
                cubeInfoCatalogue.GetCubeInfoOfType(BoardController.instance.boardInfo.GetRandomCubeType()));
            return createdCube;
        }

        private GameUnit GetRandomGameUnit()
        {
            return Random.Range(0, 1f) < Constants.BOX_PROBABILITY
                ? ObjectPoolManager.instance.Get(PoolObjectType.Box).GetComponent<GameUnit>()
                : ObjectPoolManager.instance.Get(PoolObjectType.Cube).GetComponent<GameUnit>();
        }
        
    }
    
    [Serializable]
    public class GameUnitInfo
    {
        public GameUnitType type;
        public GameUnit prefab;
        
    }
}
