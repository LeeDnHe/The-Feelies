using UnityEngine;
using System.Collections;

namespace TheFeelies.Managers
{
    public class SoundManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ttsSource;
        
        [Header("Audio Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.8f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float ttsVolume = 1f;
        
        private void Awake()
        {
            InitializeAudioSources();
        }
        
        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                GameObject musicGO = new GameObject("MusicSource");
                musicGO.transform.SetParent(transform);
                musicSource = musicGO.AddComponent<AudioSource>();
                musicSource.loop = true;
            }
            
            if (sfxSource == null)
            {
                GameObject sfxGO = new GameObject("SFXSource");
                sfxGO.transform.SetParent(transform);
                sfxSource = sfxGO.AddComponent<AudioSource>();
            }
            
            if (ttsSource == null)
            {
                GameObject ttsGO = new GameObject("TTSSource");
                ttsGO.transform.SetParent(transform);
                ttsSource = ttsGO.AddComponent<AudioSource>();
            }
            
            UpdateVolumes();
        }
        
        public void PlayTTS(AudioClip ttsClip)
        {
            if (ttsClip != null)
            {
                ttsSource.clip = ttsClip;
                ttsSource.Play();
                Debug.Log($"TTS 재생: {ttsClip.name}");
            }
        }
        
        public void StopTTS()
        {
            ttsSource.Stop();
        }
        
        public void PlayMusic(AudioClip musicClip)
        {
            if (musicClip != null)
            {
                musicSource.clip = musicClip;
                musicSource.Play();
                Debug.Log($"음악 재생: {musicClip.name}");
            }
        }
        
        public void StopMusic()
        {
            musicSource.Stop();
        }
        
        public void PlaySFX(AudioClip sfxClip)
        {
            if (sfxClip != null)
            {
                sfxSource.PlayOneShot(sfxClip);
                Debug.Log($"SFX 재생: {sfxClip.name}");
            }
        }
        
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }
        
        public void SetTTSVolume(float volume)
        {
            ttsVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }
        
        private void UpdateVolumes()
        {
            musicSource.volume = masterVolume * musicVolume;
            sfxSource.volume = masterVolume * sfxVolume;
            ttsSource.volume = masterVolume * ttsVolume;
        }
        
        public bool IsTTSPlaying()
        {
            return ttsSource.isPlaying;
        }
        
        public bool IsMusicPlaying()
        {
            return musicSource.isPlaying;
        }
    }
} 