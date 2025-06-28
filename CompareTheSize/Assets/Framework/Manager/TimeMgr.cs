using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Timer
/// </summary>
public class TimeMgr : SingletonManager<TimeMgr>,IModuleInterface
{
    private static List<Timer> timers = new List<Timer>();
    private static LinkedPool<Timer> timerPool = new LinkedPool<Timer>(() => new Timer());

    private void Update()
    {
        var delta = Time.deltaTime;
        for (int i = 0; i < timers.Count;)
        {
            var timer = timers[i];
            timer.LeftTime -= delta;
            if (timer.LeftTime <= 0)
            {
                try
                {
                    timer.Fun?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                if (timer.Repeat) timer.LeftTime = timer.RawTime;
                else
                {
                    if (timer.Repeat) ClearInterval(timer);
                    else ClearTimeout(timer);
                    continue;
                }
            }

            i++;
        }
    }

    /// <summary>
    /// Delayed call
    /// </summary>
    /// <param name="fun">Callback function</param>
    /// <param name="timeout">Timeout time (seconds)</param>
    /// <returns></returns>
    public static Timer SetTimeout(Action fun, float timeout)
    {
        var timer = timerPool.Get();
        timer.Fun = fun;
        timer.RawTime = timeout;
        timer.LeftTime = timeout;
        timer.Repeat = false;
        timers.Add(timer);
        return timer;
    }

    /// <summary>
    /// Cancel delayed call
    /// </summary>
    /// <param name="timer">Timer instance</param>
    public static void ClearTimeout(Timer timer)
    {
        var idx = timers.IndexOf(timer);
        if (idx >= 0)
        {
            timers.RemoveAt(idx);
            timerPool.Release(timer);
        }
    }

    /// <summary>
    /// Interval call
    /// </summary>
    /// <param name="fun">Callback function</param>
    /// <param name="interval">Interval time (seconds)</param>
    /// <returns></returns>
    public static Timer SetInterval(Action fun, float interval)
    {
        var timer = timerPool.Get();
        timer.Fun = fun;
        timer.RawTime = interval;
        timer.LeftTime = interval;
        timer.Repeat = true;
        timers.Add(timer);
        return timer;
    }

    /// <summary>
    /// Cancel interval call
    /// </summary>
    /// <param name="timer">Timer instance</param>
    public static void ClearInterval(Timer timer)
    {
        var idx = timers.IndexOf(timer);
        if (idx >= 0)
        {
            timers.RemoveAt(idx);
            timerPool.Release(timer);
        }
    }

    public Coroutine StartCor(IEnumerator cor)
    {
        return StartCoroutine(cor);
    }

    public void StopCor(IEnumerator cor)
    {
        StopCoroutine(cor);
    }

    public void StopCor(Coroutine cor)
    {
        StopCoroutine(cor);
    }

    public void ExecuteEndOfFrame(Action fun)
    {
        StartCoroutine(EndOfFrame(fun));
    }

    IEnumerator EndOfFrame(Action fun)
    {
        yield return new WaitForEndOfFrame();
        fun?.Invoke();
    }

    public void Init(Action<bool> onInitEnd)
    {
        onInitEnd?.Invoke(true);
    }

    public void Run(Action<bool> onRunEnd)
    {
        onRunEnd?.Invoke(true);
    }
}

public class Timer
{
    public Action Fun;
    public float LeftTime;
    public float RawTime;
    public bool Repeat;
}