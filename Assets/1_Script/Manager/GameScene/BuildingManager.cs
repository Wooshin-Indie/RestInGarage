using Garage.Interfaces;
using Garage.Props;
using Garage.Utils;
using IUtil;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Manager
{
	public class BuildingManager : NetworkBehaviour
	{
		[Header("Build")]
		[SerializeField] private Vector2Int gridOrigin;
		[SerializeField] private Vector2Int gridSize;
		[SerializeField] private GameObject gridPrefab;
		[SerializeField] private Material gridDefaultMaterial;
		[SerializeField] private Material gridOccupiedMaterial;
		[SerializeField] private Material gridDisabledMaterial;

		[Header("Preview")]
		[SerializeField] private Material previewEnableMaterial;
		[SerializeField] private Material previewDisableMaterial;

		/** 게임 시작 시 Init **/
		private GridTile[,] gridTiles;

		private HashSet<GridTile> previouslyHighlighted = new HashSet<GridTile>();

		// TODO - 호스트가 게임 시작시 직접 스폰하도록
		// + 초기 건물들도 여기서 스폰
		[Button]
		public void OnGameStart()
		{

			gridTiles = new GridTile[gridSize.x, gridSize.y];

			for (int i = 0; i < gridSize.x; i++) {
				for (int j = 0; j < gridSize.y; j++)
				{
					gridTiles[i, j] = Instantiate(gridPrefab, new Vector3(gridOrigin.x - .5f, .01f, gridOrigin.y - .5f) + new Vector3(i, 0, j), Quaternion.Euler(90f, 0f, 0f)).GetComponent<GridTile>();
					gridTiles[i, j].GetComponent<NetworkObject>().Spawn();
				}
			}
			SetActiveGrids(false);
		}

		public void OnStageInit()
		{

		}

		private void ClearGrids()
		{
			for (int i = 0; i < gridTiles.GetLength(0); i++)
			{
				for (int j = 0; j < gridTiles.GetLength(1); j++)
				{
					gridTiles[i, j].SetMaterial(gridTiles[i, j].prop != null ? gridOccupiedMaterial : gridDefaultMaterial);
				}
			}
		}
		public void TryPlaceBuilding(OwnableProp prop)
		{
			if (tmpPreview != null)
			{
				Destroy(tmpPreview);
			}
			SetActiveGrids(false);
			if (!IsAbleToPlace(prop)) return;

			// 설치 가능하다고 판단되면 서버에게 요청
			var tilePositions = new List<Vector2Int>();
			foreach (var tile in previouslyHighlighted)
			{
				Vector2Int index = WorldToGrid(tile.transform.position + new Vector3(.5f, 0f, .5f)) - gridOrigin;
				tilePositions.Add(index);
			}

			TryPlaceServerRpc(prop.NetworkObjectId, tilePositions.ToArray());
		}
		[ServerRpc]
		private void TryPlaceServerRpc(ulong propNetId, Vector2Int[] tileIndices)
		{
			NetworkObject obj = NetworkManager.SpawnManager.SpawnedObjects[propNetId];
			OwnableProp prop = obj.GetComponent<OwnableProp>();

			bool success = true;

			foreach (var index in tileIndices)
			{
				if (!IsInBounds(index)) { success = false; break; }
				if (!gridTiles[index.x, index.y].IsPlaceable(prop)) { success = false; break; }
			}

			if (!success)
			{
				TryPlaceResultClientRpc(false, propNetId, Vector3.zero, 0);
				return;
			}

			foreach (var index in tileIndices)
			{
				gridTiles[index.x, index.y].SetProp(prop);
				gridTiles[index.x, index.y].SetMaterial(gridOccupiedMaterial);
			}

			Vector3 position = GetCenterWorldPosition(tileIndices);
			int rotation = wheelRotate;

			prop.transform.position = position;
			prop.transform.rotation = Quaternion.Euler(0f, rotation * 90f, 0f);

			TryPlaceResultClientRpc(true, propNetId, position, rotation);
		}
		[ClientRpc]
		private void TryPlaceResultClientRpc(bool success, ulong propNetId, Vector3 pos, int rotation)
		{
			if (IsHost) return;

			if (!success)
			{
				Debug.Log("설치 실패");
				return;
			}

			var obj = NetworkManager.SpawnManager.SpawnedObjects[propNetId];
			var prop = obj.GetComponent<OwnableProp>();

			prop.transform.position = pos;
			prop.transform.rotation = Quaternion.Euler(0f, rotation * 90f, 0f);
		}
		private Vector3 GetCenterWorldPosition(Vector2Int[] indices)
		{
			Vector3 avg = Vector3.zero;
			foreach (var idx in indices)
			{
				avg += gridTiles[idx.x, idx.y].transform.position;
			}
			return avg / indices.Length;
		}
		private bool IsAbleToPlace(OwnableProp prop)
		{
			if (prop.GetComponent<IPlaceable>() == null) return false;
			Vector2Int tmpV = prop.GetComponent<IPlaceable>().GetSize();

			if (previouslyHighlighted.Count != (tmpV.x * tmpV.y))
				return false;

			// 전부 설치가능한 Grid인지 확인
			foreach (var tile in previouslyHighlighted)
				if (!tile.IsPlaceable(prop))
					return false;

			return true;
		}

		private void SetActiveGrids(bool isActive)
		{
			for (int i = 0; i < gridTiles.GetLength(0); i++)
			{
				for (int j = 0; j < gridTiles.GetLength(1); j++)
				{
					gridTiles[i, j].gameObject.SetActive(isActive);
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

		GameObject tmpPreview = null; 
		private Material lastAppliedMaterial = null;

		private int wheelRotate = 0;

		private void OnRotate()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				wheelRotate = (wheelRotate + 1) % 4;
			}

		}

		public void UpdatePreviewArea(OwnableProp prop, Transform playerTransform)
		{

			if (!gridTiles[0, 0].gameObject.activeSelf)
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
			foreach (var tile in previouslyHighlighted)
			{ 
				tile.SetMaterial(tile.prop != null ? gridOccupiedMaterial : gridDefaultMaterial);
			}
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
			Vector2Int startGridPos = WorldToGrid(playerTransform.position + offset);

			for (int x = 0; x < placeSize.x; x++)
			{
				for (int y = 0; y < placeSize.y; y++)
				{
					Vector2Int tilePos = startGridPos + new Vector2Int(x - centerOffset.x, y - centerOffset.y);
					Vector2Int index = tilePos - gridOrigin;

					if (IsInBounds(index))
					{
						GridTile tile = gridTiles[index.x, index.y];
						tile.SetMaterial(tile.IsPlaceable(prop) ? gridOccupiedMaterial : gridDisabledMaterial);
						previouslyHighlighted.Add(tile);
					}
				}
			}

			if (IsAbleToPlace(prop))
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

		private Vector2Int WorldToGrid(Vector3 pos)
		{
			return new Vector2Int(
				Mathf.RoundToInt(pos.x),
				Mathf.RoundToInt(pos.z)
			);
		}

		private bool IsInBounds(Vector2Int pos)
		{
			return pos.x >= 0 && pos.y >= 0 && pos.x < gridSize.x && pos.y < gridSize.y;
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

	}
}
