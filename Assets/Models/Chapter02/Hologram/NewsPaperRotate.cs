using UnityEngine;
using DG.Tweening;

public class NewsPaperRotate : MonoBehaviour
{
    [Header("Newspaper Pages")]
    [SerializeField] private Transform leftPage;
    [SerializeField] private Transform rightPage;
    
    [Header("Rotation Settings")]
    [SerializeField] private float leftTargetAngle = -80f;
    [SerializeField] private float rightTargetAngle = 80f;
    [SerializeField] private float openDuration = 1f;
    [SerializeField] private Ease easeType = Ease.OutQuad;
    
    /// <summary>
    /// 신문을 펼칩니다
    /// </summary>
    public void OpenNewspaper()
    {
        if (leftPage != null)
        {
            DOTween.Kill(leftPage);
            RotatePage(leftPage, leftTargetAngle);
        }
        
        if (rightPage != null)
        {
            DOTween.Kill(rightPage);
            RotatePage(rightPage, rightTargetAngle);
        }
    }
    
    /// <summary>
    /// 페이지를 목표 각도로 회전
    /// </summary>
    private void RotatePage(Transform page, float targetAngle)
    {
        Vector3 currentRotation = page.localEulerAngles;
        Vector3 targetRotation = new Vector3(currentRotation.x, targetAngle, currentRotation.z);
        
        page.DOLocalRotate(targetRotation, openDuration)
            .SetEase(easeType);
    }
}
