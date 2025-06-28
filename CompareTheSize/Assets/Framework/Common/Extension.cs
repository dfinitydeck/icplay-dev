using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Framework
{
    public static class Extension
    {
        /// <summary>
        /// Is empty
        /// is null or empty
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool isNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Is empty 
        /// is null
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool isNull(this object o)
        {
            return o == null;
        }

        /// <summary>
        /// Is empty
        /// is null or empty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static bool isNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null) return true;
            return !enumerable.Any();
        }

        public static T random<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable.isNullOrEmpty()) return default(T);
            return enumerable.ElementAt(UnityEngine.Random.Range(0, enumerable.Count()));
        }

        /// <summary>
        /// ForEach traversal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="action"></param>
        public static void forEach<T>(this IEnumerable<T> enumerable, UnityAction<T> action)
        {
            if (enumerable.isNullOrEmpty())
                return;
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        public static void forEach<T>(this IEnumerable<T> enumerable, UnityAction<T, int> action)
        {
            if (enumerable.isNullOrEmpty())
                return;
            var index = 0;
            foreach (var item in enumerable)
            {
                action(item, index++);
            }
        }

        /// <summary>
        /// Find component on child object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comp"></param>
        /// <param name="name"></param>
        /// <param name="addComponent"></param>
        /// <returns></returns>
        public static T find<T>(this Transform trans, string name, bool addComponent = false) where T : Component
        {
            if (!trans)
                return null;
            if (name.isNullOrEmpty())
                return null;

            var child = trans.Find(name);
            if (!child)
                return null;

            T t = child.GetComponent<T>();
            if (!t && addComponent)
            {
                t = child.addComponent<T>();
            }
            return t;
        }

        public static T[] findChilds<T>(this Transform trans, string name, bool addComponent = false) where T : Component
        {
            if (!trans)
                return null;
            if (name.isNullOrEmpty())
                return null;

            List<T> tList = new List<T>();
            int childCount = trans.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = trans.GetChild(i);
                if (child.name.Equals(name))
                {
                    T t = child.GetComponent<T>();
                    if (!t && addComponent)
                    {
                        t = child.addComponent<T>();
                    }
                    tList.Add(t);
                }
            }
            return tList.ToArray();
        }

        public static T addComponent<T>(this Component comp) where T : Component
        {
            if (!comp)
                return null;
            var t = comp.GetComponent<T>();
            if (t) return t;
            return comp.gameObject.AddComponent<T>();
        }

        public static void removeComponent<T>(this Component comp, bool includeChild = false) where T : Component
        {
            if (!comp)
                return;
            if (includeChild)
            {
                IEnumerable<UnityEngine.Object> oes = comp.GetComponentsInChildren<T>(true);
                if (oes.isNullOrEmpty())
                    return;
                foreach (var item in oes)
                {
                    UnityEngine.Object.Destroy(item);
                }
            }
            else
            {
                UnityEngine.Object o = comp.GetComponent<T>();
                if (!o)
                    return;
                UnityEngine.Object.Destroy(o);
            }
        }

        public static void removeComponent(this GameObject obj, string name)
        {
            if (!obj)
                return;

            UnityEngine.Object o = obj.GetComponent(name);
            if (!o)
                return;
            UnityEngine.Object.Destroy(o);
        }

        public static void removeComponent<T>(this GameObject go, bool includeChild = false) where T : Component
        {
            if (!go)
                return;
            IEnumerable<UnityEngine.Object> os = null;
            if (includeChild)
            {
                os = go.GetComponentsInChildren<T>(true);
            }
            else
            {
                os = go.GetComponents<T>();
            }
            if (os.isNullOrEmpty())
                return;
            foreach (var item in os)
            {
                UnityEngine.Object.DestroyImmediate(item);
            }
        }

        /// <summary>
        /// Add child object and add component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="trans"></param>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static T addChild<T>(this Transform trans, string name, Vector3 position) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(trans);
            go.transform.localPosition = position;
            go.transform.localScale = Vector3.one;
            return go.AddComponent<T>();
        }

        /// <summary>
        /// Add child object and add component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="trans"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T addChild<T>(this Transform trans, string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(trans);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            return go.AddComponent<T>();
        }

        public static string toTime(this ushort seconds)
        {
            return toTime((int)seconds);
        }

        public static string toTime(this int seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            string time = string.Empty;
            if (seconds > 86400)
            {
                long hour = seconds / 3600;
                byte min = (byte)((seconds % 3600) / 60);
                byte sec = (byte)((seconds % 3600) % 60);
                time = string.Format("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);
            }
            else if (seconds > 3600)
            {
                time = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
            }
            else
            {
                byte min = (byte)(seconds / 60);
                byte sec = (byte)(seconds % 60);
                if (min >= 10)
                {
                    time = string.Format("{0:D2}:{1:D2}", min, sec);
                }
                else
                {
                    time = string.Format("{0:D1}:{1:D2}", min, sec);
                }
            }
            //else
            //{
            //    if (seconds >= 10)
            //    {
            //        time = string.Format("{0:D2}", seconds);
            //    }
            //    else
            //    {
            //        time = string.Format("{0:D1}", seconds);
            //    }
            //}
            return time;
        }

        public static Vector2 readVector2(this BytesStream stream)
        {
            return new Vector2 { x = stream.Read16BitFloat(), y = stream.Read16BitFloat() };
        }

        public static Vector3 readVector3(this BytesStream stream)
        {
            return new Vector3 { x = stream.Read16BitFloat(), y = stream.Read16BitFloat(), z = stream.Read16BitFloat() };
        }

        public static void write(this BytesStream stream, Vector2 v)
        {
            stream.Write16BitFloat(v.x);
            stream.Write16BitFloat(v.y);
        }

        public static void write(this BytesStream stream, Vector3 v)
        {
            stream.Write16BitFloat(v.x);
            stream.Write16BitFloat(v.y);
            stream.Write16BitFloat(v.z);
        }

        public static bool isLetter(this string str)
        {
            if (str.isNullOrEmpty()) return false;

            return Regex.IsMatch(str, @"(?i)^[a-z]+$");
        }

        public static string getDirectory(this string str)
        {
            if (str.isNullOrEmpty()) return null;

            var index0 = str.LastIndexOf("/");
            var index1 = str.LastIndexOf(@"\");
            return str.Substring(0, Mathf.Max(index0, index1));
        }

        public static string getDirectoryName(this string str)
        {
            if (str.isNullOrEmpty()) return null;

            var index0 = str.LastIndexOf("/");
            var index1 = str.LastIndexOf(@"\");
            return str.Substring(Mathf.Max(index0, index1) + 1);
        }

        public static string getFileName(this string str)
        {
            if (str.isNullOrEmpty()) return null;
            var index0 = str.LastIndexOf("/");
            var index1 = str.LastIndexOf(@"\");
            var index2 = str.LastIndexOf(@".");
            var startIdx = Mathf.Max(index0, index1) + 1;

            if (index2 < 0)
            {
                return str.Substring(startIdx);
            }
            else
            {
                var length = index2 - startIdx;
                return str.Substring(startIdx, length);
            }
        }

        public static string firstLetterUpper(this string str)
        {
            if (str.isNullOrEmpty()) return null;
            if (str.Length == 1)
            {
                return str.ToUpper();
            }
            else
            {
                var tmp = str.Trim();
                return tmp.Substring(0, 1).ToUpper() + tmp.Substring(1);
            }
        }

        public static string getPanelName(this string str)
        {
            if (str.isNullOrEmpty()) return null;
            str = str.firstLetterUpper();
            if (str.EndsWith("Panel", StringComparison.OrdinalIgnoreCase))
            {
                str = str.Remove(str.LastIndexOf("Panel"));
            }
            return str += "Panel";
        }


        public static string getRelativePath(this string path)
        {
            if (path.isNullOrEmpty()) return null;
            return path.Substring(path.IndexOf("Assets/")).Replace("\\", "/");
        }

        public static string getObsolutePath(this string path)
        {
            if (path.Contains(Application.dataPath)) return path;
            return Application.dataPath + path.Replace("Assets", "").Replace("\\", "/");
        }

        public static string trimAll(this string str)
        {
            if (str.isNullOrEmpty()) return null;
            return str.Replace(" ", "").Replace("\t", "").Replace("\n", "");
        }

        public static int? indexOf<T>(this IEnumerable<T> enumerable, T item)
        {
            if (enumerable.isNullOrEmpty()) return null;
            if (item.isNull()) return null;
            var index = 0;
            foreach (var i in enumerable)
            {
                if (i.Equals(item))
                    return index;
                index++;
            }
            return null;
        }

        public static Component addComponent(this GameObject go, string name)
        {
            if (go == null)
            {
                Debug.LogError("addComponent go is null");
                return null;
            }
            if (name.isNullOrEmpty())
            {
                Debug.LogError("addComponent name is null");
                return null;
            }
            Type t = getType(name);
            if (t != null)
            {
                Component ret = go.GetComponent(t);
                if (ret == null)
                    ret = go.AddComponent(t);
                return ret;
            }
            return null;
        }

        public static Component addComponent(this Transform trans, string name)
        {
            if (trans == null)
            {
                Debug.LogError("addComponent trans is null");
                return null;
            }
            if (name.isNullOrEmpty())
            {
                Debug.LogError("addComponent name is null");
                return null;
            }
            return trans.gameObject.addComponent(name);
        }

        //https://forum.unity.com/threads/system-type-gettype-transform-not-work.40081/
        public static Type getType(string TypeName)
        {

            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, etc.
            var type = Type.GetType(TypeName);

            // If it worked, then we're done here
            if (type != null)
                return type;

            // Get the name of the assembly (Assumption is that we are using
            // fully-qualified type names)
            var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));

            // Attempt to load the indicated Assembly
            var assembly = Assembly.Load(assemblyName);
            if (assembly == null)
                return null;

            // Ask that assembly to return the proper Type
            return assembly.GetType(TypeName);

        }
    }


}