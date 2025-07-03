
    using System;
    using System.Collections.Generic;
    using Framework;
    

    public class EventMgr:Singleton<EventMgr>
    {
        private Dictionary<EventKey, Action<object>> eventDictionary = new Dictionary<EventKey, Action<object>>();

        public void AddListener(EventKey eventName, Action<object> listener)
        {
            if (eventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent += listener;
                eventDictionary[eventName] = thisEvent;
            }
            else
            {
                thisEvent += listener;
                eventDictionary.Add(eventName, thisEvent);
            }
        }

        public void RemoveListener(EventKey eventName, Action<object> listener)
        {
            if (eventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent -= listener;
                eventDictionary[eventName] = thisEvent;
            }
        }

        public void TriggerEvent(EventKey eventName, object data = null)
        {
            if (eventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent?.Invoke(data);
            }
        }
    }

    public enum EventKey
    {
        LoginSuccess,
        LogoutSuccess,
        PayResult,
        PlayerDataUpdate,
        PayResultRefresh,
    }