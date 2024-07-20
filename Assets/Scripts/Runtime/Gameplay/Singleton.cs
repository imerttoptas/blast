using UnityEngine;

namespace Runtime.Gameplay
{
    public abstract class Singleton<T> : MonoBehaviour where T: Singleton<T>  
    {
        public static T instance;
        public bool isPersistant = true;

        protected virtual void Awake()
        {
            if (isPersistant)
            {
                if (instance)
                {
                    Destroy(gameObject); 
                }
                else
                {
                    instance = this as T;
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                instance = this as T;
            }
        }
    }
}