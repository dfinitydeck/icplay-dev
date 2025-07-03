//--------------------------------------------------------------------//
//                     EFRAMEWORK LIMITED LICENSE                     //
//  Copyright (C) EFramework Software Co., Ltd. All rights reserved.  //
//                  SEE LICENSE.md FOR MORE DETAILS.                  //
//--------------------------------------------------------------------//

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Object cache pool (thread safe, does not support ILR)
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPool<T> where T : class
{
    private const int POOL_SIZE_LIMIT = 400;

    private static Queue<T> mPools = new Queue<T>();

    /// <summary>
    /// Get instance (thread safe, does not support ILR)
    /// </summary>
    /// <returns></returns>
    public static T Get()
    {
        T ret = null;
        if (mPools.Count > 0)
        {
            lock (mPools)
            {
                try
                {
                    ret = mPools.Dequeue();
                }
                catch (Exception e)
                {
                    var str = typeof(T).FullName;
                    UnityEngine.Debug.LogWarning($"ObjectPool({str}): pools dequeue error: {e.Message}");
                }
            }
        }

        if (ret == null)
        {
#if EFRAME_ENABLE_POOL_DEBUG
                var str = typeof(T).FullName;
                UnityEngine.Debug.Log($"ObjectPool({str}): new instance.");
#endif
            ret = Activator.CreateInstance<T>();
        }

        return ret;
    }

    /// <summary>
    /// Cache instance (thread safe, does not support ILR)
    /// </summary>
    /// <param name="obj"></param>
    public static void Put(T obj)
    {
        if (obj == null) return;
        if (mPools.Count < POOL_SIZE_LIMIT)
        {
            lock (mPools) mPools.Enqueue(obj);
        }
    }
}

/// <summary>
/// Object cache pool (thread safe)
/// </summary>
public class ObjectPool
{
    private const int POOL_SIZE_LIMIT = 400;

    private readonly Queue mPools = new Queue();

    private Type mType;

    private Func<object> mActivator;

    public ObjectPool(Type type)
    {
        mType = type;
    }

    public ObjectPool(Func<object> activator)
    {
        mActivator = activator;
    }

    /// <summary>
    /// Get instance (thread safe)
    /// </summary>
    /// <returns></returns>
    public object Get()
    {
        object ret = null;
        if (mPools.Count > 0)
        {
            lock (mPools)
            {
                try
                {
                    ret = mPools.Dequeue();
                }
                catch (Exception e)
                {
                    var str = mType != null ? mType.FullName :
                        mActivator != null ? mActivator.Method.DeclaringType.Name : "null";
                    UnityEngine.Debug.LogWarning($"ObjectPool({str}): pools dequeue error: {e.Message}");
                }
            }
        }

        if (ret == null)
        {
#if EFRAME_ENABLE_POOL_DEBUG
                var str =
 mType != null ? mType.FullName : mActivator != null ? mActivator.Method.DeclaringType.FullName : "null";
                UnityEngine.Debug.Log($"ObjectPool({str}): new instance.");
#endif
            ret = mActivator != null ? mActivator.Invoke() : Activator.CreateInstance(mType);
        }

        return ret;
    }

    /// <summary>
    /// Cache instance (thread safe)
    /// </summary>
    /// <param name="obj"></param>
    public void Put(object obj)
    {
        if (obj == null) return;
        if (mPools.Count < POOL_SIZE_LIMIT)
        {
            lock (mPools) mPools.Enqueue(obj);
        }
    }
}