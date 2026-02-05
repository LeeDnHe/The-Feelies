using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class SkyboxController : MonoBehaviour
{
    private Material skyboxInstance; // 스카이박스 복사본을 저장할 변수
    private bool initialized;

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        // 이미 내 씬이 활성화된 상태면 바로 시도
        TryInitialize();
    }

    void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    void Start()
    {
        // Additive 로딩에서는 Start가 "ActiveScene 전환/Lighting 반영" 전에 호출될 수 있어
        // 한 프레임 뒤에 한 번 더 시도(보험)
        StartCoroutine(TryInitializeNextFrame());
    }

    private System.Collections.IEnumerator TryInitializeNextFrame()
    {
        yield return null;
        TryInitialize();
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        // 내가 속한 씬이 활성화되는 순간에 초기화
        if (newScene == gameObject.scene)
        {
            TryInitialize();
        }
    }

    private void TryInitialize()
    {
        if (initialized || skyboxInstance != null)
            return;

        // 내 씬이 활성화되기 전에는, 이 씬의 Lighting(RenderSettings)이 아직 적용되지 않았을 수 있음
        if (SceneManager.GetActiveScene() != gameObject.scene)
            return;

        // 1. 현재 스카이박스 머티리얼을 기반으로 새로운 복사본(인스턴스)을 만듭니다.
        if (RenderSettings.skybox != null)
        {
            skyboxInstance = new Material(RenderSettings.skybox);

            // 2. 씬의 스카이박스를 이 새로운 복사본으로 교체합니다.
            RenderSettings.skybox = skyboxInstance;

            initialized = true;
            Debug.Log($"Skybox instance created successfully (ActiveScene: {SceneManager.GetActiveScene().name})");
        }
        else
        {
            Debug.LogWarning($"Skybox material is not set! (ActiveScene: {SceneManager.GetActiveScene().name})");
        }
    }

    /// <summary>
    /// 스카이박스의 Exposure 값을 현재값에서 1로 5초 동안 변경
    /// </summary>
    public void SetExposureToOne()
    {
        if (skyboxInstance == null)
        {
            Debug.LogWarning("Skybox instance is not ready!");
            return;
        }
        
        // 현재 Exposure 값 가져오기
        float currentExposure = skyboxInstance.GetFloat("_Exposure");
        
        // 3. 이제 원본이 아닌 '복사본'의 값을 안전하게 변경합니다.
        DOTween.To(() => skyboxInstance.GetFloat("_Exposure"), 
                   x => skyboxInstance.SetFloat("_Exposure", x), 
                   1f, 
                   5f)
        .SetEase(Ease.OutQuad);
        
        Debug.Log($"Skybox exposure changing from {currentExposure} to 1 over 5 seconds");
    }

    // 씬이 파괴될 때 복사된 머티리얼을 정리하여 메모리 누수를 방지합니다.
    void OnDestroy()
    {
        if (skyboxInstance != null)
        {
            Destroy(skyboxInstance);
            Debug.Log("Skybox instance destroyed");
        }
    }
}
