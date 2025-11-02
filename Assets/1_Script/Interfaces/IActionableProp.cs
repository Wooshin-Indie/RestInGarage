
using Garage.Actions;
using UnityEngine;

namespace Garage.Interfaces
{
	public interface IActionableProp
    {
        /// <summary>
        /// 처음 한 번 눌릴 때의 액션 처리
        /// 들고있는 Prop의 Action을 수행합니다.
        /// ex. 타이어 -> 굴림, 소화기 -> 분사
        /// </summary>
		void OnStartPropAction(Transform controller);

        /// <summary>
        /// 여러 프레임동안 눌려있을 때의 액션 처리
        /// </summary>
        void OnHoldingPropAction(Transform controller);

        /// <summary>
        /// 키를 뗄 때의 액션 처리
        /// </summary>
        void OnReleasedPropAction(Transform controller);

        /// <summary>
        /// 플레이어 컨트롤러가 저장할 액션 객체 전달
        /// </summary>
        PropAction GetPropAction();
	}
}
