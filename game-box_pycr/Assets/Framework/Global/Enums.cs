// Business module type
namespace Framework
{
    public enum ModuleType
    {
        MT_LogMgr = 0,          // Log Manager
        MT_DataMgr = 1,         // Data Layer Manager
        MT_CtrlMgr = 2,         // Business Layer Manager
        // MT_InputMgr = 3,        // Input Controller
        MT_FrameMgr = 5,        // Frame Split Manager
        MT_ResMgr = 6,          // Resource Manager
        MT_UIMgr = 7,         // UI Manager
        MT_AudioMgr = 8,        // Audio Manager
        MT_GUIMgr = 9,          // GUI Manager
        MT_EventMgr = 10,        // Event Manager     
        MT_LuaMgr = 11,        // Lua Manager
        MT_MonitorMgr = 12,     // Object Monitor Manager
        MT_WaitTimeMgr = 13,    // Delay Manager
        MT_StorageMgr = 14,     // Storage Manager
        MT_LanguageMgr = 15,    // Multi-language Manager
        MT_UXMgr = 16,          // UX Manager
    };

    // UI popup display type
    public enum UIPopType
    {
        UPT_None = 0,                 // No effect
        UPT_MinToMax = 1,             // Popup from minimum to maximum
        UPT_DownToUp = 2,             // Popup from bottom to top
        UPT_UpToDown = 3,             // Popup from top to bottom
        UPT_CenterToUP = 4,           // Popup from center to top
    }

    // UI display layer
    public enum UIShowLevel
    {
        LAYER_NONE = 0,             // Invalid layer
        LAYER_BG = 1000,            // Background layer
        LAYER_VIEW = 2000,          // View layer
        LAYER_POPUP = 3000,         // Popup layer
        LAYER_TIP = 4000,           // Tip layer
        LAYER_GUIDE = 5000,         // Guide layer
        LAYER_LOADING = 6000,       // Loading layer
        LAYER_MAX = 7000,           // Maximum layer
    }

    // UI display type
    public enum UIShowType
    {
        UST_PERMANENT = 1,          // Permanent display
        UST_REPLACE = 2,            // Replace display
        UST_PUSH = 3,               // Stack display
        UST_OVERLAY = 4,            // Overlay display
    }

    // UI events
    public class UIEvent
    {
        public const string OnInit = "UI_OnInit";
        public const string OnShow = "UI_OnShow";
        public const string OnHide = "UI_OnHide";
        public const string OnClose = "UI_OnClose";

        public const string OnPushView = "UI_OnPushView";
        public const string OnPopupView = "UI_OnPopupView";

        public const string OnLanguageChange = "UI_OnLanguageChange";
    }

    // Storage SDK type
    public enum StorageSDKType
    {
        ST_None = 0,                // Invalid
        ST_PlayFabSDK = 1,          // PlayFabSDK
    }

    // Multi-language type
    public enum LanguageType
    {
        Default = 0,                 // Default
        Chinese = 1,                 // Chinese
        English = 2,                 // English
        Russian = 3,                 // Russian
        German = 4,                  // German
        French = 5,                  // French
        Italian = 6,                 // Italian
        Japanese = 7,                // Japanese
        Spanish = 8,                 // Spanish
        Korean = 9,                  // Korean
        Portuguese = 10,             // Portuguese
        Max = 11,                    // Maximum value
    }

    public class GameNodeName
    {
        public const string GN_GameRoot = "GameRoot";
        public const string GN_UIRoot = "UIRoot";
        public const string GN_PoolNode = "PoolNode";
    }





    // public enum HttpEvent
    // {
    //     HE_Ok = 0,
    //     HE_TimeOut = -1,
    //     HE_Abort = -2,
    //     HE_Error = -3,
    // }

    // public enum PreloadType
    // {   // Preload type
    //     PT_Texture = 0,
    //     PT_Atlas = 1,
    //     PT_Prefab = 2,
    //     PT_Spine = 3
    // }

}

