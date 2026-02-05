using UnityEngine;
using TheFeelies.Managers;

public class CameraGazeHandler : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera gazeCamera;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask layerMask = ~0;
    [SerializeField] private QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Gaze")]
    [SerializeField] private float requiredGazeSeconds = 2f;

    private GameObject _currentTarget;
    private float _currentGazeTime;
    private bool _firedForCurrentTarget;

    private void Update()
    {
        if (gazeCamera == null)
            return;

        var ray = gazeCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out var hit, maxDistance, layerMask, queryTriggerInteraction))
        {
            var target = hit.collider != null ? hit.collider.gameObject : null;
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
            else
            {
                _currentGazeTime += Time.deltaTime;
                if (!_firedForCurrentTarget && _currentGazeTime >= requiredGazeSeconds)
                {
                    _firedForCurrentTarget = true;
                    var pm = PlayerManager.Instance;
                    pm?.GazeEvents?.NotifyGazeDetected(_currentTarget, hit);
                }
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
