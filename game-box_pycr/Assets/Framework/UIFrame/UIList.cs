namespace Framework.UIFrame
{
    public class UIList
    {
        public static OpenInfo UI_Main = new OpenInfo{Name = "UI_Main",AssetPath = "Prefabs/UI_Main.prefab"};
        public static OpenInfo UI_Setting = new OpenInfo{Name = "UI_Setting",AssetPath = "Prefabs/UI_Setting.prefab"};
        public static OpenInfo UI_Rules = new OpenInfo{Name = "UI_Rules",AssetPath = "Prefabs/UI_Rules.prefab"};
        public static OpenInfo UI_Pay = new OpenInfo {Name = "UI_Pay",AssetPath = "Prefabs/UI_Pay.prefab"};
        public static OpenInfo UI_Rank = new OpenInfo {Name = "UI_Rank",AssetPath = "Prefabs/UI_Rank.prefab"};
        public static OpenInfo UI_PayRecord = new OpenInfo {Name = "UI_PayRecord",AssetPath = "Prefabs/UI_PayRecord.prefab"};
        
    }

    public class OpenInfo
    {
        public string Name;
        public string AssetPath;
    }
}