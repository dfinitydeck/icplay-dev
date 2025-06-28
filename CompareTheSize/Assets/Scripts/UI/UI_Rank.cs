using System;
using System.Collections.Generic;
using Framework.UIFrame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Rank : UIWindow
{
    [SerializeField] private GameObject m_ItemGo;
    [SerializeField] private RectTransform m_ContentTrans;
    [SerializeField] private Button m_CloseBtn;
    
    [SerializeField] private TMP_Text m_RankTxt;
    [SerializeField] private TMP_Text m_NameTxt;
    [SerializeField] private TMP_Text m_ScoreTxt;
    [SerializeField] private TMP_Text m_PriceTxt;
    
    private List<RankItem> m_Items = new List<RankItem>();
    
    public override void OnShow(object param)
    {
        m_Items.Clear();
        m_CloseBtn.onClick.AddListener(OnCloseBtnClick);
        RefreshUI();
    }

    private void OnCloseBtnClick()
    {
        UIManager.Instance.Hide(UIList.UI_Rank);
    }

    private void RefreshUI()
    {
        var rankData = DataModule.Instance.GetRankData();
        var ownerData = rankData.own;
        var playerData = DataModule.Instance.GetPlayerData();
        var gameData = GameMgr.Instance.GetGameData();
        m_NameTxt.text = GetShowName(gameData.Name);
        m_RankTxt.text = GetRankStr(ownerData.rank);
        m_ScoreTxt.text = $"{ownerData.score} pts";
        m_PriceTxt.text = $"{playerData.allMondy} (ICP)";
        if(ownerData.score == 0)
            m_ScoreTxt.text = "--";
        
        for (int i = 0; i < rankData.rank.Count; i++)
        {
            var rank = rankData.rank[i];
            var itemGo = Instantiate(m_ItemGo,m_ContentTrans,false);
            var item = itemGo.GetComponent<RankItem>();
            item.SetData(rank,i);
            m_Items.Add(item);
        }
    }
    
    public string GetShowName(string oriName)
    {
        if(string.IsNullOrEmpty(oriName) || oriName.Length <= 8)
            return oriName;
        var showName = oriName.Substring(0, 5) + "..." + oriName.Substring(oriName.Length - 3, 3);
        return showName;
    }
    
    public string GetRankStr(int rank)
    {
        if (rank == 0)
            return "--";
        if (rank < 10)
            return $"0{rank}";
        return rank.ToString();
    }

    public override void OnHide()
    {
        m_CloseBtn.onClick.RemoveAllListeners();
    }
}