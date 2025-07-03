
    using TMPro;
    using UnityEngine;

    public class RankItem:MonoBehaviour
    {
        [SerializeField] private TMP_Text m_RankTxt;
        [SerializeField] private TMP_Text m_NameTxt;
        [SerializeField] private TMP_Text m_ScoreTxt;
        public void SetData(SingleRankData rankData,int idx)
        {
            var nameStr = GetShowName(rankData.name);
            var rankStr = GetRankStr(idx + 1);
            var scoreStr = $"{rankData.score} pts";
            m_NameTxt.text = nameStr;
            m_ScoreTxt.text = scoreStr;
            m_RankTxt.text = rankStr;
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
            if (rank < 10)
                return $"0{rank}";
            return rank.ToString();
        }
    }