using UnityEngine;
using UnityEngine.Events;
using TheFeelies.Managers;

public class GazeReceiver : MonoBehaviour
{
    [Header("Filter")]
    [SerializeField, Layer] private int targetLayer = 0;

    [Header("Events")]
    [SerializeField] private UnityEvent onGazeLayerMatched;

    private bool _isSubscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        // PlayerManager가 OnEnable 시점에 아직 없을 수도 있어서 한 번 더 시도
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (_isSubscribed)
            return;

        var pm = PlayerManager.Instance;
        if (pm == null || pm.GazeEvents == null)
            return;

        pm.GazeEvents.OnGazeDetected += HandleGazeDetected;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed)
            return;

        var pm = PlayerManager.Instance;
        if (pm != null && pm.GazeEvents != null)
            pm.GazeEvents.OnGazeDetected -= HandleGazeDetected;

        _isSubscribed = false;
    }

    private void HandleGazeDetected(GameObject target, RaycastHit hit)
    {
        if (target == null)
            return;

        if (target.layer != targetLayer)
            return;

        onGazeLayerMatched?.Invoke();
    }
}
