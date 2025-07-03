using System.Collections.Generic;
using System;
using UnityEngine;

namespace Framework
{
    public class AudioManager : SingletonManager<AudioManager>, IModuleInterface
    {
        private AudioSource m_musicSource = null;

        private Queue<AudioSource> m_audioSourcePool = new Queue<AudioSource>();
        private List<AudioSource> m_audioPlayerPool = new List<AudioSource>();

        private bool m_isPauseMusic = false;
        public bool IsPauseMusic { get { return m_isPauseMusic; } }

        private bool m_isPauseSound = false;
        public bool IsPauseSound { get { return m_isPauseSound; } }

        public void Init(Action<bool> onInitEnd)
        {
            InitAudioSources();
            onInitEnd?.Invoke(true);
        }

        public void Run(Action<bool> onRunEnd)
        {
            onRunEnd?.Invoke(true);
        }

        void OnDestroy()
        {
            StopMusic();
            StopAllCoroutines();
            m_audioSourcePool.Clear();
            m_audioPlayerPool.Clear();
        }

        private void InitAudioSources()
        {
            // Initialize background music
            GameObject go = new GameObject("MusicSource");
            go.transform.SetParent(transform);
            m_musicSource = go.AddComponent<AudioSource>();

            // // Initialize sound effects
            // for (int i = 0; i < Global.G_INIT_AUDIOCLIP_COUNT; i++)
            // {
            //     go = new GameObject("AudioSource");
            //     go.transform.SetParent(transform);
            //     AudioSource source = go.AddComponent<AudioSource>();
            //     m_audioSourcePool.Enqueue(source);
            // }
        }

        // Play music
        public void PlayMusic(string musicName, bool isLoop = true, float volume = 1.0f)
        {
            if (string.IsNullOrEmpty(musicName)) return;
            if (m_isPauseMusic) return;

            ResourceManager.Instance.LoadAudioClipCallback("Audios/" + musicName, (AudioClip clip) =>
            {
                if (clip == null)
                {
                    Debug.LogError("PlayMusic Error: " + musicName);
                    return;
                }

                Debug.Log("PlayMusic: " + musicName);

                if (m_musicSource.isPlaying)
                {
                    m_musicSource.Stop();
                }

                m_musicSource.clip = clip;
                m_musicSource.loop = isLoop;
                m_musicSource.volume = volume;
                m_musicSource.Play();
            });

        }

        // Stop music
        public void StopMusic()
        {
            if (m_musicSource.isPlaying)
            {
                m_musicSource.Stop();
            }
        }

        // Set music volume
        public void SetMusicVolume(float volume)
        {
            if (m_musicSource)
            {
                m_musicSource.volume = volume;
            }
        }

        // Play sound effect
        public void PlaySound(string soundName, bool isLoop = false, float volume = 1.0f)
        {
            if (string.IsNullOrEmpty(soundName)) return;
            if (m_isPauseSound) return;

            ResourceManager.Instance.LoadAudioClipCallback("Audios/" + soundName, (AudioClip clip) =>
            {
                if (clip == null)
                {
                    Debug.LogError("PlaySound Error: " + soundName);
                    return;
                }

                AudioSource source = GetAudioSource(soundName);
                source.clip = clip;
                source.loop = isLoop;
                source.volume = volume;
                source.Play();
            });
        }

        // Stop sound effect
        public void StopSound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                return;
            }

            AudioSource source = transform.Find(soundName).GetComponent<AudioSource>();
            if (source && source.isPlaying)
            {
                source.Stop();
                m_audioPlayerPool.Remove(source);
                m_audioSourcePool.Enqueue(source);
            }
        }

        private AudioSource GetAudioSource(string soundName)
        {
            AudioSource source = null;
            if (m_audioSourcePool.Count > 0)
            {
                source = m_audioSourcePool.Dequeue();
            }
            else
            {
                GameObject go = new GameObject(soundName);
                go.transform.SetParent(m_musicSource.transform);
                source = go.AddComponent<AudioSource>();
            }

            m_audioPlayerPool.Add(source);
            return source;
        }

        void Update()
        {
            // Check if sound effect playback is complete
            if (m_isPauseSound || m_audioPlayerPool.Count <= 0)
            {
                return;
            }

            for (int i = m_audioPlayerPool.Count - 1; i >= 0; i--)
            {
                if (!m_audioPlayerPool[i].isPlaying)
                {
                    m_audioSourcePool.Enqueue(m_audioPlayerPool[i]);
                    m_audioPlayerPool.RemoveAt(i);
                }
            }
        }

        public bool PauseMusic()
        {
            m_isPauseMusic = !m_isPauseMusic;

            if (m_isPauseMusic)
                m_musicSource?.Pause();
            else
                m_musicSource?.Play();

            return m_isPauseMusic;
        }

        public bool PauseSound()
        {
            m_isPauseSound = !m_isPauseSound;

            if (m_isPauseSound)
            {
                for (int i = m_audioPlayerPool.Count - 1; i >= 0; i--)
                {
                    m_audioPlayerPool[i].Pause();
                }
            }
            else
            {
                for (int i = m_audioPlayerPool.Count - 1; i >= 0; i--)
                {
                    m_audioPlayerPool[i].Play();
                }
            }

            return m_isPauseSound;
        }


    }
}