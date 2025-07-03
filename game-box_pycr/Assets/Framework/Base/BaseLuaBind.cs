// using System.Collections.Generic;
// using System;
// using System.Linq;
// using System.Threading;
// using UnityEngine;

// namespace Framework
// {
//     public class BaseLuaBind
//     {
//         protected string luaName = "";
//         public string LuaName { get { return luaName; } }

//         public BaseLuaBind()
//         {
//             luaName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName;
//         }

//         ~BaseLuaBind()
//         {
//             OnDestroy();
//         }

//         // Initialization
//         public virtual void OnInit()
//         {
//             // Subclass inheritance
//         }

//         public virtual void OnStart()
//         {
//             // Subclass inheritance
//         }

//         public virtual void OnDestroy()
//         {
//             // Subclass inheritance
//         }

//         // Send message
//         protected void Fire(string eventName, params object[] eventArgs)
//         {
//             EventManager.Instance.Fire(eventName, eventArgs);
//         }

//         // Get other BaseLuaBind modules
//         public static T FromLuaBind<T>() where T : BaseLuaBind
//         {
//             // return LuaManager.Instance.GetLuaBind<T>();
//         }
//     }
// }