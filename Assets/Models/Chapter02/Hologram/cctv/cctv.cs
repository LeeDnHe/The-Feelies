using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class cctv : MonoBehaviour
{
    public List<GameObject> cctvImages;
    public float appearDelay = 0.3f;

    [Header("사운드 설정")]
    public AudioClip popSound; 
    private AudioSource audioSource; 
    void Awake()
    {
      
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnEnable()
    {
        StopAllCoroutines();
        foreach (var img in cctvImages)
        {
            if (img != null) img.SetActive(false);
        }
        StartCoroutine(AppearSequence());
    }

    IEnumerator AppearSequence()
    {
        foreach (var img in cctvImages)
        {
            if (img != null)
            {
                img.SetActive(true);

               
                if (popSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(popSound);
                }

                yield return new WaitForSeconds(appearDelay);
            }
        }
    }
}