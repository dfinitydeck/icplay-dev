
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.Serialization;

    public class AudioClips:MonoBehaviour
    {
        public AudioClip[] Clips;
        public AudioSource Source;

        public void PlayClip(ClipsType clipType)
        {
            var clip = Clips[(int)clipType];
            Source.clip = clip;
            Source.loop = false;
            Source.Play();
        }
        
        public void PlayClip(int clipType = 0)
        {
            var clip = Clips[clipType];
            Source.clip = clip;
            Source.loop = false;
            Source.Play();
        }
    }

    public enum ClipsType
    {
        Correct,
        Draw,
        Again,
        FlipCard,
        Guess,
        Joker,
        Shuffle_Cards,
        Swap,
        Win,
        Wrong
    }