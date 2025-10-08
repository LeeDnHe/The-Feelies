using UnityEngine;
using System.Collections;

namespace TheFeelies.Managers
{
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager instance;
        public static SoundManager Instance => instance;
        
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
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
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
                // 이전 대화 재생 중지
                if (dialogSource.isPlaying)
                {
                    dialogSource.Stop();
                }
                
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
                // 이전 음악 재생 중지
                if (musicSource.isPlaying)
                {
                    musicSource.Stop();
                }
                
                musicSource.clip = musicClip;
                musicSource.Play();
                Debug.Log($"음악 재생: {musicClip.name}");
            }
        }
        
        public void StopMusic()
        {
            musicSource.Stop();
        }
        
        public void PlaySFX(AudioClip sfxClip, bool loop = false)
        {
            if (sfxClip != null)
            {
                // 이전 SFX 재생 중지
                if (sfxSource.isPlaying)
                {
                    sfxSource.Stop();
                }
                
                sfxSource.clip = sfxClip;
                sfxSource.loop = loop;
                sfxSource.Play();
                Debug.Log($"SFX 재생: {sfxClip.name} (Loop: {loop})");
            }
        }
        
        /// <summary>
        /// SFX 중지
        /// </summary>
        public void StopSFX()
        {
            if (sfxSource.isPlaying)
            {
                sfxSource.Stop();
                sfxSource.loop = false;
                Debug.Log("SFX 중지");
            }
        }
        
        /// <summary>
        /// 이전 SFX를 중지하지 않고 겹쳐서 재생 (여러 효과음 동시 재생)
        /// </summary>
        public void PlaySFXOneShot(AudioClip sfxClip)
        {
            if (sfxClip != null)
            {
                sfxSource.PlayOneShot(sfxClip);
                Debug.Log($"SFX 겹쳐서 재생: {sfxClip.name}");
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
            if (musicSource != null)
                musicSource.volume = masterVolume * musicVolume;
            if (sfxSource != null)
                sfxSource.volume = masterVolume * sfxVolume;
            if (dialogSource != null)
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
        
        public bool IsSFXPlaying()
        {
            return sfxSource.isPlaying;
        }
    }
} 