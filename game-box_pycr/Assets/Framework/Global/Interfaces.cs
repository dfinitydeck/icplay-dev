using System;
using UnityEngine;

namespace Framework
{

    public interface IModuleInterface
    {
        void Init(Action<bool> onInitEnd);
        void Run(Action<bool> onRunEnd);
    }

    public interface IFireBase
    {
        void Call(params object[] args);
    }
}
