/*
 * Copyright (c) 2023 by Murphy, All Rights Reserved. 
 * 
 * @Author: Murphy
 * @Date: 2022-11-23 23:28:25
 * @LastEditTime: 2023-11-28 19:13:04
 * @Description: Event Manager
 */

using System.Collections.Generic;
using System;
using System.Threading;

namespace Framework
{
    public sealed class EventManager : SingletonManager<EventManager>, IModuleInterface
    {

        private int mainThreadID = 0;
        private int eventHandleSeed = 1;

        private Dictionary<string, List<int>> eventNameMap = new Dictionary<string, List<int>>();
        private Dictionary<object, List<int>> eventObjectMap = new Dictionary<object, List<int>>();
        private Dictionary<int, IFireBase> eventMap = new Dictionary<int, IFireBase>();

        public void Init(Action<bool> onInitEnd)
        {
            mainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            onInitEnd?.Invoke(true);
        }

        public void Run(Action<bool> onRunEnd)
        {
            onRunEnd?.Invoke(true);
        }

        public int Add(string name, object target, IFireBase method)
        {
            // Save event dictionary
            int handle = eventHandleSeed;
            eventMap.Add(handle, method);

            // Save event name
            List<int> evtList;
            if (!eventNameMap.TryGetValue(name, out evtList))
            {
                evtList = new List<int>();
                eventNameMap[name] = evtList;
            }
            evtList.Add(handle);

            // Save event object
            evtList = null;
            if (!eventObjectMap.TryGetValue(target, out evtList))
            {
                evtList = new List<int>();
                eventObjectMap[target] = evtList;
            }
            evtList.Add(handle);

            eventHandleSeed++;
            return handle;
        }

        List<int> removeList = new List<int>(10);
        private void doFire(string name, params object[] args)
        {
            List<int> evtList;
            if (!eventNameMap.TryGetValue(name, out evtList)) return;

            removeList.Clear();
            var count = evtList.Count;
            for (int i = 0; i < count; i++)
            {
                int handle = evtList[i];

                IFireBase em;
                if (eventMap.TryGetValue(handle, out em))
                {
                    em.Call(args);
                }
                else
                {
                    removeList.Add(handle);
                }
            }

            // Remove events that do not exist in the event name dictionary
            var removeCount = removeList.Count;
            if (removeCount > 0)
            {
                for (int i = 0; i < removeCount; i++)
                {
                    evtList.Remove(removeList[i]);
                }
            }
        }

        public void Fire(string name, params object[] args)
        {
            if (Thread.CurrentThread.ManagedThreadId == mainThreadID)
            {
                doFire(name, args);
            }
            else
            {
                // Multi-thread event processing
            }
        }

        public void Off(object target)
        {
            List<int> evtList;
            if (!eventObjectMap.TryGetValue(target, out evtList))
                return;

            var count = evtList.Count;
            for (int i = 0; i < count; i++)
            {
                int evtHandle = evtList[i];
                if (eventMap.ContainsKey(evtHandle))
                    eventMap.Remove(evtHandle);
            }
            eventObjectMap.Remove(target);
        }

        public int On(string evtName, System.Action method)
        {
            return Add(evtName, method.Target, new Event_0(method));
        }

        public int On<A>(string evtName, System.Action<A> method)
        {
            return Add(evtName, method.Target, new Event_1<A>(method));
        }

        public int On<A, B>(string eventName, System.Action<A, B> method)
        {
            return Add(eventName, method.Target, new Event_2<A, B>(method));
        }

        public int On<A, B, C>(string eventName, System.Action<A, B, C> method)
        {
            return Add(eventName, method.Target, new Event_3<A, B, C>(method));
        }

        public int On<A, B, C, D>(string eventName, System.Action<A, B, C, D> method)
        {
            return Add(eventName, method.Target, new Event_4<A, B, C, D>(method));
        }

        public int On<A, B, C, D, E>(string eventName, Action<A, B, C, D, E> method)
        {
            return Add(eventName, method.Target, new Event_5<A, B, C, D, E>(method));
        }

    }
}