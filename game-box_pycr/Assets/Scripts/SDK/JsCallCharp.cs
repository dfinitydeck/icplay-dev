using System;
using Framework;
using UnityEngine;

public class JsCallCharp : MonoBehaviour
{
    // // Success callback
    // public void OnSuccess(string result)
    // {
    //     Log.Info("Success: " + result);
    // }

    public void OnSuccess(string res)
    {
        Log.Info($"Success:res : {res}");
        bool ret = false || res != "false";
        EventMgr.Instance.TriggerEvent(EventKey.PayResult,ret);
    }

    public void OnLogin(string res)
    {
        Log.Info($"Login success: {res}");
        GameMgr.Instance.SetName(res);
        EventMgr.Instance.TriggerEvent(EventKey.LoginSuccess,res);
        DataModule.Instance.Login();
        var intPtr = JsPlugin.GetIntPtr(res);
        JsPlugin.GetBalance(intPtr);
    }

    public void OnLogout()
    {
        Log.Info($"Logout completed");
        GameMgr.Instance.SetName("");
        EventMgr.Instance.TriggerEvent(EventKey.LogoutSuccess);
    }
    
    public void OnSuccess(IntPtr ptr)
    {
        var res = JsPlugin.GetStrByIntPtr(ptr);
        Log.Info($"Success: ptr:{res}");
    }

    public void OnSuccess()
    {
        Log.Info($"Success:OnSuccess");
    }

    public void OnBalance(string balance)
    {
        Log.Info($"OnBalance:{balance}");
        DataModule.Instance.SetBalance(balance);
        EventMgr.Instance.TriggerEvent(EventKey.PayResultRefresh);
    }


    // Error callback
    public void OnError(string error)
    {
        Log.Error("Failed: " + error);
    }
}