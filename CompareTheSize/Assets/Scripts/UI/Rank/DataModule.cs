using System;
using System.Collections;
using System.Collections.Generic;
using Best.HTTP;
using Best.HTTP.JSON.LitJson;
using Framework;
using Framework.UIFrame;
using UnityEngine;

/// <summary>
/// Data request type
/// </summary>
public enum ReqType
{
    // Login request
    Login = 1,

    // Player data
    PlayerData = 2,

    // Rank data
    Rank = 3,

    // Save data
    Save = 4,
    
    // Keep alive
    Alive = 5,
    
    // Recharge record
    PayRecord = 6,
    
    // Recharge and count mapping
    PayAndCount = 7
}

public enum ReqRankType
{
    All = 1,
    Week = 2,
}

public class DataModule : Singleton<DataModule>
{
    private string m_Url = "https://pycr.ddecks.xyz//server/Request.php";

    private PlayerData m_PlayerData;
    private RankData m_RankData;
    private Coroutine m_PlayerDataCor;
    private string m_Balance;
    private List<PayCfg> m_PayCfgs;
    private List<PayRecordInfo> m_PayRecords;

    public PlayerData GetPlayerData()
    {
        return m_PlayerData;
    }

    public void Login()
    {
        var gameData = GameMgr.Instance.GetGameData();
        var nameStr = gameData.Name; ;
        if (string.IsNullOrEmpty(nameStr))
            return;
        string url = string.Format($"{m_Url}?type={(int)ReqType.Login}&playerId={nameStr}");
        HTTPRequest request = HTTPRequest.CreateGet(new Uri(url), LoginCallBack);
        request.Send();
    }

    private void LoginCallBack(HTTPRequest req, HTTPResponse resp)
    {
        if (resp == null || string.IsNullOrEmpty(resp.DataAsText))
        {
            Log.Error($"Login response data is null");
            return;
        }

        var jsonStr = resp.DataAsText;
        if (jsonStr == "SUCCESS")
        {
            Log.Info("Login successful, requesting player data");
            ReqPlayerData();
            ReqPayAndCount();
        }
    }
    
    public void Alive()
    {
        var gameData = GameMgr.Instance.GetGameData();
        var nameStr = gameData.Name; ;
        if (string.IsNullOrEmpty(nameStr))
            return;
        //    Log.Info($"Call game server login: nameStr:{nameStr}");
        string url = string.Format($"{m_Url}?type={(int)ReqType.Alive}&playerId={nameStr}");
        HTTPRequest request = HTTPRequest.CreateGet(new Uri(url), AliveCallBack);
        request.Send();
    }

    private void AliveCallBack(HTTPRequest req, HTTPResponse resp)
    {
        if (resp == null || string.IsNullOrEmpty(resp.DataAsText))
        {
            Log.Info($"Keep alive response is null");
            return;
        }
    }
    
    public void ReqPayRecord()
    {
        var gameData = GameMgr.Instance.GetGameData();
        var nameStr = gameData.Name; ;
        if (string.IsNullOrEmpty(nameStr))
            return;
        string url = string.Format($"{m_Url}?type={(int)ReqType.PayRecord}&playerId={nameStr}");
        HTTPRequest request = HTTPRequest.CreateGet(new Uri(url), PayRecordCallBack);
        request.Send();
    }

    private void PayRecordCallBack(HTTPRequest req, HTTPResponse resp)
    {
        if (resp == null || string.IsNullOrEmpty(resp.DataAsText))
        {
            Log.Error($"Transaction record request response is null");
            return;
        }
        Log.Info($"Transaction record: {resp.DataAsText}");
        try
        {
            m_PayRecords = JsonMapper.ToObject<List<PayRecordInfo>>(resp.DataAsText);
            UIManager.Instance.Show(UIList.UI_PayRecord);
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
        Log.Info($"Transaction record parsing completed");
    }
    
    public void ReqPayAndCount()
    {
        var gameData = GameMgr.Instance.GetGameData();
        var nameStr = gameData.Name; ;
        if (string.IsNullOrEmpty(nameStr))
            return;
        //    Log.Info($"Call game server login: nameStr:{nameStr}");
        string url = string.Format($"{m_Url}?type={(int)ReqType.PayAndCount}&playerId={nameStr}");
        HTTPRequest request = HTTPRequest.CreateGet(new Uri(url), PayAndCountCallBack);
        request.Send();
    }

    private void PayAndCountCallBack(HTTPRequest req, HTTPResponse resp)
    {
        if (resp == null || string.IsNullOrEmpty(resp.DataAsText))
        {
            Log.Error($"Recharge and count mapping request response is null");
            return;
        }
        Log.Info($"Recharge and count mapping: {resp.DataAsText}");
        try
        {
            m_PayCfgs = JsonMapper.ToObject<List<PayCfg>>(resp.DataAsText);
            Log.Info($"Recharge configuration {m_PayCfgs.Count}");
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
        
    }


    IEnumerator GetPlayerDataCor()
    {
        while (true)
        {
            ReqPlayerData();
            yield return new WaitForSeconds(1f);
        }
        yield break;
    }
    
    /// <summary>
    /// Polling player data
    /// </summary>
    public void StartGetPlayerData()
    {
        if (m_PlayerDataCor != null)
        {
            TimeMgr.Instance.StopCor(m_PlayerDataCor);
            m_PlayerDataCor = null;
        }
        TimeMgr.Instance.StartCor(GetPlayerDataCor());
    }

    public void ReqPlayerData()
    {
        var gameData = GameMgr.Instance.GetGameData();
        var nameStr = gameData.Name;
        if (string.IsNullOrEmpty(nameStr))
            return;
        string url = string.Format($"{m_Url}?type={(int)ReqType.PlayerData}&playerId={nameStr}");
 //       Log.Info($"Requesting player data url:{url}");
        HTTPRequest request = HTTPRequest.CreateGet(new Uri(url), RespPlayerDataCallBack);
        request.Send();
    }

    private void RespPlayerDataCallBack(HTTPRequest req, HTTPResponse resp)
    {
        if (resp == null || string.IsNullOrEmpty(resp.DataAsText))
        {
            Log.Error($"Failed to get player response data is null");
            return;
        }

        var preCount = m_PlayerData?.count;
        var jsonStr = resp.DataAsText;
        Log.Info($"Requesting player data: {jsonStr}");
        try
        {
            m_PlayerData = JsonMapper.ToObject<PlayerData>(jsonStr);
            if (preCount != m_PlayerData.count)
            {
                // Polling ended, call success
                EventMgr.Instance.TriggerEvent(EventKey.PlayerDataUpdate);
                if (m_PlayerDataCor != null)
                {
                    TimeMgr.Instance.StopCor(m_PlayerDataCor);
                    m_PlayerDataCor = null;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error($"Player data parsing exception: {e.ToString()}");
        }
       
    }

    public void MinusGameCount()
    {
        var playerData = GetPlayerData();
        playerData.count--;
    }

    public void ReqRankData(int reqCount = 100)
    {
        var gameData = GameMgr.Instance.GetGameData();
        var nameStr = gameData.Name;
        if (string.IsNullOrEmpty(nameStr))
            return;
        string url =
            string.Format(
                $"{m_Url}?type={(int)ReqType.Rank}&rankType={(int)ReqRankType.Week}&num={reqCount}&playerId={nameStr}");
        HTTPRequest request = HTTPRequest.CreateGet(new Uri(url), RespRankCallBack);
        request.Send();
    }

    private void RespRankCallBack(HTTPRequest req, HTTPResponse resp)
    {
        if (resp == null || string.IsNullOrEmpty(resp.DataAsText))
        {
            Log.Error($"Failed to get leaderboard data is null");
            return;
        }

     //   Log.Info($"Leaderboard data: {resp.DataAsText}");
        m_RankData = JsonMapper.ToObject<RankData>(resp.DataAsText);
        UIManager.Instance.Show(UIList.UI_Rank);
    }

    public RankData GetRankData()
    {
        return m_RankData;
    }

    /// <summary>
    /// Save data
    /// </summary>
    public void SaveData()
    {
        var gameData = GameMgr.Instance.GetGameData();
        string secretKey = "ddeck888qwer";
        SaveJson saveJson = new SaveJson();
        saveJson.score = GameMgr.Instance.GetCurScore();
        saveJson.name = gameData.Name;
        saveJson.lastTime = GetUnixTimestamp().ToString();
        string jsonStr = JsonMapper.ToJson(saveJson);
        var encryptStr = saveJson.name + jsonStr + secretKey;
        var secretkeyMD5 = Helper.GetMD5(encryptStr);
        string url = $"{m_Url}?type={(int)ReqType.Save}&sign={secretkeyMD5}&playerId={saveJson.name}&data={jsonStr}";
        HTTPRequest request = HTTPRequest.CreateGet(new Uri(url));
        request.Send();
    }

    private void SaveCallBack(HTTPRequest req, HTTPResponse resp)
    {
        if (resp == null || string.IsNullOrEmpty(resp.DataAsText))
        {
            Log.Error($"Save data response is null");
            return;
        }

 //       Log.Info($"resp save:{resp.DataAsText}");
    }

    /// <summary>
    /// Get UTC timestamp
    /// </summary>
    /// <returns></returns>
    public long GetUnixTimestamp()
    {
        long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return unixTimestamp;
    }

    public void SetBalance(string balance)
    {
        var ba = decimal.Parse(balance) / 100000000;
        decimal truncated = Math.Truncate(ba * 100m) / 100m;  // 0.12
        m_Balance = truncated.ToString("0.00");  
    }

    public string GetBalance()
    {
        return m_Balance;
    }

    public List<PayCfg> GetPayCfgs()
    {
        return m_PayCfgs;
    }

    public List<PayRecordInfo> GetPayRecordInfos()
    {
        return m_PayRecords;
    }
}

[Serializable]
public class SaveJson
{
    public int score;
    public string name;
    public string lastTime;
}

[Serializable]
public class PlayerData
{
    public int loginTs;
    public int count;
    public string allMondy;
}

[Serializable]
public class RankData
{
    public List<SingleRankData> rank;
    public OwnerRankData own;
}

[Serializable]
public class SingleRankData
{
    public int score;
    public string name;
    public string lastTime;
}

[Serializable]
public class OwnerRankData
{
    public int score;
    public int rank;
}

[Serializable]
public class PayCfg
{
    public int id;
    public decimal money;
    public int count;
}

[Serializable]
public class PayRecordInfo
{
    public long ts;
    public int status;
    public string money;
}