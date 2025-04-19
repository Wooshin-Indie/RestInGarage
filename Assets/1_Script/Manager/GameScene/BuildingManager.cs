using Garage.Interfaces;
using Garage.Props;
using Garage.Utils;
using IUtil;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace Garage.Manager
{
	public class BuildingManager : MonoBehaviour
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

		// TODO - 게임 시작시 직접 스폰하도록
		// + 초기 건물들도 여기서 스폰
		[Button]
		public void OnGameStart()
		{
			GameObject parent = new GameObject { name = "Grids" };
			parent.transform.position = new Vector3(gridOrigin.x - .5f, .01f, gridOrigin.y - .5f);
			gridTiles = new GridTile[gridSize.x, gridSize.y];


			for (int i = 0; i < gridSize.x; i++) {
				for (int j = 0; j < gridSize.y; j++)
				{
					gridTiles[i, j] = Instantiate(gridPrefab, parent.transform.position + new Vector3(i, 0, j), Quaternion.Euler(90f, 0f, 0f), parent.transform).GetComponent<GridTile>();
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

		public void PlaceIfPossible(OwnableProp prop)
		{
			SetActiveGrids(false);
			if (tmpPreview != null) Destroy(tmpPreview);

			if (!IsAbleToPlace(prop))
			{
				ClearGrids();
				return;
			}

			// 전에 뒀던 곳 지우기
			for (int i = 0; i < gridTiles.GetLength(0); i++)
			{
				for (int j = 0; j < gridTiles.GetLength(1); j++)
				{
					if (gridTiles[i, j].prop == prop)
					{
						gridTiles[i, j].SetProp(null);
						gridTiles[i, j].SetMaterial(gridDefaultMaterial);
					}
				}
			}

			// 새로 둘 곳 check하기
			foreach (var tile in previouslyHighlighted)
				tile.SetProp(prop);

			prop.transform.position = GetAveragePosition();
			prop.transform.rotation = Quaternion.Euler(0f, wheelRotate * 90f, 0f);
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

		private void OnWheelRotate()
		{
			float scrollInput = Input.GetAxis("Mouse ScrollWheel");

			if (scrollInput > 0f)
			{
				wheelRotate = (wheelRotate + 1) % 4;
			}
			else if (scrollInput < 0f)
			{
				wheelRotate = (wheelRotate - 1 + 4) % 4;
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
			OnWheelRotate();
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

			Vector3 forward = playerTransform.forward;

			Vector2Int centerOffset = new Vector2Int((placeSize.x - 1) / 2, (placeSize.y - 1) / 2);
			Vector3 onY0 = GetMouseWorldPosOnY0();
			//Vector2Int startGridPos = WorldToGrid(onY0);

			//	Player 앞 대신 마우스 입력으로 변경	
			 Vector2Int startGridPos = WorldToGrid(playerTransform.position + forward * 2f) - centerOffset;
			//

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
				tmpPreview.transform.position = onY0;

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
