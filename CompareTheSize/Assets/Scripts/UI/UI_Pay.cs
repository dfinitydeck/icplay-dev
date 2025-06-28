using System;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using Framework;
using Framework.UIFrame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Payment UI
    /// </summary>
    public class UI_Pay : UIWindow
    {
        [SerializeField] private Button m_PayBtn01;
        [SerializeField] private Button m_PayBtn02;
        [SerializeField] private Button m_PayBtn03;
        [SerializeField] private TMP_Text m_PriceTxt01;
        [SerializeField] private TMP_Text m_PriceTxt02;
        [SerializeField] private TMP_Text m_PriceTxt03;
        [SerializeField] private TMP_Text m_CountTxt01;
        [SerializeField] private TMP_Text m_CountTxt02;
        [SerializeField] private TMP_Text m_CountTxt03;
        
        [SerializeField] private Button m_CloseBtn;
        [SerializeField] private Button m_RecordBtn;

        [SerializeField] private TMP_Text m_NameTxt;
        [SerializeField] private TMP_Text m_BalanceTxt;
        [SerializeField] private TMP_Text m_TicketsTxt;

        [SerializeField] private GameObject m_TipsRootGo;
        [SerializeField] private Button m_TipMaskBtn;
        [SerializeField] private GameObject m_PayTipsGo;
        [SerializeField] private GameObject m_PayNotEnoughTipsGo;
        [SerializeField] private GameObject m_PayConfirmTipsGo;
        [SerializeField] private Button m_PayTipsOKBtn;
        [SerializeField] private Button m_PayConfirmTipsOKBtn;
        [SerializeField] private Button m_PayNotEnoughTipsOKBtn;
        [SerializeField] private TMP_Text m_PayConfirmDesTxt;

        [SerializeField] private Button m_RefreshBtn;
        [SerializeField] private Button m_CopyBtn;
        [SerializeField] private TMP_Text m_TipsTxt;

        private void Awake()
        {
            EventMgr.Instance.AddListener(EventKey.PayResult,OnPayResult);
            EventMgr.Instance.AddListener(EventKey.PlayerDataUpdate, OnPlayerDataUpdate);
            EventMgr.Instance.AddListener(EventKey.PayResultRefresh,OnPlayerDataUpdate);
            m_PayTipsOKBtn.onClick.AddListener(OnPayTipsOKBtnClick);
            m_PayConfirmTipsOKBtn.onClick.AddListener(OnPayConfirmTipsOKBtnClick);
            m_PayNotEnoughTipsOKBtn.onClick.AddListener(OnPayNotEnoughTipsOKBtnClick);
            m_RefreshBtn.onClick.AddListener(OnRefreshBtnClick);
            m_CopyBtn.onClick.AddListener(OnCopyBtnClick);
        }

        private void OnCopyBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            var pri = GameMgr.Instance.GetGameData().Name;
            var ptr = JsPlugin.GetIntPtr(pri);
            JsPlugin.Copy(ptr);
            ShowTips("Copied");
        }

        private void OnRefreshBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            var nameStr = GameMgr.Instance.GetGameData().Name;
            IntPtr ptr = JsPlugin.GetIntPtr(nameStr);
            JsPlugin.GetBalance(ptr);
            ShowTips("Refreshed");
        }

        private void ShowTips(string tipsStr)
        {
            m_TipsTxt.text = tipsStr;
            m_TipsTxt.alpha = 0;
            m_TipsTxt.DOFade(1, 0.5f).OnComplete(() =>
            {
                m_TipsTxt.DOFade(0, 0.5f);
            });
            
        }
        private void OnPayNotEnoughTipsOKBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            m_TipsRootGo.gameObject.SetActive(false);
        }

        private void OnPayConfirmTipsOKBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            m_TipsRootGo.gameObject.SetActive(false);
            Buy(m_PayId);
        }

        private void OnPayTipsOKBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            m_TipsRootGo.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            EventMgr.Instance.RemoveListener(EventKey.PayResult,OnPayResult);
            EventMgr.Instance.RemoveListener(EventKey.PlayerDataUpdate, OnPlayerDataUpdate);
            EventMgr.Instance.RemoveListener(EventKey.PayResultRefresh, OnPlayerDataUpdate);
        }

        private void OnPlayerDataUpdate(object obj)
        {
            m_TipsRootGo.SetActive(false);
            RefreshUI();
        }

        private void OnPayResult(object obj)
        {
            bool isSuccess = (bool)obj;
            if(isSuccess)
                DataModule.Instance.StartGetPlayerData();
        }

        public override void OnShow(object param)
        {
            m_CloseBtn.onClick.AddListener(OnCloseBtnClick);
            m_PayBtn01.onClick.AddListener(OnPayBtn1Click);
            m_PayBtn02.onClick.AddListener(OnPayBtn2Click);
            m_PayBtn03.onClick.AddListener(OnPayBtn3Click);
            m_TipMaskBtn.onClick.AddListener(OnTipMaskBtnClick);
            m_RecordBtn.onClick.AddListener(OnRecordBtnClick);
            m_TipsRootGo.SetActive(false);
            RefreshUI();
        }

        private void OnRecordBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            DataModule.Instance.ReqPayRecord();
        }

        private void OnTipMaskBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            m_TipsRootGo.SetActive(false);
        }

        private void RefreshUI()
        {
            var playerData = DataModule.Instance.GetPlayerData();
            m_NameTxt.text = GameMgr.Instance.GetShowName();
            m_BalanceTxt.text = $"Balance  {DataModule.Instance.GetBalance()}(ICP)";
            m_TicketsTxt.text = $"Tickets  {playerData.count}";
            
            var payCfgs = DataModule.Instance.GetPayCfgs();
          
            if (payCfgs.Count < 3)
            {
                Log.Error($"Recharge configuration error, number of recharge options is not 3");
                return;
            }

            m_PriceTxt01.text = $"{payCfgs[0].money} ICP";
            m_PriceTxt02.text = $"{payCfgs[1].money} ICP";
            m_PriceTxt03.text = $"{payCfgs[2].money} ICP";
            
            m_CountTxt01.text = $"{payCfgs[0].count} tickets";
            m_CountTxt02.text = $"{payCfgs[1].count} tickets";
            m_CountTxt03.text = $"{payCfgs[2].count} tickets";
        }

        private string m_PayId;
        private void OnPayBtn1Click()
        {
            UIManager.Instance.PlayClickAudio();
             var payCfgs = DataModule.Instance.GetPayCfgs();
             var pay = payCfgs[0];
             var payId = pay.id.ToString();
             m_TipsRootGo.SetActive(true);
             m_PayId = payId;
             if (IsEnough(pay.money))
             {
                 m_PayConfirmTipsGo.gameObject.SetActive(true);
                 m_PayNotEnoughTipsGo.gameObject.SetActive(false);
                 m_PayTipsGo.gameObject.SetActive(false);
                 m_PayConfirmDesTxt.text = $"Start to purchase \n{pay.count} tickets\n\nAre you Sure?";
             }
             else
             {
                 m_PayConfirmTipsGo.gameObject.SetActive(false);
                 m_PayNotEnoughTipsGo.gameObject.SetActive(true);
                 m_PayTipsGo.gameObject.SetActive(false);
             }
        }
        private void OnPayBtn2Click()
        {
            UIManager.Instance.PlayClickAudio();
            var payCfgs = DataModule.Instance.GetPayCfgs();
            var pay = payCfgs[1];
            var payId = pay.id.ToString();
            m_PayId = payId;
            m_TipsRootGo.SetActive(true);
            if (IsEnough(pay.money))
            {
                m_PayConfirmTipsGo.gameObject.SetActive(true);
                m_PayNotEnoughTipsGo.gameObject.SetActive(false);
                m_PayTipsGo.gameObject.SetActive(false);
                m_PayConfirmDesTxt.text = $"Start to purchase \n{pay.count} tickets\n\nAre you Sure?";
            }
            else
            {
                m_PayConfirmTipsGo.gameObject.SetActive(false);
                m_PayNotEnoughTipsGo.gameObject.SetActive(true);
                m_PayTipsGo.gameObject.SetActive(false);
            }
        }
        private void OnPayBtn3Click()
        {
            UIManager.Instance.PlayClickAudio();
            var payCfgs = DataModule.Instance.GetPayCfgs();
            var pay = payCfgs[2];
            var payId = pay.id.ToString();
            m_PayId = payId;
            m_TipsRootGo.SetActive(true);
            if (IsEnough(pay.money))
            {
                m_PayConfirmTipsGo.gameObject.SetActive(true);
                m_PayNotEnoughTipsGo.gameObject.SetActive(false);
                m_PayTipsGo.gameObject.SetActive(false);
                m_PayConfirmDesTxt.text = $"Start to purchase \n{pay.count} tickets\n\nAre you Sure?";
            }
            else
            {
                m_PayConfirmTipsGo.gameObject.SetActive(false);
                m_PayNotEnoughTipsGo.gameObject.SetActive(true);
                m_PayTipsGo.gameObject.SetActive(false);
            }
        }
        
        private void Buy(string packId)
        {
            m_TipsRootGo.SetActive(true);
            m_PayConfirmTipsGo.gameObject.SetActive(false);
            m_PayNotEnoughTipsGo.gameObject.SetActive(false);
            m_PayTipsGo.gameObject.SetActive(true);
#if UNITY_WEBGL && !UNITY_EDITOR
             IntPtr ptr = JsPlugin.GetIntPtr(packId);
             JsPlugin.Instance.BuyAsync(ptr);
#endif
        }

        private void OnCloseBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            UIManager.Instance.Hide(UIList.UI_Pay);
        }

        private bool IsEnough(decimal price)
        {
            var balance = DataModule.Instance.GetBalance();
            if (decimal.Parse(balance) >= price)
                return true;
            return false;
        }

        public override void OnHide()
        {
            m_PayBtn03?.onClick.RemoveAllListeners();
            m_PayBtn02?.onClick.RemoveAllListeners();
            m_PayBtn01?.onClick.RemoveAllListeners();
            m_CloseBtn?.onClick.RemoveAllListeners();
            m_TipMaskBtn?.onClick.RemoveAllListeners();
            m_RecordBtn?.onClick.RemoveAllListeners();
        }
    }
}