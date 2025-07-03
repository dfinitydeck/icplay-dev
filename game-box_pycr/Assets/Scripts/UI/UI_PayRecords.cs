using System;
using Framework.UIFrame;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UI_PayRecords:UIWindow
    {
        [SerializeField] private GameObject m_RecordItemGo;
        [SerializeField] private Button m_CloseBtn;
        [SerializeField] private RectTransform m_LayoutTrans;
        [SerializeField] private GameObject m_NoRecordGo;
        public override void OnShow(object param)
        {
            m_CloseBtn.onClick.AddListener(OnCloseBtnClick);
            RefreshUI();
        }

        private void OnCloseBtnClick()
        {
            UIManager.Instance.Hide(UIList.UI_PayRecord);
        }

        private void RefreshUI()
        {
            var payRecords = DataModule.Instance.GetPayRecordInfos();
            if (payRecords == null || payRecords.Count == 0)
            {
                m_NoRecordGo.SetActive(true);
                m_LayoutTrans.gameObject.SetActive(false);
                return;
            }
            m_NoRecordGo.SetActive(false);
            m_LayoutTrans.gameObject.SetActive(true);
            for (int i = 0; i < payRecords.Count; i++)
            {
                var itemGo = GameObject.Instantiate(m_RecordItemGo, m_LayoutTrans,false);
                var item = itemGo.GetComponent<RecordItem>();
                item.SetIdx(i);
            }
        }

        public override void OnHide()
        {
            m_CloseBtn.onClick.RemoveAllListeners();
        }
    }
}