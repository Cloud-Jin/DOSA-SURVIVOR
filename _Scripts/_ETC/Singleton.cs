using UnityEngine;

namespace ProjectM
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        [SerializeField] private bool dontDestroy = true;
        private static T instance = null;
        [HideInInspector] public bool initComplete;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = InitManager<T>();
                }

                return instance;
            }
        }

        protected static U InitManager<U>() where U : MonoBehaviour
        {
            GameObject go = null;
            U obj = FindObjectOfType<U>();
            if (obj == null)
            {
                go = new GameObject(typeof(U).Name);
                go.AddComponent<U>();
            }
            else
            {
                go = obj.gameObject;
            }

            DontDestroyOnLoad(go);
            return go.GetComponent<U>();
        }

        private void Awake()
        {
            if (instance == null)
            {
                if (dontDestroy)
                {
                    Instance.Init();
                }
                else
                {
                    instance = GetComponent<T>();
                    Init();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected abstract void Init();
    }
}