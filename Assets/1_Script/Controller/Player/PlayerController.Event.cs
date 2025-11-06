
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
				if (currentOwningProp is TireProp) OnTireRollEnd();
                // 이 때 굴림
            }
			currentOwningProp = null;
		}

		// 타이어 놓을 때 애니메이션 키로 실행
        private void OnTireRollEnd()
        {
            UIManager.Game.CloseTireRollingUI();
            isRollChargeStarted = false;
            Managers.Input.EnablePlayerMove(); // 0.1초 뒤에 enable 하면 좋을 듯
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