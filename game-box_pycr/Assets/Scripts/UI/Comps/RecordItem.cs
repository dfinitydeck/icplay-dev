
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class RecordItem:MonoBehaviour
    {
        private int m_Idx;
        [SerializeField] private TMP_Text m_TimeTxt;
        [SerializeField] private TMP_Text m_CountTxt;
        [SerializeField] private Image m_DoneImg;
        [SerializeField] private Image m_CloseImg;
        public void SetIdx(int idx)
        {
            m_Idx = idx;
            var payRecords = DataModule.Instance.GetPayRecordInfos();
            var record = payRecords[m_Idx];
            var timeStr = GetTimeStr(record.ts);
            m_TimeTxt.text = timeStr;
            m_CountTxt.text = $"{record.money} (ICP)";
            m_DoneImg.enabled = record.status == 1;
            m_CloseImg.enabled = record.status == 0;
        }

        private string GetTimeStr(long ts)
        {
            ts /= 1000;
            DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeSeconds(ts);
            string formattedTime = dateTime.ToString("HH:mm,MMMdd，yyyy");
            return formattedTime;
        }
    }