using Garage.Interfaces;
using Garage.Props;
using IUtil;
using System.Collections.Generic;
using UnityEngine;

namespace Garage.Manager
{
	public class BuildingManager : MonoBehaviour
	{
		[SerializeField] private GameObject dottedLinePrefab;
		[SerializeField] private Material defaultMaterial;
		[SerializeField] private Material occupiedMaterial;
		[SerializeField] private Material disabledMaterial;

		/** 게임 시작 시 Init **/
		private GridTile[,] gridTiles;
		private Vector2Int gridSize = new Vector2Int(5, 11);
		private Vector2Int gridOrigin = new Vector2Int(-20, 0);

		private HashSet<GridTile> previouslyHighlighted = new HashSet<GridTile>();
		private DottedLineRenderer dottedLine;

		// TODO - 게임 시작시 직접 스폰하도록
		// + 초기 건물들도 여기서 스폰
		[Button]
		public void OnGameStart()
		{
			gridTiles = FindObjectsByType<GridTile>(sortMode: FindObjectsSortMode.None)
				.ToDictionaryByPosition(gridSize, gridOrigin);

			dottedLine = Instantiate(dottedLinePrefab).GetComponent<DottedLineRenderer>();
			dottedLine.gameObject.SetActive(false);

			SetActiveGrids(false);
		}

		public void OnStageInit()
		{
			dottedLine.gameObject.SetActive(false);
		}

		private void ClearGrids()
		{
			for (int i = 0; i < gridTiles.GetLength(0); i++)
			{
				for (int j = 0; j < gridTiles.GetLength(1); j++)
				{
					gridTiles[i, j].SetMaterial(gridTiles[i, j].prop != null ? occupiedMaterial : defaultMaterial);
				}
			}
		}

		public void PlaceIfPossible(OwnableProp prop)
		{
			SetActiveGrids(false);
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
						gridTiles[i, j].SetMaterial(defaultMaterial);
					}
				}
			}

			// 새로 둘 곳 check하기
			foreach (var tile in previouslyHighlighted)
				tile.SetProp(prop);

			prop.transform.position = GetAveragePosition();
		}

		private bool IsAbleToPlace(OwnableProp prop)
		{
			dottedLine.gameObject.SetActive(false);

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

		public void UpdatePreviewArea(OwnableProp prop, Transform playerTransform)
		{
			if (!gridTiles[0, 0].gameObject.activeSelf)
			{
				SetActiveGrids(true);
			}

			IPlaceable placeable = prop.GetComponent<IPlaceable>();
			foreach (var tile in previouslyHighlighted)
			{ 
				tile.SetMaterial(tile.prop != null ? occupiedMaterial : defaultMaterial);
			}
			previouslyHighlighted.Clear();

			Vector2Int placeSize = placeable.GetSize();
			Vector3 forward = playerTransform.forward;

			Vector2Int centerOffset = new Vector2Int((placeSize.x - 1) / 2, (placeSize.y - 1) / 2);
			Vector2Int startGridPos = WorldToGrid(GetMouseWorldPosOnY0());

			/*	Player 앞 대신 마우스 입력으로 변경	
			 Vector2Int startGridPos = WorldToGrid(playerTransform.position + forward * 2f) - centerOffset;
			*/

			for (int x = 0; x < placeSize.x; x++)
			{
				for (int y = 0; y < placeSize.y; y++)
				{
					Vector2Int tilePos = startGridPos + new Vector2Int(x - centerOffset.x, y - centerOffset.y);
					Vector2Int index = tilePos - gridOrigin;

					if (IsInBounds(index))
					{
						GridTile tile = gridTiles[index.x, index.y];
						tile.SetMaterial(tile.IsPlaceable(prop) ? occupiedMaterial : disabledMaterial);
						previouslyHighlighted.Add(tile);
					}
				}
			}

			if (IsAbleToPlace(prop))
			{
				dottedLine.gameObject.SetActive(true);
				dottedLine.DrawDottedLine(prop.transform.position, GetAveragePosition());
			}
			else
			{
				dottedLine.gameObject.SetActive(false);
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

	}
}
