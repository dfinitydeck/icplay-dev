using System.Collections.Generic;
using UnityEngine;

namespace Framework.UIFrame
{
    public class UIManager : Singleton<UIManager>
    {
        private RectTransform m_UIRoot;
        private Dictionary<string, UIWindow> m_UIWindows = new Dictionary<string, UIWindow>();
        private List<string> m_LoadingNames = new List<string>();

        private AudioSource m_MusicSource;
        private AudioClips m_CommonClips;

        public void SetUIRoot(RectTransform root)
        {
            m_UIRoot = root;
            m_CommonClips = m_UIRoot.Find("CommonClips").GetComponent<AudioClips>();
            m_MusicSource = m_UIRoot.GetComponent<AudioSource>();
            InitMusicState();
        }

        private void InitMusicState()
        {
            if (!PlayerPrefs.HasKey(Constants.KEY_MUSIC_DATA))
            {
                m_MusicSource.enabled = true;
                return;
            }

            var isMusicOn = PlayerPrefs.GetInt(Constants.KEY_MUSIC_DATA) == 1;
            m_MusicSource.enabled = isMusicOn;
        }

        public void SetMusicState(bool isOn)
        {
            m_MusicSource.enabled = isOn;
            var onKey = isOn ? 1 : 0;
            PlayerPrefs.SetInt(Constants.KEY_MUSIC_DATA, onKey);
        }

        public bool GetMusicState()
        {
            return m_MusicSource.enabled;
        }

        public void PlayClickAudio()
        {
            if(m_CommonClips != null)
                m_CommonClips.PlayClip();
        }

        public void Show(OpenInfo openInfo,object param = null)
        {
            // The current window is already displayed
            if (m_UIWindows.TryGetValue(openInfo.Name, out UIWindow uiWindow))
                return;
            // The current window is being displayed
            if (m_LoadingNames.Contains(openInfo.Name))
                return;

            m_LoadingNames.Add(openInfo.Name);
            ResourceManager.Instance.LoadGameObjectCallback(openInfo.AssetPath, (go) =>
            {
                if (m_LoadingNames.Contains(openInfo.Name))
                {
                    m_LoadingNames.Remove(openInfo.Name);
                }
                else
                {
                    // The current interface has already been closed before opening is completed
                    GameObject.Destroy(go);
                }

                go.transform.SetParent(m_UIRoot, false);
                var window = go.GetComponent<UIWindow>();
                m_UIWindows.Add(openInfo.Name, window);
                window.OnShow(param);
            });
        }

        public void Hide(OpenInfo openInfo)
        {
            if (m_UIWindows.TryGetValue(openInfo.Name, out UIWindow uiWindow))
            {
                uiWindow.OnHide();
                m_UIWindows.Remove(openInfo.Name);
                GameObject.Destroy(uiWindow.gameObject);
            }

            if (m_LoadingNames.Contains(openInfo.AssetPath))
                m_LoadingNames.Remove(openInfo.AssetPath);
        }

        public UIWindow GetOpenUIWindow(OpenInfo openInfo)
        {
            if (m_UIWindows.TryGetValue(openInfo.Name, out UIWindow uiWindow))
            {
                return uiWindow;
            }
            return null;
        }
    }
}