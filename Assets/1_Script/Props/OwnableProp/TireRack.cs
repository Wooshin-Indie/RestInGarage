using Garage.Interfaces;
using Garage.Manager;
using Garage.Utils;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class TireRack : OwnableProp, IPlaceable
	{
		[SerializeField] private TireSize tireSize;
        [SerializeField] private GameObject tirePrefab;
		[SerializeField] private GameObject previewPrefab;

        public override void Awake()
        {
            base.Awake();
            Init();
        }
        public override void Init()
        {
            base.Init();
        }

        protected override void StartInteraction(ulong newOwnerClientId)
		{
			base.StartInteraction(newOwnerClientId);

			if (GameManagerEx.Instance.IsDay)
			{
				SpawnTireServerRpc(newOwnerClientId);
				OnEndInteraction(null);
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private void SpawnTireServerRpc(ulong newOwnerClientId)
		{
			GameObject go = Managers.Spawn.SpawnInCurrentScene(tirePrefab, NetworkManager.Singleton.ConnectedClients[newOwnerClientId].PlayerObject.transform.position, Quaternion.identity, null);
			go.GetComponent<TireProp>().SetTireSize(tireSize);
			go.GetComponent<TireProp>().TryInteract(newOwnerClientId);
		}

		public override void OnEndInteraction(Transform controller)
		{
			base.OnEndInteraction(controller);
		}

		private void Update()
		{

		}

		public Vector2Int GetSize()
		{
			return new Vector2Int(2, 4);
		}

		public GameObject GetPreviewPrefab()
		{
			return previewPrefab;
		}
	}
}
