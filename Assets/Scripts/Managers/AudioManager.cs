using UnityEngine;

namespace TheFeelies.Managers
{
    /// <summary>
    /// 오디오 관리를 담당하는 매니저
    /// CutEvent에서 호출될 수 있는 메서드들을 제공
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource backgroundMusicSource;
        [SerializeField] private AudioSource sfxSource;
        
        [Header("Current Audio")]
        [SerializeField] private AudioClip currentBackgroundMusic;
        
        private static AudioManager instance;
        public static AudioManager Instance => instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // 오디오 소스가 없으면 자동 생성
            if (backgroundMusicSource == null)
            {
                backgroundMusicSource = gameObject.AddComponent<AudioSource>();
                backgroundMusicSource.loop = true;
                backgroundMusicSource.volume = 0.7f;
            }
            
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.volume = 1f;
            }
        }
        
        /// <summary>
        /// 배경음악 재생 (CutEvent에서 호출)
        /// </summary>
        public void PlayBackgroundMusic(AudioClip musicClip)
        {
            if (musicClip == null)
            {
                Debug.LogError("Music clip is null!");
                return;
            }
            
            if (backgroundMusicSource == null)
            {
                Debug.LogError("Background music source is not assigned!");
                return;
            }
            
            currentBackgroundMusic = musicClip;
            backgroundMusicSource.clip = musicClip;
            backgroundMusicSource.Play();
            
            Debug.Log($"Playing background music: {musicClip.name}");
        }
        
        /// <summary>
        /// 배경음악 변경 (CutEvent에서 호출)
        /// </summary>
        public void ChangeBackgroundMusic(AudioClip musicClip)
        {
            if (musicClip == currentBackgroundMusic)
            {
                Debug.Log("Same background music, skipping change");
                return;
            }
            
            PlayBackgroundMusic(musicClip);
        }
        
        /// <summary>
        /// 효과음 재생 (CutEvent에서 호출)
        /// </summary>
        public void PlaySFX(AudioClip sfxClip)
        {
            if (sfxClip == null)
            {
                Debug.LogError("SFX clip is null!");
                return;
            }
            
            if (sfxSource == null)
            {
                Debug.LogError("SFX source is not assigned!");
                return;
            }
            
            sfxSource.PlayOneShot(sfxClip);
            Debug.Log($"Playing SFX: {sfxClip.name}");
        }
        
        /// <summary>
        /// 배경음악 정지
        /// </summary>
        public void StopBackgroundMusic()
        {
            if (backgroundMusicSource != null)
            {
                backgroundMusicSource.Stop();
                currentBackgroundMusic = null;
                Debug.Log("Background music stopped");
            }
        }
        
        /// <summary>
        /// 모든 오디오 정지
        /// </summary>
        public void StopAllAudio()
        {
            if (backgroundMusicSource != null)
            {
                backgroundMusicSource.Stop();
            }
            
            if (sfxSource != null)
            {
                sfxSource.Stop();
            }
            
            currentBackgroundMusic = null;
            Debug.Log("All audio stopped");
        }
        
        /// <summary>
        /// 볼륨 설정
        /// </summary>
        public void SetVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            
            if (backgroundMusicSource != null)
            {
                backgroundMusicSource.volume = volume * 0.7f;
            }
            
            if (sfxSource != null)
            {
                sfxSource.volume = volume;
            }
        }
        
        /// <summary>
        /// 현재 배경음악 정보 반환
        /// </summary>
        public AudioClip GetCurrentBackgroundMusic()
        {
            return currentBackgroundMusic;
        }
    }
}
