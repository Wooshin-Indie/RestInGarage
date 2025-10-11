using Garage.Interfaces;
using Garage.Props;
using Garage.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

namespace Garage.Manager
{
	public enum GridType
	{
		Place,
		Sell,
		Upgrade
	}

	[Serializable]
	public class GridData
	{
		public GridType gridType;
		[Tooltip("그리드 원점")]
		public Vector2Int gridOrigin;
        [Tooltip("그리드 크기, 맵기준 기준점으로부터 x+ 방향은 ↑, y+ 방향은 ← ")]
        public Vector2Int gridSize;
    }

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
            gridTileDict = new();
            GridType[] gridTypes = GetGridTypes();
            foreach (GridType gridType in gridTypes)
            {
                gridTileDict.Add(gridType, new Dictionary<Vector2Int, GridTile>());
            }

            GameManagerEx.Instance.OnDisconnectedAction -= OnDisconnected;
        }
		#endregion

		private void Start()
		{
			GameManagerEx.Instance.OnBeforeStageStartAction += OnStageStart;
		}

		[Header("Build")]
		[SerializeField] private GameObject gridPrefab;
		[SerializeField] private List<GridData> gridDatas;

		[Header("Preview")]
		[SerializeField] private Material previewEnableMaterial;
		[SerializeField] private Material previewDisableMaterial;

		[SerializeField] private GameObject sellBoundPrefab;
		[SerializeField] private GameObject upgradeBoundPrefab;

		/** 게임 시작 시 Init **/
		private Dictionary<GridType, Dictionary<Vector2Int, GridTile>> gridTileDict; // gridTileDict[GridType][int월드좌표]
        public Dictionary<GridType, Dictionary<Vector2Int, GridTile>> GridTileDict => gridTileDict;

		private HashSet<GridTile> previouslyHighlighted = new HashSet<GridTile>();

		public Dictionary<ulong, OwnableProp> PlacedBuildings = new();
		public Dictionary<ulong, OwnableProp> ItemDictionary = new();
		public Dictionary<ulong, GameObject> NightDecoPropDictionary = new();

		GameObject tmpPreview = null;
		private Material lastAppliedMaterial = null;
		private int wheelRotate = 0;

		public void OnGameStart()
		{
			SetGridTileDict();

            PlacedBuildings.Clear();
			ItemDictionary.Clear();
		}
		private void SetGridTileDict()
		{
            foreach (GridData gridData in gridDatas)
            {
                for (int i = 0; i < gridData.gridSize.x; i++)
                {
                    for (int j = 0; j < gridData.gridSize.y; j++)
                    {
						Vector2Int gridPos = new Vector2Int(gridData.gridOrigin.x + i, gridData.gridOrigin.y + j);
                        GridTile tile = Managers.Spawn.SpawnInCurrentScene(
                            gridPrefab,
                            new Vector3(gridPos.x, .01f, gridPos.y),
                            Quaternion.Euler(90f, 0f, 0f)
                            ).GetComponent<GridTile>();
                        gridTileDict[gridData.gridType].Add(gridPos, tile);
                        tile.InitGridTile(gridData.gridType, gridPos);
                    }
                }
            }
        }
        public void RegisterTileOnClient(GridTile tile)
        {
			if (NetworkTransmission.instance.IsHost) return;

            Vector2Int pos = Vector2Int.zero;
            bool isInBound = false;
            foreach (var gridType in GetGridTypes())
            {
                pos = WorldToGrid(tile.transform.position);
                gridTileDict[gridType].Add(pos, tile);
                isInBound = true;
            }

            if (!isInBound)
            {
                Debug.Log("[BuildingManager] : Nothing Matched");
                return;
            }

        }

        private List<Vector3> shopPositions = new();
		private GameObject sellBoundGameObject = null;
		private GameObject upgradeBoundGameObject = null;
		public void SpawnBasicBuildings_HostOnly(int mapIdx)
		{
			shopPositions = Managers.Resource.GetData<MapData>(mapIdx).ItemPositions;

			// TODO - Map마다 지을 빌딩이 다를수도 있음 - mapIdx로 SO 받아와서 처리
			GameObject go = Managers.Spawn.SpawnInCurrentScene(prefabList[0]);
			BuildingNetworkManager.Instance.TryPlaceServerRpc(go.GetComponent<NetworkObject>().NetworkObjectId,
                GridType.Place, 0, new Vector2Int[8]
				{
					new Vector2Int(-3, 3),
					new Vector2Int(-3, 4),
					new Vector2Int(-3, 5),
					new Vector2Int(-3, 6),
					new Vector2Int(-2, 3),
					new Vector2Int(-2, 4),
					new Vector2Int(-2, 5),
					new Vector2Int(-2, 6)
				},
				NetworkManager.Singleton.LocalClientId);
			PlacedBuildings.Add(go.GetComponent<NetworkObject>().NetworkObjectId, go.GetComponent<OwnableProp>());

			go = Managers.Spawn.SpawnInCurrentScene(prefabList[2]);
			BuildingNetworkManager.Instance.TryPlaceServerRpc(go.GetComponent<NetworkObject>().NetworkObjectId,
				GridType.Place, 2, new Vector2Int[4]
				{
					new Vector2Int(-4, 1),
					new Vector2Int(-4, 2),
					new Vector2Int(-3, 1),
					new Vector2Int(-3, 2),
				},
				NetworkManager.Singleton.LocalClientId);
			PlacedBuildings.Add(go.GetComponent<NetworkObject>().NetworkObjectId, go.GetComponent<OwnableProp>());

			go = Managers.Spawn.SpawnInCurrentScene(prefabList[3]);
			BuildingNetworkManager.Instance.TryPlaceServerRpc(go.GetComponent<NetworkObject>().NetworkObjectId,
                GridType.Place, 0, new Vector2Int[1]
				{
					new Vector2Int(-2, 0)
				},
				NetworkManager.Singleton.LocalClientId);
			PlacedBuildings.Add(go.GetComponent<NetworkObject>().NetworkObjectId, go.GetComponent<OwnableProp>());


			go = Managers.Spawn.SpawnInCurrentScene(prefabList[1]);
			BuildingNetworkManager.Instance.TryPlaceServerRpc(go.GetComponent<NetworkObject>().NetworkObjectId,
                GridType.Place, 0, new Vector2Int[1]
				{
					new Vector2Int(-4, 5)
				},
				NetworkManager.Singleton.LocalClientId);
			PlacedBuildings.Add(go.GetComponent<NetworkObject>().NetworkObjectId, go.GetComponent<OwnableProp>());

            go = Managers.Spawn.SpawnInCurrentScene(prefabList[5]);
            BuildingNetworkManager.Instance.TryPlaceServerRpc(go.GetComponent<NetworkObject>().NetworkObjectId,
                GridType.Place, 0, new Vector2Int[4]
                {
                    new Vector2Int(6, 5),
                    new Vector2Int(6, 6),
                    new Vector2Int(6, 7),
                    new Vector2Int(6, 8),
                },
                NetworkManager.Singleton.LocalClientId);
            PlacedBuildings.Add(go.GetComponent<NetworkObject>().NetworkObjectId, go.GetComponent<OwnableProp>());

            Vector3 sellPos = GetGridMiddlePos(GridType.Sell);
            sellBoundGameObject = Managers.Spawn.SpawnInCurrentScene(sellBoundPrefab, sellPos, Quaternion.Euler(0, 90f, 0));

            Vector3 upgradePos = GetGridMiddlePos(GridType.Upgrade);
            upgradeBoundGameObject = Managers.Spawn.SpawnInCurrentScene(upgradeBoundPrefab, upgradePos, Quaternion.Euler(0, 90f, 0));
		}

		private Vector3 GetGridMiddlePos(GridType gridType)
		{
			Vector3 gridMiddlePos = Vector3.zero;
			foreach (GridData gridData in gridDatas)
			{
				if (gridData.gridType == gridType)
				{
					gridMiddlePos = new Vector3(
						gridData.gridOrigin.x + (gridData.gridSize.x - 1) / 2,
						0f,
						gridData.gridOrigin.y + (gridData.gridSize.y - 1) / 2);
					break;
                }
			}
			return gridMiddlePos;
		}

		// HACK - 나중엔 ResourceManager에서 로드해서 갖고있어야됨

		[SerializeField] private List<GameObject> prefabList = new();
		[SerializeField] private List<GameObject> nightDecoPrefabList = new();
		[SerializeField] private GameObject lightPrefab;
		
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

			foreach (var entry in NightDecoPropDictionary)
			{
				entry.Value.GetComponent<NetworkObject>().Despawn();
				Destroy(entry.Value.gameObject);
			}

			ItemDictionary.Clear();
			NightDecoPropDictionary.Clear();
			TurnOffLights();
			BuildingNetworkManager.Instance.OnShopItemEraseAllClientRPC();
			sellBoundGameObject.SetActive(false);
			upgradeBoundGameObject.SetActive(false);
		}

		// 스테이지 종료 시 구매할 빌딩 스폰
		public void OnStageEnd(int stageId)
		{
			SpawnNightDecoProps();

			if (stageId <= 0) return;

			var randPrefabs = GetRandomPrefabs(3);
			for (int i = 0; i < shopPositions.Count; i++)
			{
				GameObject tmpGo = Managers.Spawn.SpawnInCurrentScene(randPrefabs[i], shopPositions[i], Quaternion.identity);
				tmpGo.GetComponent<OwnableProp>().SetGridPosition(shopPositions[i]);
				ItemDictionary.Add(tmpGo.GetComponent<NetworkObject>().NetworkObjectId, tmpGo.GetComponent<OwnableProp>());

				Light lightGo = Managers.Spawn.SpawnInCurrentScene(lightPrefab, new Vector3(shopPositions[i].x, 10f, shopPositions[i].z), Quaternion.Euler(90f, 0f, 0f)).GetComponent<Light>();
				lightDictionary.Add(tmpGo.GetComponent<NetworkObject>().NetworkObjectId, lightGo);
			}

			foreach (var item in ItemDictionary)
			{
				BuildingNetworkManager.Instance.OnShopItemRevealedClientRPC(item.Value.transform.position - new Vector3(1.5f, 0, 0), item.Key, item.Value.ItemData.GetBuyPrice(item.Value.UpgradeLevel));
			}

			sellBoundGameObject.SetActive(true);
			upgradeBoundGameObject.SetActive(true);
		}

		private void SpawnNightDecoProps()
		{
            GameObject tmpGo = Managers.Spawn.SpawnInCurrentScene(nightDecoPrefabList[0], new Vector3(16, 0 ,4), Quaternion.Euler(0f, 173f, 0f));
            NightDecoPropDictionary.Add(tmpGo.GetComponent<NetworkObject>().NetworkObjectId, tmpGo);
        }

		public void TryPlaceBuilding(OwnableProp prop)
		{
			if (tmpPreview != null)
			{
				Destroy(tmpPreview);
			}
			SetActiveGrids(false);

			if (!IsAbleToPlaceOnHilighted(prop, out GridType gridType))
			{
				Managers.Sound.PlaySfx(SFXType.Wrong, .7f, 1f);
				return;
			}

			// 설치 가능하다고 판단되면 서버에게 요청
			var tilePositions = new List<Vector2Int>();
			foreach (var tile in previouslyHighlighted)
			{
				Vector2Int pos = WorldToGrid(tile.transform.position);
				tilePositions.Add(pos);
			}

			BuildingNetworkManager.Instance.TryPlaceServerRpc(prop.NetworkObjectId, gridType, wheelRotate, tilePositions.ToArray(),
				NetworkManager.Singleton.LocalClientId);
		}

		public Vector3 GetCenterWorldPos(Vector2Int[] tilePositions)
		{
            Vector2 avg = Vector2.zero;
			foreach (var pos in tilePositions)
			{
				avg += pos;
			}
			avg = avg / tilePositions.Length;

            return new Vector3(avg.x, 0, avg.y);
		}

        /// <summary>
        /// previouslyHighlighted 된 GridTile들에 놓을 수 있는지 검사
        /// </summary>
        private bool IsAbleToPlaceOnHilighted(OwnableProp prop, out bool isTileSizeCorrect)
		{
			return IsAbleToPlaceOnHilighted(prop, out isTileSizeCorrect, out GridType gridType);
        }
		private bool IsAbleToPlaceOnHilighted(OwnableProp prop, out GridType gridType)
		{
            return IsAbleToPlaceOnHilighted(prop, out bool isTileSizeCorrect, out gridType);
        }
        private bool IsAbleToPlaceOnHilighted(OwnableProp prop, out bool isTileSizeCorrect, out GridType gridType)
		{
            // isTileSizeCorrect는 프랍사이즈하고 hilight된 타일사이즈하고 개수 맞는지
            gridType = GridType.Place;
			isTileSizeCorrect = false;
			if (prop.GetComponent<IPlaceable>() == null)
			{
				isTileSizeCorrect = false;
                return false;
			}
			Vector2Int propSize = prop.GetComponent<IPlaceable>().GetSize();

			if (previouslyHighlighted.Count != (propSize.x * propSize.y))
			{
				isTileSizeCorrect = false;
                return false;
            }

			// 전부 설치가능한 Grid인지 확인
			foreach (var tile in previouslyHighlighted)
			{
				if (!tile.IsPlaceable(prop))
				{
					isTileSizeCorrect = true;
                    return false;
                }
			}

			// Tile의 GridType 반환
			gridType = previouslyHighlighted.First().gridType.Value;
			isTileSizeCorrect = false;
            return true;
		}

		private bool isGridsActive = false;
		private void SetActiveGrids(bool isActive)
		{
			foreach (GridType gridType in GetGridTypes())
			{
				foreach (GridTile tile in gridTileDict[gridType].Values)
					tile.gameObject.SetActive(isActive);
            }
			isGridsActive = isActive;
        }

		private Vector3 GetAveragePosition()
		{
			Vector3 averageWorldPos = Vector3.zero;
			foreach (var tile in previouslyHighlighted)
			{
				averageWorldPos += tile.transform.position;
			}
			averageWorldPos /= (previouslyHighlighted.Count == 0 ? 1 : previouslyHighlighted.Count);
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
			if (!isGridsActive)
			{
				SetActiveGrids(true);
				tmpPreview = Managers.Spawn.SpawnInCurrentScene(prop.GetComponent<IPlaceable>().GetPreviewPrefab());
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
			Vector3 offsetForward = new Vector3(forward.x * placeSize.x / 2, 0, forward.z * placeSize.y / 2);
			Vector2Int startGridPos = WorldToGrid(playerTransform.position + offsetForward);
			Vector2Int previewPos = startGridPos;

			foreach (GridType gridType in GetGridTypes())
			{
                for (int x = 0; x < placeSize.x; x++)
                {
                    for (int y = 0; y < placeSize.y; y++)
                    {
						Vector2Int offsetFromStart = new Vector2Int(x - centerOffset.x, y - centerOffset.y);
						offsetFromStart.x = forward.x > 0.1f ? offsetFromStart.x : -offsetFromStart.x;
						offsetFromStart.y = forward.z > 0.1f ? offsetFromStart.y : -offsetFromStart.y;
                        Vector2Int tilePos = startGridPos + offsetFromStart;

                        if (IsInBounds(gridType, tilePos))
                        {
                            GridTile tile = gridTileDict[gridType][tilePos];
                            previouslyHighlighted.Add(tile);
                        }
                    }
                }
            }

			if (IsAbleToPlaceOnHilighted(prop, out bool isTileSizeCorrect))
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
                if (isTileSizeCorrect)
                    tmpPreview.transform.position = GetAveragePosition();
                else
                    tmpPreview.transform.position = new Vector3(previewPos.x, .01f, previewPos.y) + offsetForward/2;

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
            foreach (GridType gridType in GetGridTypes())
            {
                foreach (GridTile tile in gridTileDict[gridType].Values)
				{
                    if (tile.prop != null && tile.prop.GetComponent<NetworkObject>().NetworkObjectId == networkId)
                    {
                        count++;
                        sumPos += tile.transform.position;
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

		/// 메모리 문제로 props가 리턴값 대신 쓰입니다.
		public void GetPropsInGrid(GridType gridType, HashSet<OwnableProp> props)
		{
			props.Clear();
            
			foreach (GridTile tile in gridTileDict[gridType].Values)
			{
                if (tile.prop != null)
                    props.Add(tile.prop);
            }
		}

		// 범위로 검사해야될라나
		public bool IsInBounds(GridType gridType, Vector2Int tilePos)
		{
            return gridTileDict[gridType].ContainsKey(tilePos);
		}
        /// <summary>
        /// tilePositions에 놓을 수 있는지 물리적으로 직접 검사
        /// </summary>
        public bool IsAbleToPlace(GridType gridType, Vector2Int[] tilePositions, OwnableProp prop)
		{
			if (tilePositions.Count() <= 0) return false;

            bool success = true;
            foreach (Vector2Int tilePos in tilePositions)
            {
				if (!gridTileDict[gridType].ContainsKey(tilePos)) { success = false; break; }
                if (!gridTileDict[gridType][tilePos].IsPlaceable(prop)) { success = false; break; }
            }
			return success;
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

			NightDecoPropDictionary.Clear();

            foreach (GridType gridType in GetGridTypes())
            {
                foreach (Vector2Int tilePos in gridTileDict[gridType].Keys)
                {
                    if (gridTileDict[gridType][tilePos] == null) continue;
                    gridTileDict[gridType][tilePos].GetComponent<NetworkObject>().Despawn();
                    Destroy(gridTileDict[gridType][tilePos].gameObject);
                }
            }

            // HACK - Client측에서 게임 다시시작 할때 다시 할당해주는 코드 없어서 임시로 주석처리함
            // gridTiles.Clear();
        }
		public void OnGameStarted()
		{
            GameManagerEx.Instance.OnDisconnectedAction += OnDisconnected;

            int mapIdx = GameSynchronizer.Instance.MapIdx.Value;
            if (NetworkManager.Singleton.IsHost) SpawnBasicBuildings_HostOnly(mapIdx);
		}
		public List<GameObject> GetRandomPrefabs(int counts)
		{
			var result = new List<GameObject>();
			var usedTypes = new HashSet<ItemType>();

			int maxTries = 1000; // 무한루프 방지용
			int tries = 0;

			while (result.Count < 3 && tries < maxTries)
			{
				tries++;

				GameObject randomPrefab = prefabList[UnityEngine.Random.Range(0, prefabList.Count)];
				var prop = randomPrefab.GetComponent<OwnableProp>();
				if (prop == null || prop.ItemData == null)
					continue;

				var type = prop.ItemData.ItemType;

				if (usedTypes.Contains(type))
					continue;

				usedTypes.Add(type);
				result.Add(randomPrefab);
			}

			if (result.Count < counts)
			{
				Debug.LogWarning("서로 다른 ItemType을 가진 프리팹이 3개 미만입니다.");
			}

			return result;
		}
		public GridType[] GetGridTypes()
		{
            return (GridType[])Enum.GetValues(typeof(GridType));
		}
	}
}
