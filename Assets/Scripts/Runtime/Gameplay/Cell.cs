using DG.Tweening;
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
        public Cube Cube { get; set;}
        
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
        
        public void Fill(Cube cube)
        {
            Cube = cube;
            cube.transform.SetParent(transform);
            state = CellState.Full;
        }
        
        public void OnClick()
        {
            
        }

    }

    public enum CellState
    {
        Empty = 0,
        Full = 1
    }
}
