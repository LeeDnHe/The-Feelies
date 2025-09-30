using UnityEngine;
using DG.Tweening;

public class SkyboxController : MonoBehaviour
{
    /// <summary>
    /// 스카이박스의 Exposure 값을 현재값에서 1로 5초 동안 변경
    /// </summary>
    public void SetExposureToOne()
    {
        Material skyboxMaterial = RenderSettings.skybox;
        if (skyboxMaterial == null)
        {
            Debug.LogWarning("Skybox material is not set!");
            return;
        }
        
        // 현재 Exposure 값 가져오기
        float currentExposure = skyboxMaterial.GetFloat("_Exposure");
        
        // DOTween으로 5초 동안 1로 변경
        DOTween.To(() => currentExposure, x => {
            skyboxMaterial.SetFloat("_Exposure", x);
        }, 1f, 5f)
        .SetEase(Ease.OutQuad);
        
        Debug.Log($"Skybox exposure changing from {currentExposure} to 1 over 5 seconds");
    }
}
