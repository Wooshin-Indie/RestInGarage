using Garage.Interfaces;
using Garage.Props;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Manager
{
	public class BuildingManager : MonoBehaviour
	{
		#region Singleton
		private static BuildingManager instance;
		public static BuildingManager Instance { get => instance; }

		void Awake()
		{
			Init();
		}

		private void Init()
		{
			if (null == instance)
			{
				instance = this;
				DontDestroyOnLoad(this.gameObject);
			}
			else
			{
				Destroy(this.gameObject);
			}

			gridTiles = new();
			for (int t = 0; t < gridOrigin.Count; t++)
			{
				gridTiles.Add(new GridTile[gridSize[t].x, gridSize[t].y]);
			}

			GameManagerEx.Instance.OnDisconnected += OnDisconnected;
		}
		#endregion

		[Header("Build")]
		[SerializeField] private List<Vector2Int> gridOrigin;
		[SerializeField] private List<Vector2Int> gridSize;
		[SerializeField] private GameObject gridPrefab;

		[Header("Preview")]
		[SerializeField] private Material previewEnableMaterial;
		[SerializeField] private Material previewDisableMaterial;

		/** 게임 시작 시 Init **/
		private List<GridTile[,]> gridTiles;
		public List<GridTile[,]> GridTiles => gridTiles;

		private HashSet<GridTile> previouslyHighlighted = new HashSet<GridTile>();

		public Dictionary<ulong, OwnableProp> PlacedBuildings = new();
		public Dictionary<ulong, OwnableProp> ItemDictionary = new();
		public Dictionary<ulong, GameObject> DecoPropDictionary = new();

		GameObject tmpPreview = null;
		private Material lastAppliedMaterial = null;
		private int wheelRotate = 0;

		public void OnGameStart()
		{
			gridTiles = new();

			for (int t = 0; t < gridOrigin.Count; t++)
			{
				gridTiles.Add(new GridTile[gridSize[t].x, gridSize[t].y]);
				for (int i = 0; i < gridSize[t].x; i++)
				{
					for (int j = 0; j < gridSize[t].y; j++)
					{
						GridTile tile = Instantiate(gridPrefab, new Vector3(gridOrigin[t].x - .5f, .01f, gridOrigin[t].y - .5f) + new Vector3(i, 0, j), Quaternion.Euler(90f, 0f, 0f)).GetComponent<GridTile>();
						tile.GetComponent<NetworkObject>().Spawn();
						tile.SetGridPosition(t, i, j);
					}
				}
			}

			PlacedBuildings.Clear();
		}

		public void RegisterTile(GridTile tile)
		{
			Vector2Int index = Vector2Int.zero;
			bool isInBound = false;
			for (int t = 0; t < gridOrigin.Count; t++)
			{
				index = WorldToGrid(tile.transform.position + new Vector3(.5f, 0f, .5f)) - gridOrigin[t];
				if (IsInBounds(t, index))
				{
					gridTiles[t][index.x, index.y] = tile;
					isInBound = true;
					break;
				}
			}

			if (!isInBound)
			{
				Debug.Log("[BuildingManager] : Nothing Matched");
				return;
			}

		}

		// HACK - 나중엔 ResourceManager에서 로드해서 갖고있어야됨

		[SerializeField] private List<GameObject> prefabList = new();
		[SerializeField] private GameObject lightPrefab;
		[SerializeField] private List<Vector3> shopPositions = new();

		private Dictionary<ulong, Light> lightDictionary = new();

		public void OnBuyItem(ulong networkId)
		{
			if (lightDictionary.TryGetValue(networkId, out Light light) && light != null)
			{
				light.GetComponent<NetworkObject>().Despawn();
				Destroy(light.gameObject);
			}
		}

		private void TurnOffLights()
		{
			foreach (var light in lightDictionary)
			{
				if (light.Value == null) continue;
				light.Value.GetComponent<NetworkObject>().Despawn();
				Destroy(light.Value.gameObject);
			}

			lightDictionary.Clear();
		}

		// 스테이지 시작 시 구매하지 않은 빌딩 삭제
		public void OnStageStart()
		{
			foreach (var entry in ItemDictionary)
			{
				entry.Value.GetComponent<NetworkObject>().Despawn();
				Destroy(entry.Value.gameObject);
			}

			foreach (var entry in DecoPropDictionary)
			{
				entry.Value.GetComponent<NetworkObject>().Despawn();
				Destroy(entry.Value.gameObject);
			}

			ItemDictionary.Clear();
			DecoPropDictionary.Clear();
			TurnOffLights();
			BuildingNetworkManager.Instance.OnShopItemEraseAllClientRPC();
		}

		// 스테이지 종료 시 구매할 빌딩 스폰
		public void OnStageEnd(int stageId)
		{
			SpawnNightDecoProps();

			if (stageId <= 0) return;

			// HACK - 랜덤으로 바꾸기
			for (int i = 0; i < shopPositions.Count; i++)
			{
				GameObject tmpGo = Instantiate(prefabList[i], shopPositions[i], Quaternion.Euler(0f, -90f, 0f));
				tmpGo.GetComponent<NetworkObject>().Spawn();
				tmpGo.GetComponent<OwnableProp>().SetGridPosition(shopPositions[i]);
				ItemDictionary.Add(tmpGo.GetComponent<NetworkObject>().NetworkObjectId, tmpGo.GetComponent<OwnableProp>());

				lightDictionary.Add(tmpGo.GetComponent<NetworkObject>().NetworkObjectId,
					Instantiate(lightPrefab, new Vector3(shopPositions[i].x, 10f, shopPositions[i].z), Quaternion.Euler(90f, 0f, 0f)).GetComponent<Light>());
				lightDictionary[tmpGo.GetComponent<NetworkObject>().NetworkObjectId].GetComponent<NetworkObject>().Spawn();
			}


			foreach (var item in ItemDictionary)
			{
				BuildingNetworkManager.Instance.OnShopItemRevealedClientRPC(item.Value.transform.position - new Vector3(0, 0, 1.5f), item.Key, item.Value.ItemData.BuyPrice);
			}
		}

		private void SpawnNightDecoProps()
		{

		}

		public void TryPlaceBuilding(OwnableProp prop)
		{
			if (tmpPreview != null)
			{
				Destroy(tmpPreview);
			}
			SetActiveGrids(false);
			int gridIdx = IsAbleToPlace(prop);
			if (gridIdx == -1) return;

			// 설치 가능하다고 판단되면 서버에게 요청
			var tilePositions = new List<Vector2Int>();
			foreach (var tile in previouslyHighlighted)
			{
				Vector2Int index = WorldToGrid(tile.transform.position + new Vector3(.5f, 0f, .5f)) - gridOrigin[gridIdx];
				tilePositions.Add(index);
			}

			BuildingNetworkManager.Instance.TryPlaceServerRpc(prop.NetworkObjectId, gridIdx, wheelRotate, tilePositions.ToArray());
		}

		public Vector3 GetCenterWorldPosition(int index, Vector2Int[] indices)
		{
			Vector3 avg = Vector3.zero;
			foreach (var idx in indices)
			{
				avg += gridTiles[index][idx.x, idx.y].transform.position;
			}
			return avg / indices.Length;
		}

		private int IsAbleToPlace(OwnableProp prop)
		{
			if (prop.GetComponent<IPlaceable>() == null) return -1;
			Vector2Int tmpV = prop.GetComponent<IPlaceable>().GetSize();

			if (previouslyHighlighted.Count != (tmpV.x * tmpV.y))
				return -1;

			// 전부 설치가능한 Grid인지 확인
			foreach (var tile in previouslyHighlighted)
			{
				if (!tile.IsPlaceable(prop))
					return -1;
			}

			// Tile의 GridIdx 반환
			return previouslyHighlighted.First().GridPosition.Value.x;
		}

		private void SetActiveGrids(bool isActive)
		{
			for (int t = 0; t < gridTiles.Count; t++)
			{
				for (int i = 0; i < gridTiles[t].GetLength(0); i++)
				{
					for (int j = 0; j < gridTiles[t].GetLength(1); j++)
					{
						gridTiles[t][i, j].gameObject.SetActive(isActive);
					}
				}
			}
		}

		private Vector3 GetAveragePosition()
		{
			Vector3 averageWorldPos = Vector3.zero;
			foreach (var tile in previouslyHighlighted)
			{
				averageWorldPos += tile.transform.position;
			}
			averageWorldPos /= previouslyHighlighted.Count;
			return averageWorldPos;
		}

		private void OnRotate()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				wheelRotate = (wheelRotate + 1) % 4;
			}
		}

		public void UpdatePreviewArea(OwnableProp prop, Transform playerTransform)
		{
			if (!gridTiles[0][0, 0].gameObject.activeSelf)
			{
				SetActiveGrids(true);
				tmpPreview = Instantiate(prop.GetComponent<IPlaceable>().GetPreviewPrefab());
				wheelRotate = 0;
			}

			OnRotate();
			if (tmpPreview != null)
			{
				tmpPreview.transform.rotation = Quaternion.Euler(0f, wheelRotate * 90f, 0f);
			}

			IPlaceable placeable = prop.GetComponent<IPlaceable>();
			previouslyHighlighted.Clear();

			Vector2Int placeSize = placeable.GetSize();
			switch (wheelRotate)
			{
				case 1:
				case 3:
					placeSize = new Vector2Int(placeSize.y, placeSize.x);
					break;
			}


			Vector2Int centerOffset = new Vector2Int((placeSize.x - 1) / 2, (placeSize.y - 1) / 2);

			Vector3 forward = playerTransform.forward;
			Vector3 offset = new Vector3(forward.x * placeSize.x / 2, 0, forward.z * placeSize.y / 2);
			Vector2Int startGridPos = WorldToGrid(playerTransform.position + offset
				  + new Vector3(placeSize.x % 2 == 1 ? .5f : 0f, 0f, placeSize.y % 2 == 1 ? .5f : 0f));

			for (int t = 0; t < gridTiles.Count; t++)
			{
				for (int x = 0; x < placeSize.x; x++)
				{
					for (int y = 0; y < placeSize.y; y++)
					{
						Vector2Int tilePos = startGridPos + new Vector2Int(x - centerOffset.x, y - centerOffset.y);
						Vector2Int index = tilePos - gridOrigin[t];

						if (IsInBounds(t, index))
						{
							GridTile tile = gridTiles[t][index.x, index.y];
							previouslyHighlighted.Add(tile);
						}
					}
				}
			}

			if (IsAbleToPlace(prop) != -1)
			{
				tmpPreview.transform.position = GetAveragePosition();

				if (lastAppliedMaterial != previewEnableMaterial)
				{
					ChangePreviewMaterial(tmpPreview.gameObject, previewEnableMaterial);
					lastAppliedMaterial = previewEnableMaterial;
				}
			}
			else
			{
				tmpPreview.transform.position = new Vector3(startGridPos.x, 0, startGridPos.y);

				if (lastAppliedMaterial != previewDisableMaterial)
				{
					ChangePreviewMaterial(tmpPreview.gameObject, previewDisableMaterial);
					lastAppliedMaterial = previewDisableMaterial;
				}
			}
		}

		public Vector3 GetBuildingPosition(ulong networkId)
		{
			int count = 0;
			Vector3 sumPos = Vector3.zero;
			for (int t = 0; t < gridTiles.Count; t++)
			{
				for (int i = 0; i < gridTiles[t].GetLength(0); i++)
				{
					for (int j = 0; j < gridTiles[t].GetLength(1); j++)
					{
						if (gridTiles[t][i, j].prop != null && gridTiles[t][i, j].prop.GetComponent<NetworkObject>().NetworkObjectId == networkId)
						{
							count++;
							sumPos += gridTiles[t][i, j].transform.position;
						}
					}
				}
			}

			if (count == 0) return Vector3.zero;
			return sumPos / count;
		}

		private Vector2Int WorldToGrid(Vector3 pos)
		{
			return new Vector2Int(
				Mathf.RoundToInt(pos.x),
				Mathf.RoundToInt(pos.z)
			);
		}

		public bool IsInBounds(int gridIndex, Vector2Int pos)
		{
			return pos.x >= 0 && pos.y >= 0 && pos.x < gridSize[gridIndex].x && pos.y < gridSize[gridIndex].y;
		}

		private Vector3 GetMouseWorldPosOnY0()
		{
			Camera cam = Camera.main;
			Vector3 mousePos = Input.mousePosition;

			Vector3 near = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
			Vector3 far = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.farClipPlane));

			Vector3 dir = (far - near).normalized;

			float t = -near.y / dir.y;
			Vector3 hit = near + dir * t;

			return hit;
		}

		private void ChangePreviewMaterial(GameObject go, Material material)
		{
			Renderer[] renderers = go.GetComponentsInChildren<Renderer>(includeInactive: true);

			foreach (Renderer renderer in renderers)
			{
				renderer.sharedMaterial = material;
			}
		}

		private void OnDisconnected()
		{
			if (!NetworkManager.Singleton.IsHost) return;

			for (int t = 0; t < gridOrigin.Count; t++)
			{
				gridTiles.Add(new GridTile[gridSize[t].x, gridSize[t].y]);
				for (int i = 0; i < gridSize[t].x; i++)
				{
					for (int j = 0; j < gridSize[t].y; j++)
					{
						gridTiles[t][i, j].GetComponent<NetworkObject>().Despawn();
						Destroy(gridTiles[t][i, j].gameObject);
					}
				}
			}
			gridTiles.Clear();
		}
	}
}
