using System;
using UnityEngine;

namespace TheFeelies.Events
{
    /// <summary>
    /// 카메라 시선(레이캐스트) 감지 관련 이벤트를 정의하는 순수 C# 클래스 (옵저버 패턴)
    /// 구독자는 PlayerManager.Instance.GazeEvents 를 통해 이벤트를 구독합니다.
    /// </summary>
    public class CameraGazeEvents
    {
        /// <summary>
        /// 같은 물체를 특정 시간 이상 바라보아 "감지"되었을 때 1회 호출
        /// </summary>
        public event Action<GameObject, RaycastHit> OnGazeDetected;

        public void NotifyGazeDetected(GameObject target, RaycastHit hit)
        {
            OnGazeDetected?.Invoke(target, hit);
        }
    }
}

