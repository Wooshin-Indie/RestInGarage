using Garage.Actions;
using Garage.Interfaces;
using Garage.Manager;
using Garage.Utils;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class OilPump : OwnableProp, IPlaceable, IActionableProp
    {
		[SerializeField] private Vector3 initPos;
		[SerializeField] private Vector3 initRot;

		[SerializeField] private Transform rope;
		[SerializeField] private OilGun oilGun;

		[SerializeField] private LayerMask hitLayers;
		[SerializeField] private Color cuttingColor;
		private Color originColor;

		[SerializeField] private GameObject previewPrefab;
        [SerializeField] private PropAction propAction;

        private Transform oilGunTf;
        private Rigidbody oilGunRigid;
		private RaycastHit[] hits;
		private Material ropeMaterial;

		public override void Awake()
		{
			base.Awake();
			Init();
            oilGunTf = oilGun.GetComponent<Transform>();
            oilGunRigid = oilGun.GetComponent<Rigidbody>();
			hits = new RaycastHit[5];

			if (rope != null)
			{
				ropeMaterial = rope.GetComponent<MeshRenderer>().material;
				originColor = ropeMaterial.GetColor("_Emissive_Color");
			}
        }
        public override void Init()
        {
            base.Init();
        }

        protected override void StartInteraction(ulong newOwnerClientId)
		{
			base.StartInteraction(newOwnerClientId);
		}

		public override void OnEndInteraction(Transform controller)
		{
			base.OnEndInteraction(controller);

			if (GameManagerEx.Instance.IsDay)
			{
				Managers.Sound.PlaySfx(SFXType.Tape, .8f, .8f);
			}
		}

        public void OnStartPropAction(Transform controller)
        {
			//oilGun.StartOilSpray();
        }
        public void OnHoldingPropAction(Transform controller)
        {

        }
        public void OnReleasedPropAction(Transform controller)
        {
            //oilGun.StopOilSpray();
        }
        public virtual void OnAnimationKeyPropAction(Transform controller)
        {

        }
        PropAction IActionableProp.GetPropAction()
        {
            return propAction;
        }

        private void Update()
		{
			if (GameManagerEx.Instance.IsDay)
			{
				if (controller != null)
				{
					oilGunRigid.MovePosition(controller.GetSocket(PropType.Oilgun).position);
					oilGunRigid.MoveRotation(controller.GetSocket(PropType.Oilgun).rotation);
				}
				else
				{
					oilGunTf.localPosition = (initPos);
					oilGunTf.localRotation = (Quaternion.Euler(initRot));
				}

				if (!IsHost) return;

				if(rope != null)
				{
					CheckObstacle();
					UpdateFuelHoseStatus();
				}
            }
			else
			{
				oilGunTf.localPosition = initPos;
				oilGunTf.localRotation = (Quaternion.Euler(initRot));
			}
		}

		bool isThereObstacle = false;
		float hoseCuttingProgress = 0f; // 0~1
		float hoseCuttingTime = 2f;
		private void CheckObstacle()
        {
            if (OwnClientId == ulong.MaxValue) return;

            Vector3 start = rope.position;
			Vector3 end = oilGunTf.position;

			Ray ray = new Ray(start, (end - start).normalized);
			int count = Physics.RaycastNonAlloc(ray, hits, Vector3.Distance(start, end), hitLayers);
			isThereObstacle = false;

			for(int i=0; i<count; i++)
			{
				GameObject hitObj = hits[i].collider.gameObject;

				//if (hitObj == gameObject) continue;
				//if (hitObj == oilgun.gameObject) continue;
				// TODO : 여기 오류남
				if (hitObj.CompareTag(Constants.TAG_PLAYER) && OwnClientId == Controller.OwnerClientId) continue;
				isThereObstacle = true;
                return;
			}

		}
		private void UpdateFuelHoseStatus()
		{
			if (hoseCuttingProgress > 1f)
			{
				OnEndInteraction(transform);
				hoseCuttingProgress = 0f;
			}

			if (isThereObstacle)
			{
				hoseCuttingProgress += Time.deltaTime / hoseCuttingTime;
			}
			else
			{
				hoseCuttingProgress = hoseCuttingProgress >= 0f ?
					(hoseCuttingProgress - Time.deltaTime / hoseCuttingTime) : 0f;
			}
			ropeMaterial.SetColor("_Emissive_Color", Color.Lerp(originColor, cuttingColor, hoseCuttingProgress));
			UpdateFuelHoseColorClientRPC(hoseCuttingProgress);
        }
        [ClientRpc]
        private void UpdateFuelHoseColorClientRPC(float lerpTime)
		{
            ropeMaterial.SetColor("_Emissive_Color", Color.Lerp(originColor, cuttingColor, lerpTime));
        }


        Vector2Int IPlaceable.GetSize()
		{
			return new Vector2Int(2, 2);
		}

		public GameObject GetPreviewPrefab()
		{
			return previewPrefab;
		}
    }
}
