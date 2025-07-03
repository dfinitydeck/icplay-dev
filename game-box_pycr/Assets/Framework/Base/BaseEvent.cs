using System;
using UnityEngine;

namespace Framework
{

    public class Event_0 : IFireBase
    {
        private System.Action _action;

        public Event_0(System.Action method)
        {
            this._action = method;
        }

        public void Call(params object[] args)
        {
            if ((args == null) || (args != null && args.Length == 0))
                _action();
            else
                Log.Error("Event_0 parameter error!");
        }
    }

    public class Event_1<A> : IFireBase
    {
        private System.Action<A> _action;

        public Event_1(System.Action<A> method)
        {
            this._action = method;
        }

        public void Call(params object[] args)
        {
            if (args != null && args.Length == 1)
            {
                if (this._action != null)
                    this._action((A)args[0]);
            }
            else
                Log.Error("Event_1 parameter error!");
        }
    }

    public class Event_2<A, B> : IFireBase
    {
        private System.Action<A, B> _action;
        public Event_2(System.Action<A, B> method)
        {
            this._action = method;
        }

        public void Call(params object[] args)
        {
            if (args != null && args.Length == 2)
            {
                if (this._action != null)
                    _action((A)args[0], (B)args[1]);
            }
            else
                Log.Error("Event_2 parameter error!");
        }
    }

    public class Event_3<A, B, C> : IFireBase
    {
        private System.Action<A, B, C> _action;
        public Event_3(System.Action<A, B, C> method)
        {
            this._action = method;
        }

        public void Call(params object[] args)
        {
            if (args != null && args.Length == 3)
            {
                if (this._action != null)
                    _action.Invoke((A)args[0], (B)args[1], (C)args[2]);
                // _action((A)args[0], (B)args[1], (C)args[2]);
            }
            else
                Log.Error("Event_3 parameter error!");
        }
    }

    public class Event_4<A, B, C, D> : IFireBase
    {
        private System.Action<A, B, C, D> _action;
        public Event_4(System.Action<A, B, C, D> method)
        {
            this._action = method;
        }

        public void Call(params object[] args)
        {
            if (args != null && args.Length == 4)
            {
                if (this._action != null)
                    _action((A)args[0], (B)args[1], (C)args[2], (D)args[3]);
            }
            else
                Log.Error("Event_4 parameter error!");
        }
    }

    public class Event_5<A, B, C, D, E> : IFireBase
    {
        private Action<A, B, C, D, E> _action;
        public Event_5(Action<A, B, C, D, E> method)
        {
            this._action = method;
        }

        public void Call(params object[] args)
        {
            if (args != null && args.Length == 5)
            {
                if (this._action != null)
                    _action((A)args[0], (B)args[1], (C)args[2], (D)args[3], (E)args[4]);
            }
            else
                Log.Error("Event_5 parameter error!");
        }
    }

}