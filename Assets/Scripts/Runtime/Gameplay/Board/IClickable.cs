using UnityEngine;

namespace Runtime.Gameplay.Board
{
    public interface IClickable
    {
        public GameObject GameObject { get;}
        public bool IsClickable { get; set; }
        public void OnClick();
    }
}