using UnityEngine;
using TheFeelies.Managers;

public class CameraGazeHandler : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera gazeCamera;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask layerMask = ~0;
    [SerializeField] private QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;
    [SerializeField] private bool useRigidbodyOrRootAsTarget = true;

    [Header("Gaze")]
    [SerializeField] private float requiredGazeSeconds = 2f;

    private GameObject _currentTarget;
    private float _currentGazeTime;
    private bool _firedForCurrentTarget;

    private void Awake()
    {
        // 안전장치: 인스펙터에서 미할당이면 Main Camera로 보정 시도
        if (gazeCamera == null)
            gazeCamera = Camera.main;
    }

    private void Update()
    {
        if (gazeCamera == null)
            return;

        var ray = gazeCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out var hit, maxDistance, layerMask, queryTriggerInteraction))
        {
            GameObject target = null;
            if (hit.collider != null)
            {
                if (useRigidbodyOrRootAsTarget)
                    target = hit.rigidbody != null ? hit.rigidbody.gameObject : hit.collider.transform.root.gameObject;
                else
                    target = hit.collider.gameObject;
            }
            if (target == null)
            {
                ResetGaze();
                return;
            }

            if (target != _currentTarget)
            {
                _currentTarget = target;
                _currentGazeTime = 0f;
                _firedForCurrentTarget = false;
            }

            // 같은 타겟을 바라보는 동안 항상 시간 누적 (첫 프레임도 포함)
            _currentGazeTime += Time.deltaTime;

            if (!_firedForCurrentTarget && _currentGazeTime >= requiredGazeSeconds)
            {
                _firedForCurrentTarget = true;
                var pm = PlayerManager.Instance;
                pm?.GazeEvents?.NotifyGazeDetected(_currentTarget, hit);
            }

            return;
        }
        ResetGaze();
    }

    private void ResetGaze()
    {
        _currentTarget = null;
        _currentGazeTime = 0f;
        _firedForCurrentTarget = false;
    }
}
