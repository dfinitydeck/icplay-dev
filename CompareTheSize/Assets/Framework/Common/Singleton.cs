using System.Threading.Tasks;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Singleton class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : new()
    {
        private static T _instance;
        static object _lock = new object();
        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new T();
                }
                return _instance;
            }
        }

        public static T getInstance()
        {
            return Instance;
        }
    }

    /// <summary>
    /// Mono singleton class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T m_Instance;
        private static readonly object m_Lock = new object();

        public static T Instance
        {
            get
            {
                lock (m_Lock)
                {
                    if (m_Instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).ToString();
                        DontDestroyOnLoad(obj);
                        m_Instance = obj.AddComponent<T>();
                    }
                    return m_Instance;
                }
            }
        }
        void Awake()
        {
            m_Instance =  this as T;
            OnInit();
        }

        /// <summary>
        /// Singleton initialization method
        /// </summary>
        protected virtual void OnInit()
        {
        }

        /// <summary>
        /// Singleton update method
        /// </summary>
        protected virtual void OnUpdate()
        {
        }

        /// <summary>
        /// Resource release
        /// </summary>
        protected virtual void OnDestroy()
        {
            Destroy(m_Instance.gameObject);
            m_Instance = null;
        }
    }

    /// <summary>
    /// Manager singleton class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T m_Instance;

        public static T Instance { get { return m_Instance; } }
        public static T AddRoot(GameObject root)
        {
            T mgr = root.GetComponent<T>();
            if (mgr == null)
            {
                mgr = root.AddComponent<T>();
                m_Instance = mgr;
            }
            return mgr;
        }

        // virtual public void Init()
        // {

        // }

        // virtual public void Run()
        // {

        // }
    }
}