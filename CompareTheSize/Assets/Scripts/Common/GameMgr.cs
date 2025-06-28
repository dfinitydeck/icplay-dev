using System.Collections.Generic;
using Framework;
using Framework.UIFrame;
using UI;
using UnityEngine;

/// <summary>
/// Game Manager
/// </summary>
public class GameMgr : Singleton<GameMgr>
{
    // 54 cards
    private List<PokerInfo> m_Pokers = new List<PokerInfo>();
    private int m_SwapCount = 0;
    private int m_CurScore = 0;
    private PokerInfo m_CurPoker;
    private UI_Start m_StartUI;

    private GameData m_GameData;

    public void Init(UI_Start uiStart)
    {
        m_GameData = new GameData();
    //    PayConfigMgr.Instance.Init();
        Application.targetFrameRate = 30;
        m_StartUI = uiStart;
        m_SwapCount = Constants.MAX_SWAP_COUNT;
        InitPokers();
        Shuffle(m_Pokers);
    }

    /// <summary>
    /// Get next poker card
    /// </summary>
    /// <returns></returns>
    public PokerInfo GetPoker()
    {
        if (m_Pokers.Count == 0)
            return null;
        m_CurPoker = m_Pokers[0];
        m_Pokers.RemoveAt(0);
        return m_CurPoker;
    }

    public void Return()
    {
        m_StartUI.Reset();
    }

    public UI_Start GetUIStart()
    {
        return m_StartUI;
    }
    /// <summary>
    /// Initialize poker cards
    /// </summary>
    private void InitPokers()
    {
        m_Pokers.Clear();
        for (int i = 0; i < 13; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                var poker = new PokerInfo
                {
                    Size = i + 1
                };
                switch (j)
                {
                    case 0:
                        poker.pType = PokerType.Hearts;
                        break;
                    case 1:
                        poker.pType = PokerType.Spades;
                        break;
                    case 2:
                        poker.pType = PokerType.Clubs;
                        break;
                    case 3:
                        poker.pType = PokerType.Diamonds;
                        break;
                }

                m_Pokers.Add(poker);
            }
        }

        // Jokers are 14 and 15
        var poker01 = new PokerInfo
        {
            Size = 14,
            pType = PokerType.None
        };
        var poker02 = new PokerInfo
        {
            Size = 15,
            pType = PokerType.None
        };
        m_Pokers.Add(poker01);
        m_Pokers.Add(poker02);
    }


    /// <summary>
    /// Shuffle
    /// </summary>
    private void Shuffle<T>(IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    /// <summary>
    /// Get remaining poker card count
    /// </summary>
    /// <returns></returns>
    public int GetResiduePokers()
    {
        return m_Pokers.Count;
    }

    /// <summary>
    /// Get current score
    /// </summary>
    public int GetCurScore()
    {
        return m_CurScore;
    }

    public void AddScore()
    {
        m_CurScore++;
    }

    /// <summary>
    /// Get current remaining shuffle count
    /// </summary>
    /// <returns></returns>
    public int GetSwapCount()
    {
        if(m_SwapCount < 0)
            m_SwapCount = 0;
        return m_SwapCount;
    }

    /// <summary>
    /// Reduce current shuffle count
    /// </summary>
    public void Swap()
    {
        if(m_CurPoker != null)
            m_Pokers.Add(m_CurPoker);
        Shuffle(m_Pokers);
        m_SwapCount--;
     
    }

    public int CompareSize(PokerInfo poker1, PokerInfo poker2)
    {
        if (poker1.Size == poker2.Size)
            return 0;
        if (poker1.Size > poker2.Size && poker2.Size != 1)
            return 1;
        if (poker1.Size < poker2.Size && poker1.Size != 1)
            return -1;
        if (poker1.Size == 1)
            return 1;
        if (poker2.Size == 1)
            return -1;
        return 0;
    }

    public void Reset()
    {
        InitPokers();
        Swap();
        m_CurScore = 0;
        m_SwapCount = Constants.MAX_SWAP_COUNT;
    }
    
    #region Game Data Related

    public void SetName(string nameStr)
    {
        m_GameData.Name = nameStr;
    }

    public GameData GetGameData()
    {
        if(m_GameData == null)
            m_GameData = new GameData();
        return m_GameData;
    }

    public string GetShowName()
    {
        var oriName = m_GameData.Name;
        if (string.IsNullOrEmpty(oriName))
            return string.Empty;
        var showName = oriName.Substring(0, 11) + "..." + oriName.Substring(oriName.Length - 10, 10);
        return showName;
    }

    #endregion
}

public class PokerInfo
{
    public int Size;
    public PokerType pType;
}

/// <summary>
/// Poker suit
/// </summary>
public enum PokerType
{
    // Hearts
    Hearts,

    // Spades
    Spades,

    // Clubs
    Clubs,

    // Diamonds
    Diamonds,

    // Empty
    None
}

public class GameData
{
    public string Name;
}