using System;
using System.Collections.Generic;
using Runtime.Gameplay.Board;
using UnityEngine;

namespace Runtime.Gameplay.Input
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        public Action<Cell> OnClickedToClickableObject;
        
        private RaycastHit2D[] hits = new RaycastHit2D[10];
        private List<RaycastHit2D> hitList = new();

        void Update()
        {
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                IClickable clickedObject = TryGetClickedObject();

                if (clickedObject != null)
                {
                    if (clickedObject.GameObject.TryGetComponent(out Cell cell))
                    {
                        OnClickedToClickableObject?.Invoke(cell);
                    }
                }
            }
        }
        
        private IClickable TryGetClickedObject()
        {
            RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(UnityEngine.Input.mousePosition), Vector2.zero);
            
            if (hit.collider != null && hit.collider.TryGetComponent(out IClickable clickedObject))
            {
                if (clickedObject.IsClickable)
                {
                    return clickedObject;
                }
            }

            return null;
        }
        
        private IClickable TryGetClosestObjectInACircle(float inputCircleRadius)
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            int count = Physics2D.CircleCastNonAlloc(mousePos, inputCircleRadius, Vector2.zero, hits);
            
            if (count > 0)
            {
                hitList.Clear();
                for (int i = 0; i < count; i++)
                {
                    hitList.Add(hits[i]);
                }
                
                hitList.Sort((x,y)=> Vector2.SqrMagnitude((Vector2)x.point-mousePos).CompareTo(Vector2.SqrMagnitude((Vector2)y.point-mousePos)));
                if (hitList[0].collider.TryGetComponent(out IClickable clickedObject))
                {
                    if (clickedObject.IsClickable)
                    {
                        return clickedObject;
                    }
                }
            }
            
            return null;
        }
    }
}
