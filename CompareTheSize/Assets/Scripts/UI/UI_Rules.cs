
    using Framework.UIFrame;
    using UnityEngine;
    using UnityEngine.UI;

    public class UI_Rules:UIWindow
    {
        [SerializeField] private Button m_CloseBtn;
        public override void OnShow(object param)
        {
            AddEvent();
        }

        private void AddEvent()
        {
            m_CloseBtn.onClick.AddListener(OnCloseBtnClick);
        }

        private void OnCloseBtnClick()
        {
            UIManager.Instance.PlayClickAudio();
            UIManager.Instance.Hide(UIList.UI_Rules);
        }

        
        public override void OnHide()
        {
            RemoveEvent();
        }

        private void RemoveEvent()
        {
            m_CloseBtn.onClick.RemoveListener(OnCloseBtnClick);
        }
    }