/*
 * CocosCreator Game Framework
 * 
 * @Author: Murphy
 * @Date: 2022-11-23 23:28:25
 * @LastEditTime: 2023-11-28 19:13:04
 * @Description: Four-layer queue structure
 */

using System.Collections.Generic;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Framework
{
    // Game object pool
    public class GameObjectPool
    {
        public const int DEFAULT_SIZE = 3;              // Object pool initial count
        public const int MAX_SIZE = 10;                 // Object pool maximum count

        public bool IsInit { get; private set; } = false;
        // Objects that need to be stored in the object pool
        private GameObject poolObject = null;
        private Queue<GameObject> pool = new Queue<GameObject>();
        private int defaultSize = DEFAULT_SIZE;
        private int maxSize = MAX_SIZE;

        public GameObjectPool(int size = DEFAULT_SIZE, int max = MAX_SIZE)
        {
            defaultSize = size;
            maxSize = max;
            IsInit = false;
        }

        ~GameObjectPool()
        {
            Clear();
        }

        public async UniTask<GameObjectPool> InitPoolAsnyc(string path)
        {
            GameObject go = await ResourceManager.Instance.LoadGameObjectAsync(path);
            if (go != null)
            {
                InitPool(go);
                return this;
            }
            else
            {
                return null;
            }
        }

        public void InitPool(string path, Action<GameObjectPool> cb = null)
        {
            ResourceManager.Instance.LoadGameObjectCallback(path, (GameObject go) =>
            {
                if (go != null)
                {
                    InitPool(go);
                    cb?.Invoke(this);
                }
                else
                {
                    cb?.Invoke(null);
                }
            });
        }

        public void InitPool(GameObject go)
        {
            Clear();

            poolObject = go;
            InitPoolObject(poolObject);

            var count = (defaultSize < 0) ? DEFAULT_SIZE : defaultSize;
            for (int i = 0; i < count - 1; i++)
            {
                InitPoolObject();
            }
            IsInit = true;
        }

        private GameObject InitPoolObject(GameObject go = null)
        {
            var instGo = (go != null) ? go : GameObject.Instantiate(poolObject);
            Transform parent = AppEntry.Instance.GetGameNode(GameNodeName.GN_PoolNode);
            instGo.transform.SetParent(parent);
            instGo.SetActive(false);
            pool.Enqueue(instGo);
            return instGo;
        }

        public GameObject Get()
        {
            GameObject go;
            if (pool.Count > 0)
            {
                go = pool.Dequeue();
                go.SetActive(true);
            }
            else
            {
                go = GameObject.Instantiate(poolObject);
            }

            return go;
        }

        public void Put(GameObject go)
        {
            if (go != null)
            {
                if (pool.Count < maxSize)
                {
                    go.SetActive(false);
                    Transform parent = AppEntry.Instance.GetGameNode(GameNodeName.GN_PoolNode);
                    go.transform.SetParent(parent);
                    pool.Enqueue(go);
                }
                else
                {
                    GameObject.Destroy(go);
                }
            }
        }

        public void Clear()
        {
            while (pool.Count > 0)
            {
                GameObject.Destroy(pool.Dequeue());
            }
        }
    }

    // Game object Map
    public class GameObjectMap
    {
        private string MapName { get; set; }
        // Object pool based on GameObjectPool
        private Dictionary<string, GameObjectPool> poolMap = new Dictionary<string, GameObjectPool>();

        public GameObjectMap(string mapName)
        {
            MapName = mapName;
        }
        ~GameObjectMap()
        {
            ClearAll();
        }

        // Get an object pool from the cache pool
        public GameObjectPool GetPool(string name, int size = GameObjectPool.DEFAULT_SIZE, int max = GameObjectPool.MAX_SIZE)
        {
            GameObjectPool pool = null;
            if (!poolMap.TryGetValue(name, out pool))
            {
                pool = new GameObjectPool(size, max);
                poolMap.Add(name, pool);
            }
            return pool;
        }

        // Store the object pool into the cache pool
        public void PutPool(string name, GameObjectPool pool)
        {
            GameObjectPool srcPool = null;
            if (poolMap.TryGetValue(name, out srcPool))
            {
                poolMap.Add(name, pool);
            }
        }

        // Get an object from the object pool with the specified name. If the object pool has no available objects, return null.
        public GameObject Get(string name)
        {
            GameObjectPool pool = null;
            if (poolMap.TryGetValue(name, out pool))
            {
                return pool.Get();
            }
            else
            {
                return null;
            }
        }

        // Store an object that is no longer needed into the object pool in the cache pool.
        public void Put(string name, GameObject go)
        {
            GameObjectPool pool = null;
            if (poolMap.TryGetValue(name, out pool))
            {
                pool.Put(go);
            }
        }

        // Clear all objects in the object pool
        public void ClearPool(string name)
        {
            GameObjectPool pool = null;
            if (poolMap.TryGetValue(name, out pool))
            {
                pool.Clear();
            }
        }

        // Clear the entire cache pool
        public void ClearAll()
        {
            GameObjectPool pool = null;
            var ge = this.poolMap.GetEnumerator();

            while (ge.MoveNext())
            {
                pool = ge.Current.Value;
                if (pool != null)
                {
                    pool.Clear();
                }
            }
            poolMap.Clear();
        }
    }

    public class PoolManager : SingletonManager<PoolManager>, IModuleInterface
    {
        // Object pool based on GameObjectPool
        private string BASE_MAP_NAME = "Base";
        private Dictionary<string, GameObjectMap> mapDict = new Dictionary<string, GameObjectMap>();

        public void Init(Action<bool> onInitEnd)
        {
            mapDict.Add(BASE_MAP_NAME, new GameObjectMap(BASE_MAP_NAME));
            onInitEnd?.Invoke(true);
        }

        public void Run(Action<bool> onRunEnd)
        {
            onRunEnd?.Invoke(true);
        }

        public void OnDestroy()
        {
            ClearAll();
        }

        public GameObjectMap GetMap(string mapName)
        {
            GameObjectMap map = null;
            if (!mapDict.TryGetValue(mapName, out map))
            {
                map = new GameObjectMap(mapName);
                mapDict.Add(mapName, map);
            }
            return map;
        }

        // Get an object pool from the cache pool
        public GameObjectPool GetPool(string poolName, int size = GameObjectPool.DEFAULT_SIZE, int max = GameObjectPool.MAX_SIZE)
        {
            return GetMapPool(BASE_MAP_NAME, poolName, size, max);
        }

        public GameObjectPool GetMapPool(string mapName, string poolName, int size = GameObjectPool.DEFAULT_SIZE, int max = GameObjectPool.MAX_SIZE)
        {
            GameObjectMap map;
            mapDict.TryGetValue(mapName, out map);
            if (map != null)
            {
                return map.GetPool(poolName, size, max);
            }
            else
            {
                return null;
            }
        }

        // Store the object pool into the cache pool
        public void PutPool(string poolName, GameObjectPool pool)
        {
            PutMapPool(BASE_MAP_NAME, poolName, pool);
        }

        public void PutMapPool(string mapName, string poolName, GameObjectPool pool)
        {
            GameObjectMap map = mapDict[mapName];
            if (map != null)
            {
                map.PutPool(poolName, pool);
            }
        }

        // Get an object from the object pool with the specified name. If the object pool has no available objects, return null.
        public GameObject Get(string poolName)
        {
            return Get(BASE_MAP_NAME, poolName);
        }

        public GameObject Get(string mapName, string poolName)
        {
            GameObjectMap map = mapDict[mapName];
            if (map != null)
            {
                return map.Get(poolName);
            }
            else
            {
                return null;
            }
        }

        // Store an object that is no longer needed into the object pool in the cache pool.
        public void Put(string poolName, GameObject go)
        {
            Put(BASE_MAP_NAME, poolName, go);
        }

        public void Put(string mapName, string poolName, GameObject go)
        {
            GameObjectMap map = mapDict[mapName];
            if (map != null)
            {
                map.Put(poolName, go);
            }
        }

        // Clear all objects in the object pool
        public void ClearPool(string poolName)
        {
            ClearMapPool(BASE_MAP_NAME, poolName);
        }

        public void ClearMapPool(string mapName, string poolName)
        {
            GameObjectMap map = mapDict[mapName];
            if (map != null)
            {
                map.ClearPool(poolName);
            }
        }

        public void ClearMap(string mapName)
        {
            GameObjectMap map = mapDict[mapName];
            if (map != null)
            {
                map.ClearAll();
            }
        }

        // Clear the entire cache map
        public void ClearAll()
        {
            GameObjectMap map = null;
            var ge = mapDict.GetEnumerator();
            while (ge.MoveNext())
            {
                map = ge.Current.Value;
                if (map != null)
                {
                    map.ClearAll();
                }
            }
            mapDict.Clear();
        }
    }
}
