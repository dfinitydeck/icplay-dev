using System;
using System.Collections.Generic;
using Framework.UIFrame;
using UI;
using UnityEngine;

namespace Framework
{
    public class AppEntry : MonoBehaviour
    {
        [SerializeField] private RectTransform m_UIRoot;
        [SerializeField] private UI_Start m_UIStart;
        // Module manager
        protected ModuleType[] runModules = 
        {
            ModuleType.MT_EventMgr, 
            ModuleType.MT_ResMgr,
            ModuleType.MT_WaitTimeMgr,
            ModuleType.MT_AudioMgr
        };

        // Module list
        protected Dictionary<ModuleType, IModuleInterface> moduleMap = new Dictionary<ModuleType, IModuleInterface>();

        protected static AppEntry instance;
        public static AppEntry Instance { get { return AppEntry.instance; } }

        private AsyncOperation sceneLoading;
        private Action<float> sceneLoadingAction;

        private void AppAwake()
        {
            OnAwake();
            LoadModules();
            InitModules(() =>
            {
                StartModules(() =>
                {
                    UIManager.Instance.SetUIRoot(m_UIRoot);
                    OnStart();
                    Log.Info("App.Start");
                });
            });
        }

        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            JsPlugin.OnLoaded();
        #endif
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            AppEntry.instance = this;
            DontDestroyOnLoad(this);
            Log.Info("App.Awake");
            AppAwake();
        }

        private void OnDestroy()
        {
            // StorageManager.Instance.SaveLocalData();

            Log.Info("App.OnDestroy");
        }

        public Transform GetGameNode(string name)
        {
            return transform.Find(name);
        }

        // Load basic modules
        protected void LoadModules()
        {
            IModuleInterface module = null;
            ModuleType type;
            for (var i = 0; i < runModules.Length; i++)
            {
                type = runModules[i];
                module = DoLoadModule(type);
                moduleMap[type] = module;
            };
        }

        protected IModuleInterface DoLoadModule(ModuleType type)
        {
            IModuleInterface module = null;
            switch (type)
            {
                case ModuleType.MT_EventMgr: module = EventManager.AddRoot(gameObject); break;
                case ModuleType.MT_ResMgr: module = ResourceManager.AddRoot(gameObject); break;
                case ModuleType.MT_WaitTimeMgr: module = TimeMgr.AddRoot(gameObject); break;
                case ModuleType.MT_AudioMgr: module = AudioManager.AddRoot(gameObject); break;
            }
            return module;
        }

        protected void InitModules(Action endCB)
        {
            IModuleInterface module = null;
            var ge = this.moduleMap.GetEnumerator();
            void DoInit()                               // Initialize recursively in order
            {
                if (ge.MoveNext())
                {
                    module = ge.Current.Value;
                    module?.Init((bool isOk) =>
                    {
                        DoInit();
                    });
                }
                else
                {
                    Log.Info("App.InitModules");
                    endCB?.Invoke();
                    ge.Dispose();
                }
            }

            DoInit();
        }

        protected void StartModules(Action endCB)
        {
            IModuleInterface module = null;
            var ge = this.moduleMap.GetEnumerator();
            void DoRun()                               // Initialize recursively in order
            {
                if (ge.MoveNext())
                {
                    module = ge.Current.Value;
                    module?.Run((bool isOk) =>
                    {
                        DoRun();
                    });
                }
                else
                {
                    Log.Info("App.StartModules");
                    endCB?.Invoke();
                    ge.Dispose();
                }
            }

            DoRun();
        }


        // Initialize App
        protected virtual void OnAwake()
        {
            // This is inherited by subclasses, defining the framework functional modules (runModules) that need to be loaded at runtime
        }

        // Start App
        protected virtual void OnStart()
        {
            GameMgr.Instance.Init(m_UIStart);
        }

    }
}