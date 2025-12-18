using System;
using TheFeelies.Core; // PlayerDotweenMover 참조

namespace TheFeelies.Events
{
    /// <summary>
    /// 플레이어 이동 관련 이벤트를 정의하는 순수 C# 클래스 (옵저버 패턴)
    /// PlayerManager에서 분리되어 독립적으로 관리됨
    /// </summary>
    public class PlayerMovementEvents
    {
        // PlayerDotweenMover가 시작될 때 호출되는 이벤트
        public event Action<PlayerDotweenMover> OnDotweenMoveStarted;

        /// <summary>
        /// PlayerDotweenMover가 시작되었음을 알림
        /// </summary>
        public void NotifyDotweenMoveStarted(PlayerDotweenMover mover)
        {
            OnDotweenMoveStarted?.Invoke(mover);
        }
    }
}
