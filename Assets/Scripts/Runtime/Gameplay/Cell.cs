using UnityEngine;

namespace Runtime.Gameplay
{
    public class Cell : MonoBehaviour , IClickable
    {
        public GameObject GameObject => gameObject;
        public bool IsClickable { get; set; }
        public CellState state;
        public int RowIndex { get; private set; }
        public int ColIndex { get; private set; }
        public GameUnit Unit { get; private set;}
        
        public void Init(Transform parent, Vector2 pos, Vector2 scale, int rowIndex, int colIndex)
        {
            IsClickable = true;
            state = CellState.Empty;
            transform.position = pos;
            transform.localScale = scale;
            RowIndex = rowIndex;
            ColIndex = colIndex;
            transform.SetParent(parent);
        }
        
        public void Fill(GameUnit targetUnit)
        {
            Unit = targetUnit;
            Unit.transform.SetParent(transform);
            state = CellState.Full;
        }
        
        public GameUnit RemoveUnit()
        {
            GameUnit cachedUnit = Unit;
            state = CellState.Empty;
            Unit = null;
            return cachedUnit;
        }
        
        public void OnClick()
        {
            
        }

        public T GetUnit<T>() where T : GameUnit
        {
            if (Unit is T targetUnit)
            {
                return targetUnit;
            }

            return null;
        }

    }

    public enum CellState
    {
        Empty = 0,
        Full = 1
    }
}
