using UnityEngine;
using DG.Tweening;

public class SkyboxController : MonoBehaviour
{
    private Material skyboxInstance; // 스카이박스 복사본을 저장할 변수

    void Start()
    {
        // 1. 현재 스카이박스 머티리얼을 기반으로 새로운 복사본(인스턴스)을 만듭니다.
        if (RenderSettings.skybox != null)
        {
            skyboxInstance = new Material(RenderSettings.skybox);
            
            // 2. 씬의 스카이박스를 이 새로운 복사본으로 교체합니다.
            RenderSettings.skybox = skyboxInstance;
            
            Debug.Log("Skybox instance created successfully");
        }
        else
        {
            Debug.LogWarning("Skybox material is not set!");
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
