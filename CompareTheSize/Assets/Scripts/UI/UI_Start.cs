using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Best.HTTP.JSON.LitJson;
using DG.Tweening;
using Framework.UIFrame;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UI
{
    /// <summary>
    /// Login UI
    /// </summary>
    public class UI_Start:UIWindow
    {
        [SerializeField] private Button m_StartBtn;
        [SerializeField] private Button m_LoginBtn;
        [SerializeField] private Button m_QuitBtn;
        [SerializeField] private RectTransform m_LoadingTrans;
        [SerializeField] private GameObject m_LoadingTipsGo;
        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private CanvasGroup m_StartBtnCanvasGroup;
        [SerializeField] private CanvasGroup m_LoginBtnCanvasGroup;
        [SerializeField] private Button m_SettingBtn;
        [SerializeField] private Button m_RankBtn;
        [SerializeField] private Button m_PayBtn;
        [SerializeField] private Image m_DesImg;
        [SerializeField] private TMP_Text m_NameTxt;

        [SerializeField] private GameObject m_TipsRoot;
        [SerializeField] private GameObject m_PayTips01Go;
        [SerializeField] private GameObject m_StartTipsGo;
        [SerializeField] private Button m_TipMaskBtn;
        [SerializeField] private Button m_LoginBtn02;
        [SerializeField] private Button m_PayBtn02;
        
        private Image m_LoadingImg;
        private Timer m_AliveTimer;
       

       private void Awake()
       {
           m_QuitBtn.gameObject.SetActive(false);
           m_StartBtn.onClick.AddListener(OnStartBtnClick);
           m_LoginBtn.onClick.AddListener(OnLoginBtnClick);
           m_LoginBtn02.onClick.AddListener(OnLoginBtnClick);
           m_QuitBtn.onClick.AddListener(OnQuitBtnClick);
           m_LoadingImg = m_LoadingTrans.GetComponent<Image>();
           m_LoadingTrans.gameObject.SetActive(false);
           m_SettingBtn.onClick.AddListener(OnSettingBtnClick);
           m_PayBtn.onClick.AddListener(OnPayBtnClick);
           m_PayBtn02.onClick.AddListener(OnPayBtnClick);
           m_RankBtn.onClick.AddListener(OnRankBtnClick);
           m_TipMaskBtn.onClick.AddListener(OnTipMaskBtnClick);
           EventMgr.Instance.AddListener(EventKey.LoginSuccess, OnLoginSuccess);
           EventMgr.Instance.AddListener(EventKey.LogoutSuccess,OnLogoutSuccess);
           EventMgr.Instance.AddListener(EventKey.PlayerDataUpdate,OnPalyerDataUpdate);
           m_DesImg.enabled = true;
           m_StartBtn.gameObject.SetActive(false);
           m_LoadingTipsGo.SetActive(false);
           OnTipMaskBtnClick();
       }

       private void OnPalyerDataUpdate(object obj)
       {
           var nameStr = GameMgr.Instance.GetShowName();
           m_NameTxt.text = nameStr + $"\n Tickets {DataModule.Instance.GetPlayerData()?.count:0}";
       }

       private void OnQuitBtnClick()
       {
        #if UNITY_WEBGL && !UNITY_EDITOR
           JsPlugin.Logout();
        #endif
       }

       private void OnLogoutSuccess(object obj)
       {
           TimeMgr.ClearInterval(m_AliveTimer);
           m_LoginBtn.gameObject.SetActive(true);
           m_LoginBtnCanvasGroup.alpha = 1;
           m_StartBtn.gameObject.SetActive(false);
           m_LoadingImg.enabled = false;
           m_QuitBtn.gameObject.SetActive(false);
           m_DesImg.enabled = true;
           m_NameTxt.text = string.Empty;
       }

       private void OnLoginBtnClick()
       {
           // var str =
           //     @"[{""money"":0,""status"":0},{""ts"":1749569410890,""money"":0,""status"":1},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""ts"":1749916012180,""money"":1.0e-8,""status"":1},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""ts"":1749917392425,""money"":1.0e-8,""status"":1},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""ts"":1749572761277,""money"":1.0e-8,""status"":1},{""money"":0,""status"":0},{""ts"":1749570054444,""money"":1.0e-8,""status"":1},{""money"":0,""status"":0},{""money"":0,""status"":0},{""ts"":1749566365380,""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0},{""ts"":1749568448109,""money"":0,""status"":0},{""money"":0,""status"":0},{""money"":0,""status"":0}]";
           //
           // var list = JsonMapper.ToObject<List<PayRecordInfo>>(str);
           // return;
           
#if UNITY_WEBGL && !UNITY_EDITOR
            JsPlugin.Login();
#endif
           UIManager.Instance.PlayClickAudio();
           m_StartBtn.gameObject.SetActive(false);
           m_LoadingTipsGo.gameObject.SetActive(true);
           m_LoginBtnCanvasGroup.DOFade(0, 0.1f).OnComplete(() =>
           {
               m_LoginBtn.gameObject.SetActive(false);
               m_LoadingTrans.gameObject.SetActive(true);
           });
           m_LoadingImg.enabled = true;
           m_DesImg.enabled = false;
       }

       private Timer m_Timer;
       private void OnStartBtnClick()
       {
           UIManager.Instance.PlayClickAudio();
           m_LoadingTipsGo.gameObject.SetActive(false);
           var playerData = DataModule.Instance.GetPlayerData();
           var leftCount = playerData.count;
           if (leftCount <= 0)
           {
               m_TipsRoot.SetActive(true);
               m_PayTips01Go.SetActive(false);
               m_StartTipsGo.SetActive(true);
               return;
           }
           
           m_StartBtnCanvasGroup.DOFade(0, 0.1f).OnComplete(() =>
           {
               m_StartBtn.gameObject.SetActive(false);
               m_LoadingTrans.gameObject.SetActive(true);
           });
           m_LoadingImg.enabled = true;
           m_Timer = TimeMgr.SetTimeout(LoginTimeout, 300);
       
           DataModule.Instance.MinusGameCount();
           UIManager.Instance.Show(UIList.UI_Main);
       }
       
       private void LoginTimeout()
       {
           var showName = GameMgr.Instance.GetShowName();
           if (string.IsNullOrEmpty(showName))
           {
                m_LoadingImg.enabled = false;
                m_StartBtn.gameObject.SetActive(true);
                m_StartBtnCanvasGroup.alpha = 1;
           }
       }

       private void OnPayBtnClick()
       {
           UIManager.Instance.PlayClickAudio();
           var gameData = GameMgr.Instance.GetGameData();
           var nameStr = gameData.Name;
           // Currently not logged in
           if (string.IsNullOrEmpty(nameStr))
           {
               m_TipsRoot.SetActive(true);
               m_PayTips01Go.SetActive(true);
               m_StartTipsGo.SetActive(false);
               return;
           }
           var payCfgs = DataModule.Instance.GetPayCfgs();
           if (payCfgs == null || payCfgs.Count == 0)
               return;
           UIManager.Instance.Show(UIList.UI_Pay);
       }

       private void OnTipMaskBtnClick()
       {
           UIManager.Instance.PlayClickAudio();
           m_TipsRoot.SetActive(false);
           m_PayTips01Go.SetActive(false);
           m_StartTipsGo.SetActive(false);
       }
       
       private void OnRankBtnClick()
       {
           UIManager.Instance.PlayClickAudio();
           var gameData = GameMgr.Instance.GetGameData();
           if (gameData == null || string.IsNullOrEmpty(gameData.Name))
               return;
           DataModule.Instance.ReqRankData(100);
       }
       
       private void OnSettingBtnClick()
       {
           UIManager.Instance.PlayClickAudio();
           UIManager.Instance.Show(UIList.UI_Setting,0);
       }

       public void Reset()
       {
           m_StartBtnCanvasGroup.alpha = 1;
           m_LoadingTrans.gameObject.SetActive(false);
           m_StartBtn.gameObject.SetActive(true);
           
       }
       
       private void OnLoginSuccess(object obj)
       {
           m_LoadingTipsGo.gameObject.SetActive(false);
           var nameStr = GameMgr.Instance.GetShowName();
           m_DesImg.enabled = false;
           m_QuitBtn.gameObject.SetActive(true);
           m_NameTxt.text = nameStr + $"\n Tickets {DataModule.Instance.GetPlayerData()?.count:0}";
           m_LoginBtn.gameObject.SetActive(false);
           m_StartBtn.gameObject.SetActive(true);
           m_LoadingImg.enabled = false;
           m_StartBtnCanvasGroup.DOFade(1, 0.5f);
           TimeMgr.SetInterval(DataModule.Instance.Alive, 3.0f);
       }

       private void OnDestroy()
       {
           m_StartBtn.onClick.RemoveAllListeners();
           m_SettingBtn.onClick.RemoveAllListeners();
           m_RankBtn.onClick.RemoveAllListeners();
           m_PayBtn.onClick.RemoveAllListeners();
           m_TipMaskBtn.onClick.RemoveAllListeners();
           m_QuitBtn.onClick.RemoveAllListeners();
           m_LoginBtn02.onClick.RemoveAllListeners();
           EventMgr.Instance.RemoveListener(EventKey.LoginSuccess, OnLoginSuccess);
           EventMgr.Instance.RemoveListener(EventKey.LogoutSuccess, OnLogoutSuccess);
           EventMgr.Instance.RemoveListener(EventKey.PlayerDataUpdate,OnPalyerDataUpdate);
       }

       public override void OnShow(object param)
        {
            
        }

        public override void OnHide()
        {
            
        }
    }
}