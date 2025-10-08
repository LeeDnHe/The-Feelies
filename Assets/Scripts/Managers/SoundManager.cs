using UnityEngine;
using System.Collections;

namespace TheFeelies.Managers
{
    public class SoundManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource dialogSource;
        
        [Header("Audio Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.8f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float dialogVolume = 1f;
        
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
            
            if (dialogSource == null)
            {
                GameObject dialogGO = new GameObject("DialogSource");
                dialogGO.transform.SetParent(transform);
                dialogSource = dialogGO.AddComponent<AudioSource>();
            }
            
            UpdateVolumes();
        }
        
        public void PlayDialog(AudioClip dialogClip)
        {
            if (dialogClip != null)
            {
                dialogSource.clip = dialogClip;
                dialogSource.Play();
                Debug.Log($"대화 재생: {dialogClip.name}");
            }
        }
        
        public void StopDialog()
        {
            dialogSource.Stop();
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
        
        public void SetDialogVolume(float volume)
        {
            dialogVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }
        
        private void UpdateVolumes()
        {
            musicSource.volume = masterVolume * musicVolume;
            sfxSource.volume = masterVolume * sfxVolume;
            dialogSource.volume = masterVolume * dialogVolume;
        }
        
        public bool IsDialogPlaying()
        {
            return dialogSource.isPlaying;
        }
        
        public bool IsMusicPlaying()
        {
            return musicSource.isPlaying;
        }
    }
} 