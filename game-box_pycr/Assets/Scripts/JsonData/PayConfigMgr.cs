using System;
using System.Collections.Generic;
using Framework;
using LitJson;
using UnityEngine;

[Serializable]
public class PayConfigMgr:Singleton<PayConfigMgr>
{
    private PayRoot m_PayRoot;
    public void Init()
    {
        // Log.Info("Init PayConfigMgr---");
        // ResourceManager.Instance.LoadGameAssetCallback<TextAsset>("Data/payConfig.json", (asset) =>
        // {
        //     var textStr = asset.text;
        //     m_PayRoot = JsonMapper.ToObject<PayRoot>(textStr);
        // });
    }

    public List<PayInfo> GetPayList()
    {
        if (m_PayRoot == null)
            return null;
        return m_PayRoot.payConfigs;
    }
}

[Serializable]
public class PayRoot
{
    public List<PayInfo> payConfigs;
}
[Serializable]
public class PayInfo
{
    public int id;
    public int money;
    public int count;
}