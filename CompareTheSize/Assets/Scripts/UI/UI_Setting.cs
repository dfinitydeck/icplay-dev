using Framework;
using Framework.UIFrame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UI_Setting : UIWindow
    {
        [SerializeField] private Button m_BtnClose;
        [SerializeField] private Button m_MusicOffBtn;
        [SerializeField] private Button m_MusicOnBtn;
        [SerializeField] private Button m_RuleBtn;
        [SerializeField] private Button m_MainMenuBtn;
        [SerializeField] private Button m_ResumeBtn;
        
        [SerializeField] private Image m_TitleImg01;
        [SerializeField] private Image m_TitleImg02;
        
        [SerializeField] private TMP_Text m_PIDTxt;
        [SerializeField] private TMP_Text m_PIDTitleTxt;
        [SerializeField] private Image m_BgSetting;
        [SerializeField] private Image m_BgResume;
        
        public override void OnShow(object param)
        {
            m_MusicOffBtn.onClick.AddListener(OnMusicOffBtnClick);
            m_MusicOnBtn.onClick.AddListener(OnMusicOnBtnClick);
            m_BtnClose.onClick.AddListener(OnCloseBtnClick);
            m_RuleBtn.onClick.AddListener(OnRuleBtnClick);
            m_MainMenuBtn.onClick.AddListener(OnMainMenuBtnClick);
            m_ResumeBtn.onClick.AddListener(OnResumeBtnClick);
            var type = int.Parse(param.ToString());
            m_TitleImg01.enabled = type == 0;
            m_TitleImg02.enabled = type == 1;
            m_PIDTitleTxt.gameObject.SetActive(type == 1);
            m_PIDTxt.gameObject.SetActive(type == 1);
            m_BtnClose.gameObject.SetActive(type == 0);
            m_MainMenuBtn.gameObject.SetActive(type == 1);
            m_BgResume.enabled = type == 1;
            m_BgSetting.enabled = type == 0;
            RefreshUI();
        }

        private void OnResumeBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            UIManager.Instance.Hide(UIList.UI_Setting);
        }

        private void OnMainMenuBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            UIManager.Instance.Hide(UIList.UI_Main);
            UIManager.Instance.Hide(UIList.UI_Setting);
            GameMgr.Instance.Return();
        }

        private void OnRuleBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            UIManager.Instance.Show(UIList.UI_Rules);
        }

        private void OnCloseBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            UIManager.Instance.Hide(UIList.UI_Setting);
        }

        private void OnMusicOnBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            UIManager.Instance.SetMusicState(false);
            RefreshUI();
        }


        private void OnMusicOffBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            UIManager.Instance.SetMusicState(true);
            RefreshUI();
        }

        private void RefreshUI()
        {
            var isMusic = UIManager.Instance.GetMusicState();
            m_MusicOffBtn.gameObject.SetActive(!isMusic);
            m_MusicOnBtn.gameObject.SetActive(isMusic);
            m_PIDTxt.text = GameMgr.Instance.GetShowName();
            
        }

        public override void OnHide()
        {
            m_MusicOffBtn.onClick.RemoveListener(OnMusicOffBtnClick);
            m_MusicOnBtn.onClick.RemoveListener(OnMusicOnBtnClick);
            m_BtnClose.onClick.RemoveListener(OnCloseBtnClick);
            m_RuleBtn.onClick.RemoveListener(OnRuleBtnClick);
            m_MainMenuBtn.onClick.RemoveListener(OnMainMenuBtnClick);
        }
    }
}