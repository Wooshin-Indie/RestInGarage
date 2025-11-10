
using Garage.Interfaces;
using Garage.Manager;
using Garage.Props;
using Garage.Utils;
using UnityEngine;

namespace Garage.Controller
{
	/// <summary>
	/// Event 관련 함수들 
	/// </summary>
	public partial class PlayerController
	{

        #region Animation Events


        // 애니메이션 클립에서 Key로 호출될 함수
        private void OnActionKeyEvent()
        {
            if (currentPropAction == null) return;
            if (currentOwningProp == null) return;
            Debug.Log("Called animation event: OnActionEnd");

            currentPropAction.OnAnimationKey(transform);
            currentOwningProp.GetComponent<IActionableProp>()?.OnAnimationKeyPropAction(transform);
            isActionStarted = false;
            // OnActionEndEvent가 호출되는 유형의 액션에서 애니메이션 실행 중 액션 키 입력 시 오류 발생 가능 (액션 안에서 통제해야할라나)
        }

        private void OnStartPlace()
		{
			if (!IsOwner) return;

			Managers.Input.DisablePlayerMove();
            rigid.linearVelocity = Vector3.zero;
		}
		private void OnEndPlace() // 타이어 굴리기
		{
			if (!IsOwner) return;

			Managers.Input.EnablePlayerMove();
			if (currentOwningProp == null) return;

			if (currentOwningProp.GetComponent<IActionableProp>() != null)
			{
				currentOwningProp.GetComponent<IActionableProp>().OnReleasedPropAction(transform);
            }
			currentOwningProp = null;
		}
    
		private void OnEndThrow()
		{
			if (!IsOwner) return;

			isAbleToMove = true;
			if (currentOwningProp == null) return;

			if (currentOwningProp.GetComponent<IActionable>() != null)
			{
				currentOwningProp.GetComponent<IActionable>().OnStopPropAction(transform);
				OnTireRoll();
			}
			currentOwningProp = null;
		}

		private void OnPutTire()
		{
			if (!IsOwner) return;
			if (currentOwningProp == null) return;

			Managers.Sound.PlaySfx(SFXType.Put, 1.3f, 1f);

			currentFixablePart?.Interact(this, currentOwningProp);
			DespawnPropServerRPC(currentOwningProp.NetworkObjectId);
			currentOwningProp = null;
			Managers.Input.EnablePlayerMove();
        }


		private void OnFootstep()
		{
			// TODO - 바닥 텍스쳐에 따라 소리 다르게 하면 좋을듯?
			// 지금은 자갈 밟는 소리임
			Managers.Sound.PlaySfx(SFXType.Walk, .7f, 1f);
		}
		private void OnCrouch()
		{
			Managers.Sound.PlaySfx(SFXType.Wrench, .5f, 1.1f);
		}

		private void OnHammer()
		{
			Managers.Sound.PlaySfx(SFXType.Hammer, .8f, 1.2f);

			if (!IsOwner) return;
			Vector3 VFXpos = currentOwningProp.transform.position;
			VFXManager.Instance.PlayVFX(VFXType.Spark, VFXpos + transform.forward);
		}

		private void OnOiling()
		{
			if (currentOwningProp is OilPump)
			{
				Managers.Sound.PlaySfx(SFXType.Glug, .9f, UnityEngine.Random.Range(.85f, 1.15f));
			}
		}

		private void OnKick()
		{
			if(!IsOwner) return;
			if (currentKickableCar == null) return;

			Vector3 fromMeToCar = currentKickableCar.transform.position - transform.position;

			if (fromMeToCar.x < 0) // <-
			{
				currentKickableCar.ApplyKickServerRPC(false);
				transform.rotation = Quaternion.Euler(new Vector3(0f, -90f, 0f));
			}
			else if (fromMeToCar.x > 0) // ->
			{
				currentKickableCar.ApplyKickServerRPC(true);
				transform.rotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
			}
		}

		private void OnKickEnd()
        {
            Debug.Log("OnKickEnd");
            Managers.Input.EnablePlayerInputs();
		}

		private void OnGettingUp()
		{
			Debug.Log("OnGettingUp");
            rigid.constraints = originalConstraints;
            Managers.Input.EnablePlayerInputs();
            isKnockedBack = false;
        }

		#endregion
	}
}