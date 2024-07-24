using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay.Pooling
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        public List<ObjectPool> pools;
        
        [System.Serializable]
        public class ObjectPool
        {
            public PoolObjectType poolObjectType; 
            public GameObject originalPrefab;
            public HashSet<GameObject> ActiveObjects = new();
            public Queue<GameObject> ObjectsInQueue = new();
            public int InitialSize; 
        }
        
        protected override void Awake()
        {
            base.Awake();
            InitializePool();
        }
        
        private void InitializePool()
        {
            foreach (ObjectPool pool in pools)
            {
                for (int i = 0; i < pool.InitialSize; i++)
                {
                    GameObject obj = Instantiate(pool.originalPrefab);
                    obj.SetActive(false); 
                    pool.ObjectsInQueue.Enqueue(obj);
                }
            }
        }
        
        public GameObject Get(PoolObjectType poolObjectType)
        {
            ObjectPool targetPool = pools.Find(x => x.poolObjectType == poolObjectType);
            
            if (targetPool.ObjectsInQueue.Count > 0)
            {
                GameObject obj = targetPool.ObjectsInQueue.Dequeue();
                targetPool.ActiveObjects.Add(obj);
                obj.SetActive(true);
                return obj;
            }
            GameObject obj2 = Instantiate(targetPool.originalPrefab);
            targetPool.ActiveObjects.Add(obj2);
            obj2.SetActive(true);
            return obj2;
        }

        public void ReturnToPool(GameObject obj)
        { 
            PoolObjectType type = obj.GetComponent<PoolObject>().type;
            ObjectPool targetPool = pools.Find(x => x.poolObjectType == type);
            
            if (targetPool.ActiveObjects.Contains(obj))
            {
                targetPool.ActiveObjects.Remove(obj);
                targetPool.ObjectsInQueue.Enqueue(obj);
                obj.SetActive(false);
                obj.transform.parent = transform; 
            }
            else
            {
                Debug.LogError("Duplication error");
            }
        }
        
        public void ReturnAllToPoolByType(PoolObjectType type)
        {
            ObjectPool targetPool = pools.Find(x => x.poolObjectType == type);
            List<GameObject> tmp = new List<GameObject>(targetPool.ActiveObjects);
            
            int count = targetPool.ActiveObjects.Count; 
            for (int i = 0; i < count; i++)
            {
                ReturnToPool(tmp[i]);
            }
        }
        
        public void ResetAll()
        {
            foreach (ObjectPool pool in pools)
            {
                ReturnAllToPoolByType(pool.poolObjectType);
            }
        }
    }

    public enum PoolObjectType
    {
        Cell = 0,
        Cube = 1,
        Box = 2
    }
}
