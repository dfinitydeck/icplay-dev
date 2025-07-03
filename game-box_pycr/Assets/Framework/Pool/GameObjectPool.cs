//--------------------------------------------------------------------//
//                     EFRAMEWORK LIMITED LICENSE                     //
//  Copyright (C) EFramework Software Co., Ltd. All rights reserved.  //
//                  SEE LICENSE.md FOR MORE DETAILS.                  //
//--------------------------------------------------------------------//

using System;
using System.Collections.Generic;
using Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Unity object cache pool (main thread)
/// </summary>
public class GameObjectPool : MonoBehaviour
{
    /// <summary>
    /// Object cache type
    /// </summary>
    public enum CacheType
    {
        /// <summary>
        /// Scene cache
        /// </summary>
        Scene,

        /// <summary>
        /// Global cache
        /// </summary>
        Shared,
    }

    /// <summary>
    /// Cache handler
    /// </summary>
    private class CacheHandler
    {
        public string Path;
        public GameObject Origin;
        public CacheType Type;
        public Queue<GameObject> Pool = new Queue<GameObject>();
    }

    /// <summary>
    /// Object pool
    /// </summary>
    private static readonly Dictionary<string, CacheHandler> mPools = new Dictionary<string, CacheHandler>();

    /// <summary>
    /// All objects
    /// </summary>
    private static readonly Dictionary<GameObject, byte> mObjects = new Dictionary<GameObject, byte>();

    /// <summary>
    /// Objects in use
    /// </summary>
    private static readonly Dictionary<GameObject, CacheHandler> mUsings =
        new Dictionary<GameObject, CacheHandler>();

    /// <summary>
    /// GC optimization
    /// </summary>
    private static readonly List<GameObject> mKeysToRemove = new List<GameObject>();

    /// <summary>
    /// GC optimization
    /// </summary>
    private static readonly List<string> mKeysToRemove2 = new List<string>();

    /// <summary>
    /// Whether disposed
    /// </summary>
    private static bool mDisposed;

    /// <summary>
    /// Singleton
    /// </summary>
    public static GameObjectPool Instance;

    void Awake()
    {
        Instance = this;
        mDisposed = false;
    }

    void OnDestroy()
    {
        mDisposed = true;
        Instance = null;
        mPools.Clear();
        mUsings.Clear();
        Debug.Log("pool is destory");
    }

    public void Clear()
    {
        mDisposed = true;
        Instance = null;
        mPools.Clear();
        mUsings.Clear();
    }

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="root">Parent node</param>
    public static void Initialize(Transform root = null)
    {
        if (Instance == null)
        {
            var go = new GameObject("Pool");
            go.AddComponent<GameObjectPool>();
            if (root) go.transform.SetParent(root);
            else DontDestroyOnLoad(go);
            // AssetManager.BeforeUnloadAll -= OnUnload;
            // AssetManager.BeforeUnloadAll += OnUnload;
        }
    }

    private static void OnUnload(Scene scene)
    {
        mKeysToRemove.Clear();
        foreach (var kvp in mUsings)
        {
            if (kvp.Value.Type == CacheType.Scene) mKeysToRemove.Add(kvp.Key);
        }

        foreach (var go in mKeysToRemove)
        {
            mUsings.Remove(go);
            try
            {
                Destroy(go);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                mObjects.Remove(go);
            }
        }

        mKeysToRemove2.Clear();
        foreach (var kvp in mPools)
        {
            if (kvp.Value.Type == CacheType.Scene) mKeysToRemove2.Add(kvp.Key);
        }

        foreach (var key in mKeysToRemove2)
        {
            var handler = mPools[key];
            mPools.Remove(key);
            mObjects.Remove(handler.Origin);
            while (handler.Pool.Count > 0)
            {
                var go = handler.Pool.Dequeue();
                try
                {
                    Destroy(go);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    mObjects.Remove(go);
                }
            }
        }
    }

    /// <summary>
    /// Whether source instance exists (main thread)
    /// </summary>
    /// <param name="key">Indexer</param>
    /// <returns></returns>
    public static bool Has(string key)
    {
        if (mDisposed || string.IsNullOrEmpty(key)) return false;
        if (Instance == null) Initialize();
        else if (Instance.enabled)
        {
        }

        ; // Ensure called on main thread
        return mPools.ContainsKey(key);
    }

    /// <summary>
    /// Set source instance (main thread)
    /// </summary>
    /// <param name="key">Indexer</param>
    /// <param name="origin">Source instance</param>
    /// <param name="cache">Cache type</param>
    public static void Set(string key, GameObject origin, CacheType cache = CacheType.Scene)
    {
        if (mDisposed || string.IsNullOrEmpty(key)) return;
        if (Instance == null) Initialize();
        else if (Instance.enabled)
        {
        }

        ; // Ensure called on main thread
        if (Has(key))
        {
            Debug.LogWarning("GameObjectPool.Set: key exists:" + key);
        }
        else
        {
            var handler = ObjectPool<CacheHandler>.Get();
            handler.Path = key;
            handler.Origin = origin;
            handler.Type = cache;
            mPools[key] = handler;
            if (!mObjects.ContainsKey(origin)) mObjects.Add(origin, 0);
        }
    }

    /// <summary>
    /// Remove source instance (main thread)
    /// </summary>
    /// <param name="key">Indexer</param>
    public static void Del(string key)
    {
        if (mDisposed || string.IsNullOrEmpty(key)) return;
        if (Instance == null) Initialize();
        else if (Instance.enabled)
        {
        }

        ; // Ensure called on main thread
        if (Has(key))
        {
            mPools.TryGetValue(key, out var handler);
            mPools.Remove(key);
            mObjects.Remove(handler.Origin);
            foreach (var v in handler.Pool) mObjects.Remove(v);

            mKeysToRemove.Clear();
            foreach (var kvp in mUsings)
            {
                if (kvp.Value == handler) mKeysToRemove.Add(kvp.Key);
            }

            foreach (var k in mKeysToRemove)
            {
                mUsings.Remove(k);
                mObjects.Remove(k);
            }
        }
    }

    /// <summary>
    /// Instantiate object (main thread)
    /// </summary>
    /// <param name="key">Indexer</param>
    /// <param name="active">Whether to activate</param>
    /// <param name="position">World coordinates</param>
    /// <param name="rotation">World orientation</param>
    /// <param name="scale">Local scale</param>
    /// <param name="life">Lifecycle</param>
    /// <returns></returns>
    public static GameObject Get(string key, bool active = true, Vector3 position = default,
        Quaternion rotation = default, Vector3 scale = default, float life = -1f)
    {
        if (mDisposed || string.IsNullOrEmpty(key)) return null;
        if (Instance == null) Initialize();
        else if (Instance.enabled)
        {
        }

        ; // Ensure called on main thread
        if (mPools.TryGetValue(key, out var handler))
        {
            GameObject go;
            if (handler.Pool.Count > 0) go = handler.Pool.Dequeue();
            else
            {
                go = Instantiate(handler.Origin);
                go.name = handler.Origin.name;
                mObjects.Add(go, 0);
            }

            go.SetActive(active);
            if (position != default) go.transform.position = position;
            if (rotation != default) go.transform.rotation = rotation;
            if (scale != default) go.transform.localScale = scale;
            mUsings[go] = handler;
            if (life > 0) TimeMgr.SetTimeout(() => Put(go),life);
            return go;
        }

        return null;
    }

    /// <summary>
    /// Recycle instance (main thread)
    /// </summary>
    /// <param name="go">Instance object</param>
    /// <param name="delay">Delay recycle</param>
    public static void Put(GameObject go, float delay = -1f)
    {
        if (mDisposed || go == null) return;
        if (Instance == null) Initialize();
        else if (Instance.enabled)
        {
        }

        ; // Ensure called on main thread
        if (delay > 0)
            TimeMgr.SetTimeout(() =>
            {
                if (go) DoPut(go);
            },delay);
        else DoPut(go);
    }

    private static void DoPut(GameObject go)
    {
        if (mUsings.TryGetValue(go, out var handler))
        {
            go.transform.parent = Instance.transform;
            go.SetActive(false);
            handler.Pool.Enqueue(go);
            mUsings.Remove(go);
        }
        else
        {
            if (!mObjects.ContainsKey(go)) // Avoid multiple recycling
            {
                Destroy(go);
            }
        }
    }
}